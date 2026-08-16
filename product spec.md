<img src="https://r2cdn.perplexity.ai/pplx-full-logo-primary-dark%402x.png" style="height:64px;margin-right:32px"/>

# Yes, give me the full product spec and C\# project structure

Yes. Below is the full product spec and a C\# project structure optimized for a Windows-first, Quest-first, novice-proof tester installer. The design assumption is that Quest installation requires developer mode plus USB-debugging authorization, and Android installation benefits from ADB automation but must degrade gracefully to simpler flows when users are unwilling or unable to enable debugging.[^1][^2][^3]

## Product spec

**Product name placeholder:** `Singularity Tester Installer` or `Sideload Concierge`. The product goal is to let a non-technical tester connect a Quest headset or Android phone, follow a linear wizard, and install your app with minimal ambiguity and near-zero support contact.[^4][^5]

### Core goals

- One obvious next action at every step.
- Automatic device classification: Quest 2/3 vs Android phone.
- Human-readable error handling for common ADB and package failures.
- App-specific install manifests so the same shell can deploy multiple test apps.
- Built-in recovery, diagnostics, and support export.


### Non-goals

- General-purpose Android file manager.
- Device debloater.
- Store replacement.
- Complex developer console surfaced to testers.

Observation: adding SideQuest-class breadth would degrade usability. Your product wins by removing options, not by adding them.[^5][^6]

## User flows

### Primary flow

1. Launch app.
2. Click `Start`.
3. Connect device with USB.
4. Tool detects device class and current readiness.
5. Tool guides user through only the missing prerequisites.
6. Tool installs the correct build.
7. Tool validates install and offers launch instructions.
8. Tool shows green completion screen with optional diagnostics export.

This should be a state machine, not a free-navigation dashboard, because novices perform better with constrained progress and fewer branching decisions.

### Quest flow

Quest sideloading requires developer mode and USB debugging approval before deployment can succeed. Meta’s current setup docs instruct users to enable developer mode in the Meta Horizon app, then accept the USB debugging prompt in-headset and ideally select “Always allow from this computer.”[^1][^2]

Flow:

1. Detect Quest model via `adb devices -l` and properties.
2. If no device, show cable/connectivity screen.
3. If unauthorized, show “Put on headset now” screen with image cues.
4. If developer mode likely absent, show exact Meta mobile app path.
5. Once authorized, install APK.
6. Verify package presence.
7. Show “Find it under Unknown Sources” instructions, noting Quest UI placement can vary and depends on sideloaded app presence.[^7][^8][^9][^1]

### Android phone flow

ADB can install APKs directly, with `-r` for reinstall, `-d` for version downgrade, `-t` for test APKs, and `-g` to grant manifest permissions where appropriate. That means your installer can cover most test-build churn without making users uninstall manually each time.[^10][^11][^3]

Flow:

1. Detect Android phone.
2. Check authorization state.
3. If unauthorized, instruct user to unlock phone and accept USB debugging.
4. Install using a policy-based flag set.
5. Verify package.
6. Offer launch or completion instructions.

Error: assuming all phones should use identical prompts. Samsung and Pixel users encounter similar ADB authorization concepts, but manufacturer UI wording and USB mode defaults differ enough that your copy should support vendor-specific variants when detected.[^3]

## Functional requirements

### 1. Device detection

The app must:

- Poll ADB server for attached devices.
- Parse `adb devices -l`.
- Retrieve model, manufacturer, serial, Android version, and headset/phone classification.
- Distinguish states: `NotConnected`, `Unauthorized`, `Offline`, `ConnectedReady`, `BusyInstalling`, `InstallFailed`, `Installed`.

Detection sources:

- `adb devices -l`
- `adb shell getprop ro.product.manufacturer`
- `adb shell getprop ro.product.model`
- `adb shell getprop ro.build.version.release`

ADB is the canonical control path for install and inspection, so your app should treat it as the single system of record for connection state.[^3]

