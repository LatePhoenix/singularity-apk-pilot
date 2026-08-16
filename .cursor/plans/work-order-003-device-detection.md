Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- docs/PRODUCT\_SPEC.md

\- .cursor/rules/adb-and-process-rules.mdc

\- .cursor/rules/testing-and-verification.mdc



Task:

Implement device detection and classification.



Create:

\- DeviceInfo

\- DeviceKind

\- DeviceConnectionState

\- DeviceDetectionService

\- DeviceClassificationService

\- DevicePropertyService

\- DeviceMonitorService



Requirements:

\- Parse `adb devices -l` output into typed device records.

\- Distinguish:

&#x20; - no device

&#x20; - unauthorized

&#x20; - offline

&#x20; - connected ready

\- Classify likely Meta Quest vs Android phone using manufacturer/model properties.

\- Retrieve manufacturer, model, and Android version via getprop when possible.

\- Expose a poll-based monitoring API suitable for WPF binding.



Tests:

\- Add parser fixtures for realistic adb devices outputs.

\- Add classification tests for Quest 2, Quest 3, Pixel, Samsung, unauthorized device, offline device.



After changes:

\- Build and run tests.

\- Summarize edge cases not yet handled.

