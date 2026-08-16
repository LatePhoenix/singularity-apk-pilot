Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- docs/SUPPORT\_RUNBOOK.md

\- .cursor/rules/testing-and-verification.mdc



Task:

Implement diagnostics bundle export.



Create:

\- IDiagnosticsService

\- DiagnosticsService

\- EnvironmentSnapshotService

\- LogcatCollector

\- ZipBundleWriter

\- DiagnosticBundleInfo

\- DTOs/schemas for diagnostics metadata



Requirements:

\- Export a zip bundle containing:

&#x20; - app version/build info

&#x20; - selected manifest/app id

&#x20; - connected device metadata

&#x20; - adb state snapshot

&#x20; - install attempt details

&#x20; - sanitized stdout/stderr

&#x20; - filtered logcat for the package if available

\- Avoid collecting unrelated device data.

\- Make bundle generation callable from install failure and completion screens.



After changes:

\- Build the solution.

\- Document bundle contents.

\- Note any redaction assumptions.