### 2. Readiness engine

Checks:

- ADB binary available and launchable.
- ADB server start/restart health.
- Device physically connected.
- Device authorized.
- For Quest: developer mode expectation and USB debugging prompt completion.
- Existing package installed or absent.
- Signature/version collision risk.
- Free storage estimate.
- APK compatibility basic checks.

Common install collisions include version downgrade and existing-package cases; ADB supports reinstall and downgrade flags, but signature mismatches may still require uninstall-first handling.[^11][^10][^3]

### 3. Install engine

Support these policies per app manifest:

- `InstallFresh`
- `ReinstallKeepData`
- `ReinstallAllowDowngrade`
- `UninstallThenInstall`
- `InstallTestBuild`
- `GrantRuntimePermissions`

Canonical command patterns:

- `adb install app.apk`
- `adb install -r app.apk`
- `adb install -r -d app.apk`
- `adb install -r -t app.apk`
- `adb install -r -g app.apk`[^10][^3]

Initial scope should be single-APK only. Split APK/APKS support can be Phase 2 because it increases parser complexity and multiplies failure modes.[^5][^3]

### 4. Guided UI content

Every step screen should contain:

- One action headline.
- One sentence of explanation.
- One highlighted primary button.
- Device-specific illustration or screenshot.
- “Why am I seeing this?” expandable help.
- Hidden advanced details for you.

Example Quest prompt:

- Headline: `Put on your headset now`
- Body: `A permission message is waiting inside the headset. Select “Always allow from this computer,” then choose Allow.`[^1]


### 5. Recovery engine

Provide one-click actions for:

- Restart ADB server.
- Retry detection.
- Switch install mode.
- Uninstall old build.
- Retry install with downgrade.
- Export diagnostics.
- Show cable troubleshooting.
- Show vendor-specific unlock guidance.

This matters because ADB failure is often procedural, not catastrophic; the product should translate low-level install errors into deterministic recovery actions.[^11][^3]

### 6. Validation and completion

After install:

- Confirm package exists via `pm list packages`.
- Optionally read installed version.
- Offer “Open app now” where possible.
- For Quest, show where to find sideloaded content under Unknown Sources, accounting for UI movement between updates.[^9][^12][^7]


### 7. Diagnostics bundle

Export ZIP should include:

- App manifest ID and version.
- Device metadata.
- ADB state snapshot.
- Install command attempted.
- Sanitized stderr/stdout.
- Filtered `logcat` for your package.
- Timestamp and app version.

That bundle reduces support ping-pong and turns “it didn’t work” into actionable evidence.

## App manifest spec

Define each distributed app via JSON. That gives you a reusable shell and avoids hardcoding one installer per project.

```json
{
  "appId": "com.singularity.exampleapp",
  "displayName": "Example App",
  "buildVersion": "0.9.3-test7",
  "apkPath": "payloads/example-app.apk",
  "targetPlatforms": ["quest", "android"],
  "installPolicy": "ReinstallAllowDowngrade",
  "grantPermissions": true,
  "allowTestApk": true,
  "launchAfterInstall": false,
  "preferredDeviceFamilies": ["meta-quest-2", "meta-quest-3", "pixel", "samsung"],
  "postInstallNotes": {
    "quest": [
      "Open Library.",
      "Open the filter menu.",
      "Select Unknown Sources.",
      "Launch Example App."
    ],
    "android": [
      "Find Example App in your app drawer and open it."
    ]
  },
  "support": {
    "contactLabel": "Send diagnostics to Matt",
    "contactEmail": "support@example.com"
  }
}
```


## Screen-by-screen spec

### 1. Welcome

Purpose: set expectation.
UI:

- Product name
- Target app name/version
- `Start` button
- Secondary: `Advanced`

Copy:

- `This tool installs Example App on your headset or phone. You only need a USB cable and about two minutes.`


### 2. Connect device

Purpose: reduce ambiguity before detection.
UI:

