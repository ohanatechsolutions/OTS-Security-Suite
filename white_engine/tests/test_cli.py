import contextlib
import io
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
from ots_white.cli import main

class WhiteAuditTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def report(self, *args):
        out = io.StringIO()
        with contextlib.redirect_stdout(out):
            exit_code = main([str(self.root), "--format", "json", *args])
        return exit_code, json.loads(out.getvalue())

    def test_benign_complete_and_schema(self):
        (self.root / "hello.py").write_text("print('hello')\n", encoding="utf-8")
        code, result = self.report("--no-sca")
        self.assertEqual(code, 0)
        self.assertTrue(result["complete"])
        self.assertEqual(result["scanned_files"], 1)
        self.assertEqual(result["findings"], [])
        self.assertEqual(result["checks"]["sca"], "skipped")

    def test_secret_redaction_and_threshold(self):
        secret = "AKIA" + "Z" * 16
        (self.root / "config.txt").write_text("value=" + secret + "\n", encoding="utf-8")
        code, result = self.report("--no-sca")
        self.assertEqual(code, 1)
        self.assertEqual(result["findings"][0]["rule_id"], "aws-access-key")
        self.assertNotIn(secret, json.dumps(result))
        self.assertEqual(self.report("--no-sca", "--fail-on", "none")[0], 0)

    def test_sast_metadata_only(self):
        (self.root / "code.py").write_text("eval(user_input)\n", encoding="utf-8")
        code, result = self.report("--no-sca")
        self.assertEqual(code, 0)
        self.assertEqual(result["findings"][0]["severity"], "medium")
        self.assertNotIn("user_input", json.dumps(result))

    def test_sca_selected_fails_closed(self):
        (self.root / "hello.py").write_text("print('ok')\n", encoding="utf-8")
        code, result = self.report()
        self.assertEqual(code, 2)
        self.assertFalse(result["complete"])
        self.assertEqual(result["checks"]["sca"], "failed")

    def test_history_non_git_incomplete(self):
        (self.root / "hello.py").write_text("print('ok')\n", encoding="utf-8")
        code, result = self.report("--no-sca", "--history")
        self.assertEqual(code, 2)
        self.assertFalse(result["complete"])
        self.assertEqual(result["checks"]["history"], "failed")

    def test_history_git_and_exclude(self):
        subprocess.run(["git", "init", "-q", str(self.root)], check=True)
        subprocess.run(["git", "-C", str(self.root), "config", "user.email", "synthetic@example.invalid"], check=True)
        subprocess.run(["git", "-C", str(self.root), "config", "user.name", "Synthetic Tester"], check=True)
        (self.root / "hello.py").write_text("print('ok')\n", encoding="utf-8")
        subprocess.run(["git", "-C", str(self.root), "add", "hello.py"], check=True)
        subprocess.run(["git", "-C", str(self.root), "commit", "-qm", "fixture"], check=True)
        code, result = self.report("--no-sca", "--history")
        self.assertEqual(code, 0)
        self.assertGreaterEqual(result["scanned_history_blobs"], 1)
        self.assertEqual(result["checks"]["history"], "completed")
        code, result = self.report("--no-sca", "--exclude", "*.py")
        self.assertEqual(code, 0)
        self.assertEqual(result["scanned_files"], 0)

    def test_symlink_and_binary_not_followed(self):
        (self.root / "data.bin").write_bytes(b"\x00" + b"AKIA" + b"Z" * 16)
        target = self.root / "outside.txt"
        target.write_text("AKIA" + "Z" * 16, encoding="utf-8")
        try:
            (self.root / "alias.txt").symlink_to(target)
        except (OSError, NotImplementedError):
            pass
        code, result = self.report("--no-sca", "--exclude", "outside.txt")
        self.assertEqual(code, 0)
        self.assertEqual(result["findings"], [])
        self.assertGreaterEqual(result["skipped_files"], 1)

    def test_bounded_findings_fail_closed(self):
        (self.root / "many.txt").write_text(("AKIA" + "Z" * 16 + "\n") * 501, encoding="utf-8")
        code, result = self.report("--no-sca")
        self.assertEqual(code, 2)
        self.assertFalse(result["complete"])
        self.assertTrue(result["errors"])

if __name__ == "__main__":
    unittest.main()
