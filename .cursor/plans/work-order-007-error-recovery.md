Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- .cursor/rules/adb-and-process-rules.mdc

\- docs/SUPPORT\_RUNBOOK.md



Task:

Implement error classification and suggested recovery actions.



Create:

\- RecoveryAction

\- IRecoveryService

\- RecoveryService

\- ErrorClassifier

\- RetryPolicyFactory

\- AutoFixExecutor

\- FriendlyMessageService



Requirements:

\- Map raw adb/package output into typed install errors.

\- Handle at least:

&#x20; - device unauthorized

&#x20; - device offline

&#x20; - no devices found

&#x20; - install failed version downgrade

&#x20; - already exists

&#x20; - signature mismatch

&#x20; - insufficient storage

&#x20; - likely developer mode missing

&#x20; - cable/usb mode issue

\- Return up to 3 user-facing actions max per failure.

\- Include one-click automatic fixes where safe, such as restarting adb server or retrying install with a valid alternate policy.



Tests:

\- Add classifier tests for sample adb/package outputs.

\- Add recovery action tests for each major error category.



After changes:

\- Build and run tests.

\- Summarize the final error taxonomy.