- USB illustration
- `I connected it`
- Auto-refresh status
- Optional “Show cable tips”

Rules:

- Stay on screen until device seen.
- Show vendor/model when detected.


### 3. Device detected

Purpose: branch early.
UI:

- Large device card
- `Meta Quest 3 detected` or `Pixel 9 detected`
- `Continue`

Behavior:

- Branch to Quest prerequisites or Android prerequisites based on classification.


### 4. Authorization required

Purpose: resolve `unauthorized`.
Quest copy should tell users to put on the headset and approve the prompt; phone copy should tell them to unlock the device and approve USB debugging. Meta’s docs explicitly recommend “Always allow from this computer” for Quest.[^1]

### 5. Enable developer mode

Quest-only.
Purpose: recover from absent sideload readiness.
UI:

- Ordered instructions
- Mobile app path
- Optional QR link to help page
- `I turned it on`

Meta’s current developer docs specify enabling developer mode through the Meta Horizon mobile app for the headset.[^2][^1]

### 6. Ready to install

Purpose: final confirmation without jargon.
UI:

- App card
- Device card
- Install mode summary
- `Install now`


### 7. Installing

Purpose: reassure and block premature interaction.
UI:

- Progress steps, not raw percentage unless you can compute it accurately
- Current action label: `Sending app`, `Installing`, `Verifying`
- Secondary: `Cancel`


### 8. Install problem

Purpose: deterministic recovery.
UI:

- Plain-English error title
- Likely cause
- 1–3 recommended actions max
- `Try automatic fix`
- `Export diagnostics`

Examples:

- `An older incompatible version is already installed.`
- `Your device has not approved this computer yet.`
- `The device disconnected during installation.`


### 9. Complete

Purpose: close the loop.
UI:

- Green success state
- Device-specific launch instructions
- `Done`
- `Export diagnostics`
- Optional `Install another app`

Quest completion must explain that the app may appear under Unknown Sources, which Meta and community guidance both reinforce.[^12][^7][^9]

## Error taxonomy

Map raw ADB/package errors to friendly categories:

- `UnauthorizedDevice`
- `OfflineDevice`
- `NoDevicesFound`
- `VersionDowngrade`
- `PackageAlreadyExists`
- `SignatureMismatch`
- `InsufficientStorage`
- `DebuggingNotApproved`
- `DeveloperModeLikelyDisabled`
- `CableOrUsbModeIssue`
- `UnknownInstallFailure`

Examples:

- `INSTALL_FAILED_VERSION_DOWNGRADE` → suggest retry with downgrade policy or uninstall-first. ADB supports `-d` for downgrade installs.[^10][^11][^3]
- `device unauthorized` → show approval flow.
- `more than one device/emulator` → show device picker.
- `INSTALL_PARSE_FAILED...` → APK likely bad or incompatible; export diagnostics.


## Packaging model

There are two viable deployment modes:

### Mode A: Dedicated installer per app

You generate one Windows package that contains:

- The installer shell
- Portable ADB
- One app manifest
- One APK payload

Pros:

- Simplest tester experience
- Lowest cognitive load
- Best for external testing waves


### Mode B: Multi-app tester hub

One shell, multiple payload manifests.
Pros:

- Easier for recurring internal testers
- Reusable across projects
Cons:
- More UI complexity
- Higher chance of user picking the wrong build

Recommendation: start with Mode A. Error: premature platform-generalization. The winning move is a single-purpose installer per test build family, not a universal app portal on day one.

## C\# project structure

Use .NET 8 with WPF and MVVM. Your natural implementation fit is C\#, and WPF gives you fast desktop iteration plus good packaging options.

