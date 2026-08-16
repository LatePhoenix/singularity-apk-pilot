Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- .cursor/rules/wpf-ui-rules.mdc

\- docs/COPY\_DECK.md



Task:

Build the first-pass WPF shell and wizard pages.



Create:

\- ShellWindow

\- ShellViewModel

\- Wizard page views and viewmodels:

&#x20; - Welcome

&#x20; - ConnectDevice

&#x20; - DeviceDetected

&#x20; - Authorization

&#x20; - DeveloperMode

&#x20; - ReadyToInstall

&#x20; - Installing

&#x20; - InstallProblem

&#x20; - Complete



Requirements:

\- Use MVVM.

\- Keep code-behind minimal.

\- Display one primary action per page.

\- Add an optional help expander and advanced details region.

\- Bind page navigation to wizard state.

\- Use placeholder styling if needed, but make the flow usable.



Do not:

\- Spend time on final visual polish.

\- Hardcode adb logic into the UI layer.



After changes:

\- Build the solution.

\- Describe how page switching works.

\- Note any TODOs for styling/accessibility refinement.

