"""OTS W.H.I.T.E. local source auditor (self-contained preview).

Read-only secret/SAST and reachable Git-history checks. This is a new engine,
not a recovery of the deleted standalone OTS W.H.I.T.E. implementation.
No external package execution, network calls, or automatic remediation.
"""
from __future__ import annotations

import argparse
import fnmatch
import json
import os
from pathlib import Path
import re
import stat
import subprocess
import sys

TOOL = "OTS W.H.I.T.E."
SCHEMA = "1.0"
MAX_FILES = 20000
MAX_FILE_BYTES = 2 * 1024 * 1024
MAX_TOTAL_BYTES = 128 * 1024 * 1024
MAX_HISTORY_BLOBS = 20000
MAX_HISTORY_BYTES = 128 * 1024 * 1024
MAX_FINDINGS = 500
SKIP_DIRS = frozenset({".git", ".hg", ".svn", ".venv", "venv", "node_modules", "__pycache__", ".tox", ".mypy_cache", ".pytest_cache", "bin", "obj", "dist", "build"})
CODE_EXTENSIONS = frozenset({".py", ".js", ".jsx", ".ts", ".tsx", ".go", ".cs", ".ps1", ".sh", ".rb", ".php"})
LEVELS = {"low": 1, "medium": 2, "high": 3, "critical": 4}

# Keep only finding metadata: never persist, print or return the matched value.
# This intentionally favors bounded, reviewable heuristics over a claim of full SAST.
RULES = (
    ("pem-private-key", "high", re.compile(r"-----BEGIN (?:RSA |EC |OPENSSH |DSA |ENCRYPTED )?PRIVATE KEY-----"), "secret"),
    ("aws-access-key", "high", re.compile(r"\b(?:AKIA|ASIA)[A-Z0-9]{16}\b"), "secret"),
    ("github-access-token", "high", re.compile(r"\b(?:gh[pousr]_[A-Za-z0-9_]{30,}|github_pat_[A-Za-z0-9_]{50,})\b"), "secret"),
    ("slack-access-token", "high", re.compile(r"\bxox[baprs]-[0-9A-Za-z-]{20,}\b"), "secret"),
    ("generic-credential-assignment", "high", re.compile(r"\b(?:api[_-]?key|secret[_-]?key|client[_-]?secret|access[_-]?token|password)\b\s*[:=]\s*['\"]?([A-Za-z0-9_+/=.-]{24,})", re.IGNORECASE), "secret"),
    ("python-eval", "medium", re.compile(r"\beval\s*\("), "sast"),
    ("python-exec", "medium", re.compile(r"\bexec\s*\("), "sast"),
    ("python-shell-true", "medium", re.compile(r"\bshell\s*=\s*True\b"), "sast"),
    ("python-pickle-load", "medium", re.compile(r"\bpickle\.(?:load|loads)\s*\("), "sast"),
    ("python-unsafe-yaml", "medium", re.compile(r"\byaml\.load\s*\("), "sast"),
    ("tls-verify-disabled", "medium", re.compile(r"\bverify\s*=\s*False\b"), "sast"),
)


def _excluded(path: str, patterns: list[str]) -> bool:
    return any(fnmatch.fnmatchcase(path, pat) or fnmatch.fnmatchcase(path.rsplit("/", 1)[-1], pat) for pat in patterns)


def _read_regular(path: Path) -> bytes | None:
    """Avoid symlink traversal; never execute or modify scanned content."""
    flags = os.O_RDONLY | getattr(os, "O_BINARY", 0) | getattr(os, "O_NOFOLLOW", 0)
    fd = os.open(path, flags)
    try:
        info = os.fstat(fd)
        if not stat.S_ISREG(info.st_mode):
            return None
        if info.st_size > MAX_FILE_BYTES:
            return None
        with os.fdopen(os.dup(fd), "rb") as stream:
            data = stream.read(MAX_FILE_BYTES + 1)
        return data if len(data) <= MAX_FILE_BYTES else None
    finally:
        os.close(fd)


def _text(data: bytes) -> str | None:
    if b"\x00" in data[:4096]:
        return None
    try:
        return data.decode("utf-8-sig")
    except UnicodeDecodeError:
        return None