```text
SingularityTesterInstaller/
├─ SingularityTesterInstaller.sln
├─ src/
│  ├─ Installer.App/
│  │  ├─ App.xaml
│  │  ├─ App.xaml.cs
│  │  ├─ Bootstrap/
│  │  │  ├─ ServiceRegistration.cs
│  │  │  ├─ AppBootstrapper.cs
│  │  │  └─ ThemeBootstrapper.cs
│  │  ├─ Resources/
│  │  │  ├─ Styles/
│  │  │  │  ├─ Colors.xaml
│  │  │  │  ├─ Typography.xaml
│  │  │  │  ├─ Buttons.xaml
│  │  │  │  ├─ Cards.xaml
│  │  │  │  ├─ Wizard.xaml
│  │  │  │  └─ Icons.xaml
│  │  │  ├─ Images/
│  │  │  └─ Strings/
│  │  │     ├─ en-US.xaml
│  │  │     └─ Troubleshooting.en-US.xaml
│  │  ├─ Views/
│  │  │  ├─ ShellWindow.xaml
│  │  │  ├─ ShellWindow.xaml.cs
│  │  │  ├─ Pages/
│  │  │  │  ├─ WelcomePage.xaml
│  │  │  │  ├─ ConnectDevicePage.xaml
│  │  │  │  ├─ DeviceDetectedPage.xaml
│  │  │  │  ├─ AuthorizationPage.xaml
│  │  │  │  ├─ DeveloperModePage.xaml
│  │  │  │  ├─ ReadyToInstallPage.xaml
│  │  │  │  ├─ InstallingPage.xaml
│  │  │  │  ├─ InstallProblemPage.xaml
│  │  │  │  ├─ CompletePage.xaml
│  │  │  │  ├─ DevicePickerDialog.xaml
│  │  │  │  └─ AdvancedPanel.xaml
│  │  ├─ ViewModels/
│  │  │  ├─ ShellViewModel.cs
│  │  │  ├─ Wizard/
│  │  │  │  ├─ WelcomePageViewModel.cs
│  │  │  │  ├─ ConnectDevicePageViewModel.cs
│  │  │  │  ├─ DeviceDetectedPageViewModel.cs
│  │  │  │  ├─ AuthorizationPageViewModel.cs
│  │  │  │  ├─ DeveloperModePageViewModel.cs
│  │  │  │  ├─ ReadyToInstallPageViewModel.cs
│  │  │  │  ├─ InstallingPageViewModel.cs
│  │  │  │  ├─ InstallProblemPageViewModel.cs
│  │  │  │  └─ CompletePageViewModel.cs
│  │  │  └─ DesignTime/
│  │  ├─ Converters/
│  │  └─ Behaviors/
│  │
│  ├─ Installer.Core/
│  │  ├─ Abstractions/
│  │  │  ├─ IAdbClient.cs
│  │  │  ├─ IDeviceService.cs
│  │  │  ├─ IInstallService.cs
│  │  │  ├─ IWizardFlowService.cs
│  │  │  ├─ IManifestService.cs
│  │  │  ├─ IDiagnosticsService.cs
│  │  │  ├─ IRecoveryService.cs
│  │  │  ├─ IContentService.cs
│  │  │  ├─ ILogger.cs
│  │  │  └─ IClock.cs
│  │  ├─ Models/
│  │  │  ├─ DeviceInfo.cs
│  │  │  ├─ DeviceKind.cs
│  │  │  ├─ DeviceConnectionState.cs
│  │  │  ├─ InstallManifest.cs
│  │  │  ├─ InstallPolicy.cs
│  │  │  ├─ InstallRequest.cs
│  │  │  ├─ InstallResult.cs
│  │  │  ├─ InstallError.cs
│  │  │  ├─ WizardStep.cs
│  │  │  ├─ WizardState.cs
│  │  │  ├─ RecoveryAction.cs
│  │  │  └─ DiagnosticBundleInfo.cs
│  │  ├─ Services/
│  │  │  ├─ Adb/
│  │  │  │  ├─ AdbClient.cs
│  │  │  │  ├─ AdbProcessRunner.cs
│  │  │  │  ├─ AdbOutputParser.cs
│  │  │  │  └─ AdbCommandFactory.cs
│  │  │  ├─ Devices/
│  │  │  │  ├─ DeviceDetectionService.cs
│  │  │  │  ├─ DeviceClassificationService.cs
│  │  │  │  ├─ DevicePropertyService.cs
│  │  │  │  └─ DeviceMonitorService.cs
│  │  │  ├─ Install/
│  │  │  │  ├─ InstallService.cs
│  │  │  │  ├─ InstallPlanner.cs
│  │  │  │  ├─ InstallVerifier.cs
│  │  │  │  ├─ PermissionGrantService.cs
│  │  │  │  └─ PackageConflictService.cs
│  │  │  ├─ Flow/
│  │  │  │  ├─ WizardFlowService.cs
│  │  │  │  ├─ QuestFlowStrategy.cs
│  │  │  │  ├─ AndroidPhoneFlowStrategy.cs
│  │  │  │  └─ FlowDecisionEngine.cs
│  │  │  ├─ Recovery/
│  │  │  │  ├─ RecoveryService.cs
│  │  │  │  ├─ ErrorClassifier.cs
│  │  │  │  ├─ RetryPolicyFactory.cs
│  │  │  │  └─ AutoFixExecutor.cs
│  │  │  ├─ Diagnostics/
│  │  │  │  ├─ DiagnosticsService.cs
│  │  │  │  ├─ LogcatCollector.cs
│  │  │  │  ├─ EnvironmentSnapshotService.cs
│  │  │  │  └─ ZipBundleWriter.cs
│  │  │  ├─ Content/
│  │  │  │  ├─ ManifestService.cs
│  │  │  │  ├─ ContentPackResolver.cs
│  │  │  │  └─ CopyDeckService.cs
│  │  │  └─ Support/
│  │  │     ├─ FriendlyMessageService.cs
│  │  │     ├─ UrlLauncher.cs
│  │  │     └─ ClipboardService.cs
│  │  └─ Utilities/
│  │     ├─ Result.cs
│  │     ├─ Guard.cs
│  │     └─ JsonDefaults.cs
│  │
│  ├─ Installer.Infrastructure/
│  │  ├─ Logging/
│  │  │  ├─ FileLogger.cs
│  │  │  └─ SessionLogWriter.cs
│  │  ├─ Storage/
│  │  │  ├─ AppDataPaths.cs
│  │  │  ├─ TempFileService.cs
│  │  │  └─ EmbeddedResourceExtractor.cs
│  │  ├─ Packaging/
│  │  │  ├─ PayloadLocator.cs
│  │  │  ├─ PortableAdbLocator.cs
│  │  │  └─ BuildStampReader.cs
│  │  └─ Process/
│  │     ├─ ProcessService.cs
│  │     └─ ProcessResult.cs
│  │
│  └─ Installer.Contracts/
│     ├─ Manifests/
│     │  ├─ install-manifest.schema.json
│     │  └─ diagnostics-manifest.schema.json
│     └─ Dtos/
│        ├─ DeviceSnapshotDto.cs
│        ├─ InstallAttemptDto.cs
│        └─ DiagnosticBundleDto.cs
│
├─ payloads/
│  ├─ current/
│  │  ├─ app-manifest.json
│  │  └─ example-app.apk
│  └─ tools/
│     ├─ adb/
│     └─ scrcpy/
│
├─ tests/
│  ├─ Installer.Core.Tests/
│  │  ├─ Devices/
│  │  ├─ Install/
│  │  ├─ Flow/
│  │  ├─ Recovery/
│  │  └─ Diagnostics/
│  ├─ Installer.Infrastructure.Tests/
│  └─ Installer.App.Tests/
│
├─ build/
│  ├─ packaging/
│  │  ├─ InnoSetup/
│  │  │  └─ Installer.iss
│  │  └─ scripts/
│  │     ├─ pack.ps1
│  │     ├─ sign.ps1
│  │     └─ bundle-payload.ps1
│  └─ ci/
│     └─ github-actions.yml
│
└─ docs/
   ├─ product-spec.md
   ├─ copy-deck.md
   ├─ manifest-examples/
   └─ support-runbook.md
```


