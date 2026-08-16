Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- docs/PRODUCT\_SPEC.md

\- docs/COPY\_DECK.md

\- .cursor/rules/architecture-boundaries.mdc



Task:

Implement the wizard state machine and flow strategies.



Create:

\- WizardStep

\- WizardState

\- IWizardFlowService

\- WizardFlowService

\- FlowDecisionEngine

\- QuestFlowStrategy

\- AndroidPhoneFlowStrategy



Requirements:

\- Model a deterministic step flow:

&#x20; Welcome -> ConnectDevice -> DeviceDetected -> Authorization/DeveloperMode if needed -> ReadyToInstall -> Installing -> InstallProblem or Complete

\- Skip steps automatically when state already satisfies prerequisites.

\- Branch early between Quest and Android phone flows.

\- Return friendly UI-oriented step models, not raw error text.

\- Keep flow logic fully testable and UI-agnostic.



Tests:

\- Add flow tests for:

&#x20; - Quest unauthorized

&#x20; - Quest authorized but not ready

&#x20; - Quest ready

&#x20; - Android phone unauthorized

&#x20; - Android phone ready

&#x20; - install failed path

&#x20; - install success path



After changes:

\- Build and run tests.

\- Summarize all implemented wizard transitions.

