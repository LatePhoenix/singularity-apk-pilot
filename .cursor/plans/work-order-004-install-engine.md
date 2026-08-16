Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- docs/MANIFEST\_SCHEMA.md

\- .cursor/rules/adb-and-process-rules.mdc



Task:

Implement install planning and install execution.



Create:

\- InstallManifest

\- InstallPolicy

\- InstallRequest

\- InstallPlan

\- InstallResult

\- InstallError

\- InstallService

\- InstallPlanner

\- InstallVerifier

\- PackageConflictService

\- PermissionGrantService



Requirements:

\- Support install policies:

&#x20; - InstallFresh

&#x20; - ReinstallKeepData

&#x20; - ReinstallAllowDowngrade

&#x20; - UninstallThenInstall

&#x20; - InstallTestBuild

\- Build adb install flags appropriately:

&#x20; - -r

&#x20; - -d

&#x20; - -t

&#x20; - -g

\- Verify app presence after install.

\- Keep package conflict logic separate from raw install execution.

\- Do not build split-APK/APKS support yet.



Tests:

\- Add unit tests for install plan generation by policy.

\- Add tests for flag combinations and verification logic.



After changes:

\- Build and run tests.

\- Summarize which install scenarios are now supported.