## Architecture notes

### Installer.App

This is pure presentation and orchestration. No raw ADB parsing should live in ViewModels. That separation prevents the classic WPF failure mode where process execution logic leaks into UI code and becomes untestable.

### Installer.Core

This contains all decision logic:

- Detect device
- Infer next wizard step
- Build ADB commands
- Classify errors
- Choose recovery actions

This is where most unit tests belong.

### Installer.Infrastructure

This handles real OS/process/file access. Keep it boring and replaceable. ADB process launching, temporary extraction, logging, and ZIP writing belong here.

### Installer.Contracts

Schemas and DTOs only. This keeps your content-pack and diagnostics formats stable and versionable.

## Key domain models

### `DeviceInfo`

```csharp
public sealed record DeviceInfo(
    string Serial,
    string Manufacturer,
    string Model,
    string AndroidVersion,
    DeviceKind Kind,
    DeviceConnectionState State,
    bool IsAuthorized,
    bool IsQuest,
    IReadOnlyDictionary<string, string> Properties);
```


### `InstallManifest`

```csharp
public sealed record InstallManifest(
    string AppId,
    string DisplayName,
    string BuildVersion,
    string ApkPath,
    IReadOnlyList<string> TargetPlatforms,
    InstallPolicy InstallPolicy,
    bool GrantPermissions,
    bool AllowTestApk,
    bool LaunchAfterInstall,
    IReadOnlyDictionary<string, IReadOnlyList<string>> PostInstallNotes);
```


