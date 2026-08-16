Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- .cursor/rules/architecture-boundaries.mdc

\- .cursor/rules/adb-and-process-rules.mdc



Task:

Implement the first pass of adb infrastructure and abstractions.



Create:

\- IAdbClient

\- AdbClient

\- AdbProcessRunner

\- AdbCommandFactory

\- basic process result model

\- Result utility if needed



Requirements:

\- Support starting adb server.

\- Support listing devices with `adb devices -l`.

\- Support running `adb shell getprop <key>`.

\- Support apk install command execution with configurable flags.

\- Keep command building separate from process execution.

\- Return typed results, not just raw strings.

\- Add logging hooks/interfaces but keep implementation lightweight.



Tests:

\- Add unit tests for command construction.

\- Add parser tests only if parser is introduced in this work order.



After changes:

\- Build the solution.

\- Report exactly which adb commands are supported.

