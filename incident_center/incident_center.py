"""Read-only OpenCanary JSONL intake for OTS Security Suite.

No network listeners, remediation, privileged commands or external services.
"""
from __future__ import annotations

import argparse
import hashlib
import ipaddress
import json
import os
from pathlib import Path
import sqlite3
import sys

MAX_LINE = 65536
SCHEMA = """CREATE TABLE IF NOT EXISTS incidents (
    fingerprint TEXT PRIMARY KEY,
    observed_at TEXT NOT NULL,
    source_ip TEXT NOT NULL,
    destination_ip TEXT NOT NULL,
    event_type INTEGER NOT NULL,
    severity TEXT NOT NULL
)"""


def _ip(value: object) -> str:
    if not isinstance(value, str):
        raise ValueError("IP address must be a string")
    return str(ipaddress.ip_address(value))


def parse_event(raw: bytes) -> tuple[str, str, str, int, str, str]:
    """Validate minimal OpenCanary fields without storing sensitive logdata."""
    if not raw or len(raw) > MAX_LINE:
        raise ValueError("empty or oversized record")
    try:
        event = json.loads(raw)
    except (ValueError, UnicodeDecodeError) as exc:
        raise ValueError("invalid JSON") from exc
    if not isinstance(event, dict):
        raise ValueError("expected JSON object")
    timestamp = event.get("local_time")
    if not isinstance(timestamp, str) or not 1 <= len(timestamp) <= 80:
        raise ValueError("invalid local_time")
    kind = event.get("logtype")
    if isinstance(kind, bool) or not isinstance(kind, int) or not 0 <= kind <= 999999:
        raise ValueError("invalid logtype")
    src = _ip(event.get("src_host"))
    dst = _ip(event.get("dst_host"))
    # All interactions with decoy services merit review, not automatic blocking.
    fingerprint = hashlib.sha256(raw.rstrip(b"\r\n")).hexdigest()
    return fingerprint, timestamp, src, dst, kind, "review"


def connect(db_path: Path) -> sqlite3.Connection:
    db_path = db_path.expanduser()
    if not db_path.parent.is_dir():
        raise FileNotFoundError("Database directory does not exist")
    if db_path.is_symlink():
        raise ValueError("Database path must not be a symbolic link")
    old_umask = os.umask(0o077) if os.name == "posix" else None
    try:
        db = sqlite3.connect(str(db_path), timeout=5)
    finally:
        if old_umask is not None:
            os.umask(old_umask)
    db.execute(SCHEMA)
    db.commit()
    return db


def ingest(log_path: Path, db_path: Path) -> dict[str, int]:
    stats = {"accepted": 0, "duplicates": 0, "rejected": 0}
    with connect(db_path) as db, log_path.open("rb") as log:
        for line in log:
            try:
                event = parse_event(line)
            except ValueError:
                stats["rejected"] += 1
                continue
            cursor = db.execute(
                "INSERT OR IGNORE INTO incidents VALUES (?, ?, ?, ?, ?, ?)", event
            )
            stats["accepted" if cursor.rowcount else "duplicates"] += 1
        db.commit()
    return stats


def recent(db_path: Path, limit: int) -> list[dict[str, object]]:
    if not 1 <= limit <= 100:
        raise ValueError("limit must be between 1 and 100")
    with connect(db_path) as db:
        rows = db.execute(
            "SELECT observed_at, source_ip, destination_ip, event_type, severity "
            "FROM incidents ORDER BY rowid DESC LIMIT ?", (limit,)
        ).fetchall()
    return [dict(zip(("observed_at", "source_ip", "destination_ip", "event_type", "severity"), row)) for row in rows]


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Local OpenCanary incident intake (read-only)")
    parser.add_argument("--db", required=True, type=Path, help="Local SQLite database path")
    sub = parser.add_subparsers(dest="command", required=True)
    imp = sub.add_parser("import", help="Import an existing OpenCanary JSONL log")
    imp.add_argument("log", type=Path)
    show = sub.add_parser("recent", help="Display recent incident metadata")
    show.add_argument("--limit", type=int, default=20)
    args = parser.parse_args(argv)
    try:
        result = ingest(args.log, args.db) if args.command == "import" else recent(args.db, args.limit)
    except (OSError, sqlite3.Error, ValueError) as exc:
        print(f"incident-center: {type(exc).__name__}: {exc}", file=sys.stderr)
        return 2
    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
