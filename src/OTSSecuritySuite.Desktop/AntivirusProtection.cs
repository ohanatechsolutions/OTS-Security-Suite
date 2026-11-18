using System.Runtime.InteropServices;

namespace OTSSecuritySuite.Desktop;

/// <summary>
/// Read-only interpretation of antivirus products registered with Windows Security Center.
/// "Protected" here means registered, active antivirus with current signatures;
/// it is not a guarantee that the machine is free of malware.
/// </summary>
public enum AntivirusCoverage { Protected, NeedsAttention, Unprotected, Unknown }
public enum AntivirusRuntimeState { On = 0, Off = 1, Snoozed = 2, Expired = 3, Unknown = 4 }

public sealed record RegisteredAntivirus(string Name, AntivirusRuntimeState State, bool SignaturesCurrent);
public sealed record AntivirusAssessment(
    AntivirusCoverage Coverage, IReadOnlyList<RegisteredAntivirus> Products, string Summary, string Guidance)
{
    public bool ActiveDefender => Products.Any(p => p.State == AntivirusRuntimeState.On &&
        (p.Name.Contains("Microsoft Defender", StringComparison.OrdinalIgnoreCase) ||
         p.Name.Contains("Windows Defender", StringComparison.OrdinalIgnoreCase)));

    public static AntivirusAssessment Evaluate(IReadOnlyList<RegisteredAntivirus> products, bool inventorySucceeded)
    {
        if (!inventorySucceeded)
            return new(AntivirusCoverage.Unknown, products,
                "UNKNOWN · Antivirus status could not be verified",
                "Windows Security Center did not return reliable information. Open Windows Security to check protection manually.");
        var active = products.Where(p => p.State == AntivirusRuntimeState.On).ToArray();
        if (active.Any(p => p.SignaturesCurrent))
            return new(AntivirusCoverage.Protected, products,
                "PROTECTED · Active antivirus detected",
                "Real-time antivirus is reported as on with current signatures. This is not a guarantee that the computer is threat-free.");
        if (active.Length != 0)
            return new(AntivirusCoverage.NeedsAttention, products,
                "ATTENTION · Antivirus active but signatures outdated",
                "Update antivirus definitions using the registered provider's application.");
        if (products.Count == 0)
            return new(AntivirusCoverage.Unprotected, products,
                "UNPROTECTED · No antivirus registered",
                "Microsoft Defender Antivirus is built into Windows 10/11. Open Windows Security to enable or repair Virus & threat protection; do not download an unofficial installer.");
        if (products.Any(p => p.State == AntivirusRuntimeState.Unknown))
            return new(AntivirusCoverage.Unknown, products,
                "UNKNOWN · Installed antivirus status is unavailable",
                "At least one antivirus is registered but its current state cannot be verified. Open Windows Security.");
        return new(AntivirusCoverage.Unprotected, products,
            "UNPROTECTED · Installed antivirus is not running",
            "Your installed antivirus is off, snoozed or expired. Open Windows Security or your antivirus application to restore protection.");
    }
}

/// <summary>
/// Calls the supported Windows Security Center COM product list (IWSCProductList).
/// No WMI productState bit decoding, registry guessing, service toggling or AV installation.
/// Must be called from an STA thread (the WPF UI thread).
/// </summary>
public static class WindowsAntivirusInventory
{
    private static readonly Guid ProductListClsid = new("17072F7B-9ABE-4A74-A261-1EB76B55107A");
    private const int AntivirusProvider = 4; // WSC_SECURITY_PROVIDER_ANTIVIRUS

    public static AntivirusAssessment Read()
    {
        if (!OperatingSystem.IsWindows())
            return AntivirusAssessment.Evaluate(Array.Empty<RegisteredAntivirus>(), false);
        object? instance = null;
        var products = new List<RegisteredAntivirus>();
        try
        {
            var type = Type.GetTypeFromCLSID(ProductListClsid, throwOnError: true)!;
            instance = Activator.CreateInstance(type) ?? throw new COMException("Windows Security Center is unavailable.");
            dynamic list = instance;
            list.Initialize(AntivirusProvider);
            int count = list.Count;
            if (count < 0 || count > 100)
                throw new InvalidOperationException("Invalid antivirus provider count.");
            for (int i = 0; i < count; i++)
            {
                object? productObject = null;
                try
                {
                    productObject = list.Item(i);
                    dynamic product = productObject!;
                    string name = (string)product.ProductName;
                    int rawState = (int)product.ProductState;
                    int signature = (int)product.SignatureStatus; // 1=current; 0=outdated
                    var state = Enum.IsDefined(typeof(AntivirusRuntimeState), rawState)
                        ? (AntivirusRuntimeState)rawState : AntivirusRuntimeState.Unknown;
                    products.Add(new RegisteredAntivirus(string.IsNullOrWhiteSpace(name) ? "Unnamed antivirus" : name.Trim(),
                        state, signature == 1));
                }
                finally
                {
                    if (productObject is not null && Marshal.IsComObject(productObject))
                        Marshal.FinalReleaseComObject(productObject);
                }
            }
            return AntivirusAssessment.Evaluate(products, true);
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException or
                                   System.Runtime.InteropServices.ExternalException or
                                   Microsoft.CSharp.RuntimeBinder.RuntimeBinderException or
                                   System.Reflection.TargetInvocationException or ArgumentException)
        {
            // Partial inventories are not authoritative. Never report a false "protected" state.
            return AntivirusAssessment.Evaluate(Array.Empty<RegisteredAntivirus>(), false);
        }
        finally
        {
            if (instance is not null && Marshal.IsComObject(instance))
                Marshal.FinalReleaseComObject(instance);
        }
    }
}