### `InstallResult`

```csharp
public sealed record InstallResult(
    bool Success,
    string? InstalledVersion,
    InstallError? Error,
    string RawOutput,
    IReadOnlyList<RecoveryAction> SuggestedActions);
```


## Service boundaries

### `IAdbClient`

Responsibilities:

- Start/kill ADB server
- Enumerate devices
- Query properties
- Install APK
- Uninstall package
- Grant permissions
- Launch package
- Collect logcat

This should expose typed methods, not shell strings.

### `IDeviceService`

Responsibilities:

- Live device monitor
- Classification
- Readiness snapshot
- Vendor/model adaptation


### `IInstallService`

Responsibilities:

- Generate install plan from manifest + device state
- Execute plan
- Verify package presence
- Return typed result


### `IRecoveryService`

Responsibilities:

- Translate `InstallError` to recovery actions
- Attempt automatic fixes
- Decide whether to retry silently or prompt user


### `IDiagnosticsService`

Responsibilities:

- Gather evidence
- Sanitize secrets
- Produce ZIP bundle


## Wizard state machine

Model the wizard as deterministic transitions. Example:

```text
Welcome
 -> ConnectDevice
 -> DeviceDetected
 -> AuthorizationRequired? yes -> Authorization
 -> DeveloperModeRequired? yes -> DeveloperMode
 -> ReadyToInstall
 -> Installing
 -> InstallFailed? yes -> InstallProblem
 -> Complete
```

The state machine should be driven by actual device state, not button order. If the user plugs in an already-authorized Quest with correct prerequisites, the flow should skip directly to `ReadyToInstall`.[^3][^1]

## Install strategy logic

Pseudo-rules:

- If package absent: normal install.
- If package exists and manifest policy is `ReinstallKeepData`: use `-r`.
- If downgrade needed and allowed: use `-r -d`.
- If build is test APK: include `-t`.
- If permissions should be auto-granted and allowed: include `-g`.[^3]

Suggested install planner output:

```csharp
public sealed record InstallPlan(
    string PackageId,
    string ApkPath,
    IReadOnlyList<string> AdbFlags,
    bool RequiresUninstallFirst,
    bool VerifyAfterInstall,
    bool OfferLaunchAfterInstall);
```