def _scan_text(report: dict, text: str, path: str, *, history: bool = False, commit: str = "") -> None:
    for line_no, line in enumerate(text.splitlines(), 1):
        for rule_id, severity, pattern, kind in RULES:
            if kind == "sast" and (history or Path(path).suffix.lower() not in CODE_EXTENSIONS):
                continue
            if not pattern.search(line):
                continue
            # No source excerpts, matching values, or raw scanner output in reports.
            finding = {"rule_id": rule_id, "severity": severity, "path": path, "line": line_no, "category": kind}
            if history:
                finding["commit"] = commit[:12]
            report["findings"].append(finding)
            if len(report["findings"]) >= MAX_FINDINGS:
                raise RuntimeError("Finding count exceeded safe limit; audit incomplete")


def _worktree(root: Path, report: dict, patterns: list[str]) -> None:
    total = 0

    def on_walk_error(_error: OSError) -> None:
        # os.walk silently discards permission/I/O errors unless onerror is set.
        # Do not expose user paths or filesystem exception text in reports.
        report["unreadable_dirs"] += 1
        if "One or more directories could not be enumerated; audit incomplete" not in report["errors"]:
            report["errors"].append("One or more directories could not be enumerated; audit incomplete")

    for base, dirs, files in os.walk(root, topdown=True, followlinks=False, onerror=on_walk_error):
        base_path = Path(base)
        dirs[:] = sorted(d for d in dirs if d not in SKIP_DIRS and not (base_path / d).is_symlink() and not _excluded((base_path / d).relative_to(root).as_posix() + "/", patterns))
        for name in sorted(files):
            path = base_path / name
            relative = path.relative_to(root).as_posix()
            if _excluded(relative, patterns):
                continue
            # Count *all* in-scope file entries, even binary, symlinked, unreadable
            # and oversized files. Otherwise those files bypass the file budget.
            report["inventoried_files"] += 1
            if report["inventoried_files"] > MAX_FILES:
                raise RuntimeError("Worktree file inventory exceeded safe limit; audit incomplete")
            if path.is_symlink():
                report["skipped_files"] += 1
                continue
            try:
                data = _read_regular(path)
            except (OSError, PermissionError):
                report["errors"].append("Could not read a worktree file; audit incomplete")
                continue
            if data is None:
                report["skipped_files"] += 1
                continue
            total += len(data)
            if total > MAX_TOTAL_BYTES:
                raise RuntimeError("Worktree byte budget exceeded; audit incomplete")
            text = _text(data)
            if text is None:
                report["skipped_files"] += 1
                continue
            report["scanned_files"] += 1
            _scan_text(report, text, relative)


def _git(root: Path, args: list[str], *, timeout: int = 30, max_output: int = 4 * 1024 * 1024) -> bytes:
    env = dict(os.environ, GIT_OPTIONAL_LOCKS="0", GIT_NO_REPLACE_OBJECTS="1", GIT_TERMINAL_PROMPT="0", GIT_CONFIG_NOSYSTEM="1")
    env.pop("GIT_CONFIG_COUNT", None)
    proc = subprocess.run(["git", "-C", str(root), "--no-pager", *args], cwd=root, env=env,
                          capture_output=True, timeout=timeout, check=False)
    if proc.returncode != 0:
        raise RuntimeError("Git history could not be inspected; audit incomplete")
    if len(proc.stdout) > max_output:
        raise RuntimeError("Git history exceeded output budget; audit incomplete")
    return proc.stdout


