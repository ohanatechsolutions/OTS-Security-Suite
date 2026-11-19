using System.Runtime.InteropServices;
using System.Windows;

namespace OTSSecuritySuite.Hub;

public partial class App : Application
{
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SetCurrentProcessExplicitAppUserModelID(string appID);

    protected override void OnStartup(StartupEventArgs e)
    {
        if (OperatingSystem.IsWindows())
            _ = SetCurrentProcessExplicitAppUserModelID("OhanaTechSolutions.OTSSecuritySuite");
        base.OnStartup(e);
    }
}
