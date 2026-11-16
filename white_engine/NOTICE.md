# W.H.I.T.E. replacement engine — scope and provenance

This is **new in-repository OTS Security Suite code**, not a restored copy of
the deleted standalone OTS W.H.I.T.E. repository. It uses Python 3.12 standard
library only at runtime; PyInstaller is used only during Windows packaging.

**Supported in this preview:** local text source/config inspection for selected
credential patterns, metadata-only findings, a small set of heuristic SAST
patterns, optional read-only locally reachable Git blob history, bounded scan
budgets, explicit incomplete errors, and a JSON v1.0 contract.

**Not supported:** offline OSV/CVE dependency analysis, old WHITE's complete
rule corpus or proven detection parity, unreachable Git objects, remote refs,
Git LFS payloads, large/binary content analysis, and arbitrary language SAST.

An SCA request intentionally returns an incomplete report (exit 2), never a
false 'clean'. The suite UI disables the unavailable option for this preview.
The scanner never outputs matching credential values or source excerpts.

**Licensing:** The deleted WHITE source and any old third-party dependencies
are **not included**. This notice does not grant a redistribution license for
the suite; root distribution licensing remains a release-owner decision.