## Logging and privacy

Log everything needed for support, but never collect irrelevant personal device data. Device model, Android version, serial hash, package results, and filtered logcat are useful; full filesystem scans are not. That gives you support-grade observability without building accidental spyware theater.

## UI/UX guidance

Use large cards, sparse screens, and explicit verbs:

- `Connect headset`
- `Put on headset`
- `Approve this computer`
- `Install app`
- `Done`

Avoid:

- `ADB`
- `Sideload`
- `Package manager`
- `USB transport`
unless hidden behind an advanced disclosure. Your user-facing copy should describe intent, not mechanism.


## Phase plan

### Phase 1: MVP

- Single APK payload
- Quest + phone detection
- Authorization handling
- Install, reinstall, downgrade support
- Friendly errors
- Diagnostics ZIP
- Inno Setup packaging


### Phase 2

- Split APK/APKS
- Embedded scrcpy support mode
- QR-linked visual help
- Multi-device picker
- Multi-app hub mode
- Better vendor-specific phone copy


### Phase 3

- Wireless ADB pairing where practical
- Auto-update manifest/payload channel
- Remote support session integration
- Tester telemetry with explicit opt-in


## Recommended packaging

Use Inno Setup for the Windows installer shell because it is mature, scriptable, and already aligned with your prior installer work. Your final package should install the WPF app, portable ADB payload, current app manifest, and APK payload in one predictable folder structure.[^13][^14]

## Immediate next implementation order

1. Build `Installer.Core` models and enums.
2. Implement `AdbClient` plus device parsing.
3. Implement `DeviceMonitorService`.
4. Implement wizard state machine.
5. Implement `InstallService` with `-r`, `-d`, `-t`, `-g` policy support.
6. Implement error classification and recovery suggestions.
7. Build the WPF pages.
8. Add diagnostics ZIP export.
9. Package with Inno Setup.

Would you like the next step as a Cursor-ready implementation plan with exact file-by-file starter scaffolding and class skeletons?
<span style="display:none">[^15][^16][^17][^18][^19][^20]</span>

<div align="center">⁂</div>

[^1]: https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/

[^2]: https://developers.meta.com/horizon/documentation/android-apps/enable-developer-mode/

[^3]: https://developer.android.com/tools/adb

[^4]: https://developers.meta.com/horizon/documentation/unity/ts-mqdh-deploy-build/

[^5]: https://adbappcontrol.com/

[^6]: https://sidequestvr.com/setup-howto

[^7]: https://www.meta.com/help/quest/291654372573077/

[^8]: https://www.reddit.com/r/MetaQuestVR/comments/1sijj5z/unknown_sources_missing_with_the_new_ui_update/

[^9]: https://communityforums.atmeta.com/discussions/Questions_Discussions/how-to-open-an-unknown-source-at-meta-quest-3-after-v2-1-buil%EF%BC%9F/1371438

[^10]: https://stackoverflow.com/questions/13400499/is-there-any-way-to-ignore-install-failed-version-downgrade-on-application-insta

[^11]: https://getandora.in/blog/adb-install-failed

[^12]: https://truenorthvr.com/where-to-find-unknown-sources-meta-quest-vr/

[^13]: https://www.perplexity.ai/search/ee25eeb4-aa08-4ae1-a4ff-d032e24be968

[^14]: https://www.perplexity.ai/search/27321dd3-d31b-4f21-b900-17cc9c286d7b

[^15]: https://www.facebook.com/groups/metaxr/posts/1191033264412628/

[^16]: https://www.youtube.com/watch?v=RRUWDsH4RqA

[^17]: https://www.facebook.com/groups/Quest2Community/posts/1439547600524965/

[^18]: https://www.youtube.com/watch?v=TWHrvQ3VTJQ

[^19]: https://www.youtube.com/watch?v=J5PR7J8n6P4

[^20]: https://www.youtube.com/watch?v=BtHBA_--AxQ\&vl=en

