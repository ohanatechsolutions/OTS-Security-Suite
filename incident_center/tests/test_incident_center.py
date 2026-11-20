import json
import os
from pathlib import Path
import sys
import tempfile
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from incident_center import MAX_LINE, ingest, parse_event, recent, main


def event(**changes):
    data = {"local_time": "2026-11-20 10:00:00", "logtype": 1000,
            "src_host": "192.0.2.9", "dst_host": "192.0.2.10",
            "logdata": {"PASSWORD": "DO_NOT_STORE", "USERNAME": "private"}}
    data.update(changes)
    return json.dumps(data).encode() + b"\n"


class IntakeTests(unittest.TestCase):
    def test_parse_omits_credentials(self):
        result = parse_event(event())
        self.assertEqual(result[2], "192.0.2.9")
        self.assertNotIn("DO_NOT_STORE", repr(result))

    def test_invalid_and_oversized(self):
        for value in [b"", b"not json", b"[]", b"x" * (MAX_LINE + 1),
                      event(src_host="bad"), event(logtype="1000"), event(local_time="")]:
            with self.subTest(value=value[:40]), self.assertRaises(ValueError):
                parse_event(value)

    def test_dedupe_persistence_and_no_secrets(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            log = root / "opencanary.log"
            db = root / "incidents.sqlite"
            log.write_bytes(event() + event() + b"bad\n")
            self.assertEqual(ingest(log, db), {"accepted": 1, "duplicates": 1, "rejected": 1})
            self.assertEqual(ingest(log, db), {"accepted": 0, "duplicates": 2, "rejected": 1})
            self.assertEqual(len(recent(db, 20)), 1)
            self.assertNotIn(b"DO_NOT_STORE", db.read_bytes())
            if os.name == "posix":
                self.assertEqual(db.stat().st_mode & 0o077, 0)

    def test_oversized_log_line_is_one_rejection(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            log = root / "events.jsonl"
            log.write_bytes(b"x" * (MAX_LINE * 3) + b"\n" + event())
            self.assertEqual(ingest(log, root / "events.sqlite"),
                             {"accepted": 1, "duplicates": 0, "rejected": 1})

    def test_cli_and_limit(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            log = root / "events.jsonl"
            log.write_bytes(event())
            db = root / "events.sqlite"
            self.assertEqual(main(["--db", str(db), "import", str(log)]), 0)
            self.assertEqual(main(["--db", str(db), "recent", "--limit", "1"]), 0)
            self.assertEqual(main(["--db", str(db), "recent", "--limit", "1000"]), 2)


if __name__ == "__main__":
    unittest.main()