def _history(root: Path, report: dict, patterns: list[str]) -> None:
    # --all includes locally reachable refs; orphaned objects/LFS payloads are out of scope.
    listing = _git(root, ["rev-list", "--objects", "--all"], timeout=60, max_output=8 * 1024 * 1024)
    total = 0
    for raw in listing.splitlines():
        if not raw:
            continue
        oid, _, path = raw.partition(b" ")
        if not re.fullmatch(rb"[0-9a-f]{40,64}", oid):
            raise RuntimeError("Git history inventory contained an invalid object id")
        if not path:
            continue
        name = path.decode("utf-8", "replace").replace("\\", "/")
        if _excluded(name, patterns) or any(part in SKIP_DIRS for part in name.split("/")):
            continue
        # Bound all traversed history entries, including oversized, binary
        # and non-blob objects, instead of counting only successfully scanned text.
        report["inventoried_history_objects"] += 1
        if report["inventoried_history_objects"] > MAX_HISTORY_BLOBS:
            raise RuntimeError("Git history object inventory exceeded safe limit; audit incomplete")
        kind = _git(root, ["cat-file", "-t", oid.decode("ascii")], max_output=128).strip()
        if kind != b"blob":
            continue
        size = _git(root, ["cat-file", "-s", oid.decode("ascii")], max_output=128).strip()
        if not size.isdigit():
            raise RuntimeError("Git blob size was invalid")
        length = int(size)
        if length > MAX_FILE_BYTES:
            report["skipped_history_blobs"] += 1
            continue
        total += length
        if total > MAX_HISTORY_BYTES:
            raise RuntimeError("Git history byte budget exceeded safe limit")
        blob = _git(root, ["cat-file", "blob", oid.decode("ascii")], max_output=MAX_FILE_BYTES + 1)
        text = _text(blob)
        if text is None:
            report["skipped_history_blobs"] += 1
            continue
        report["scanned_history_blobs"] += 1
        _scan_text(report, text, name, history=True, commit=oid.decode("ascii"))


def audit(root: Path, *, history: bool, no_sca: bool, patterns: list[str]) -> dict:
    report = {"tool": TOOL, "schema_version": SCHEMA, "complete": False,
              "checks": {"secrets": "completed", "sast": "completed", "history": "skipped", "sca": "skipped"},
              "errors": [], "findings": [], "scanned_files": 0, "skipped_files": 0,
              "inventoried_files": 0, "unreadable_dirs": 0,
              "scanned_history_blobs": 0, "skipped_history_blobs": 0,
              "inventoried_history_objects": 0,
              "limitations": ["Heuristic source audit only; not proof of safety", "Historical audit excludes unreachable Git objects, LFS and remote refs"]}
    try:
        if not root.is_dir():
            raise RuntimeError("Selected audit root is not a directory")
        _worktree(root, report, patterns)
        if report["unreadable_dirs"]:
            report["checks"]["secrets"] = "failed"
            report["checks"]["sast"] = "failed"
    except (OSError, RuntimeError) as exc:
        report["errors"].append(str(exc) if isinstance(exc, RuntimeError) else "Worktree inspection failed")
        report["checks"]["secrets"] = "failed"
        report["checks"]["sast"] = "failed"
    if history:
        try:
            _history(root, report, patterns)
            report["checks"]["history"] = "completed"
        except (OSError, RuntimeError, subprocess.TimeoutExpired) as exc:
            report["errors"].append(str(exc) if isinstance(exc, RuntimeError) else "Git history inspection failed")
            report["checks"]["history"] = "failed"
    if not no_sca:
        # An absent CVE feed is not a clean dependency audit. This preview never
        # downloads data, runs package managers, or silently skips an enabled check.
        report["checks"]["sca"] = "failed"
        report["errors"].append("Offline dependency CVE analysis is not bundled in this preview; disable SCA to run available checks")
    report["complete"] = not report["errors"] and all(v in ("completed", "skipped") for v in report["checks"].values())
    return report


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="OTS W.H.I.T.E. read-only local code security audit")
    parser.add_argument("root", type=Path)
    parser.add_argument("--format", choices=("json",), default="json")
    parser.add_argument("--history", action="store_true")
    parser.add_argument("--no-sca", action="store_true")
    parser.add_argument("--exclude", action="append", default=[])
    parser.add_argument("--fail-on", choices=("none", "low", "medium", "high", "critical"), default="high")
    args = parser.parse_args(argv)
    report = audit(args.root.resolve(), history=args.history, no_sca=args.no_sca, patterns=args.exclude)
    print(json.dumps(report, separators=(",", ":"), ensure_ascii=True))
    if not report["complete"]:
        return 2
    if args.fail_on != "none" and any(LEVELS.get(f["severity"], 0) >= LEVELS[args.fail_on] for f in report["findings"]):
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
