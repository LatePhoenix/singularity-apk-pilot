# Architecture

Layered .NET 8 solution. UI never builds or parses `adb` commands.

```
Installer.App            WPF views, view models, composition root
        ↓
Installer.Core           domain models, abstractions, decision logic
        ↓
Installer.Infrastructure OS/process/filesystem/logging implementations
Installer.Contracts      DTOs and JSON schemas only
```

## Project rules

| Project | May depend on | Must not contain |
| --- | --- | --- |
| App | Core, Contracts, Infrastructure (composition root only) | ADB argument assembly, parsers |
| Core | Contracts | Process start, ZIP writing, WPF types. `File.Exists` is allowed for payload and install-set checks. |
| Infrastructure | Core, Contracts | Wizard copy, WPF |
| Contracts | nothing | Behavior |

ViewModels call service interfaces only. `App/Bootstrap` is the only place that binds interfaces to Infrastructure types.

## Core modules

- **Adb:** `IAdbClient`, `AdbCommandFactory`, output parsers. Command strings are built here; process launch is not. Wireless commands: `tcpip`, `connect`, `disconnect`, `pair`, and Wi-Fi address via `ip`. Install commands include `install-multiple`, `cmd package resolve-activity`, and `am start`.
- **Packages:** `IApkInspector` reads package id / version / split name from APK zip manifests (binary AXML or XML). `.apks` / `.xapk` are zip-of-apks. `IInstallSetFactory` groups files into install sets.
- **Wireless:** `IWirelessAdbService` enables USB-first Wi-Fi, reconnects a saved endpoint, and pairs then connects. Last address is stored in AppData, never pairing codes. Connect device hides Wi-Fi behind an expander; Choose apps offers **Switch to Wi-Fi** after USB approval. Quest 2 / 3 / 3S / Pro share that path.
- **Devices:** parse `adb devices -l`, classify Quest vs phone (`hollywood`, `eureka`, `panther`, `seacliff`), prefer a Wi-Fi record when USB and Wi-Fi both appear, poll for WPF binding. Monitor starts when the shell loads. `IDeviceHealthService` plus `IUsbEvidenceProbe` distinguish empty `adb` from Windows seeing a headset, an ADB interface without a driver, or MTP-only. `ITroubleshootingService` is a side-flow (`WizardStep.Troubleshoot`) shown in an owned modal helper window (not the main wizard card). Quest nodes include Meta developer account (skipped when USB already shows the headset) and two-prompt Allow. Phone nodes include Samsung Auto Blocker. Competing ADB/WebUSB tools open Restart helper first. Leave helper / window close returns to the screen that opened it. A ready device closes the helper immediately.
- **Install:** plan flags from `InstallPolicy` + manifest + `InstallSet`. User-selected APKs do not get `-g`/`-t` by default. Protected package ids are refused. Auto-fix never uninstalls (signature mismatch requires the explicit Remove control). `IInstalledAppService` lists third-party apps, refuses protected ids, uninstalls one at a time.
- **Flow:** deterministic wizard state machine with Quest and Android strategies. Single-device Connect/Start skips Device detected. Device refresh does not leave Device detected while two or more ready devices are unresolved. Installed apps is a side step from Choose apps / Complete; troubleshooting is a side step from Connect / Authorization / Developer mode / connection-lost Install problem, presented as an owned helper window. Two failed Connect attempts open that helper instead of the thin Developer mode page. A ready device closes the helper immediately.
- **Recovery:** classify stderr into `InstallError`, return ≤3 actions, optional auto-fix, explicit replace/remove using the known package id. `DebuggingNotApproved` is unused by the classifier and treated as Unauthorized.
- **Diagnostics:** assemble a sanitized ZIP (session log, USB evidence, filtered logcat). Pairing codes, user profile paths, and raw serials are redacted. Serials are HMAC-hashed with a per-install key. Keep the last 10 session logs and 20 diagnostic ZIPs. **Send a report** is on every screen except Installing.
- **Content:** load `app-manifest.json`, resolve copy for the current step/device. Recents store last files/folder next to the Wi-Fi endpoint. **Pilot** (`IGuideCoach` / `GuideCompanion`) is the named helper surface: grandmother-language next-step scripts, docked or popped out.

## Infrastructure modules

- Process execution (`ProcessService`, `AdbProcessRunner`).
- Portable ADB and payload path resolution. Release builds use only bundled `adb.exe` (optional `.sha256` sidecar). Elevated USB helper runs `pnputil` only when `android_winusb.inf` exists under payloads (`File.Exists`); there is no INF digest check.
- File logger / session log.
- ZIP writer, temp files, recents JSON, last report recipient email, mail compose (MAPI / Outlook / mailto), GitHub latest-release check, USB evidence probe, optional elevated Quest USB helper (`pnputil` only when `android_winusb.inf` is present under payloads).

## Wizard state machine

```
Welcome
 → ConnectDevice          (until a device serial exists; skipped on Start if a device is already seen)
    ⇄ Troubleshoot helper   (Need help connecting? pops a modal window; also after two failed Connect attempts)
 → DeviceDetected         (only when two or more ready devices)
 → Authorization          (if unauthorized; monitor notices Allow without a click)
 → DeveloperMode          (Quest, offline / remaining short path)
 → ReadyToInstall (choose APK files)
    ⇄ InstalledApps          (third-party list + one-at-a-time remove)
 → Installing
 → InstallProblem | Complete
    ⇄ InstalledApps
    ⇄ Troubleshoot helper   (connection-lost install errors; same modal window)
```

Transitions are driven by `DeviceInfo` and `InstallResult`, not by button order. An already-authorized Quest skips to ReadyToInstall (APK picker).

## Install planning

| Policy | Behavior |
| --- | --- |
| InstallFresh | install, no `-r` |
| ReinstallKeepData | `-r` |
| ReinstallAllowDowngrade | `-r -d` |
| UninstallThenInstall | `uninstall` then install |
| InstallTestBuild | include `-t` |

Manifest `grantPermissions` adds `-g`. `allowTestApk` adds `-t` even when the policy is not InstallTestBuild.

## Logging / privacy

Log redacted command lines (pairing codes masked), exit codes, and sanitized output. Hash device serials with a per-install HMAC key. Strip `%USERPROFILE%` paths in exported bundles. Do not scan the device filesystem. Do not collect accounts, contacts, or a full package inventory in diagnostics. Installed apps reads third-party package names locally to show the list.

## Packaging output

`build/packaging/scripts/pack.ps1` publishes a self-contained `win-x64` build, then Inno Setup 7 writes:

- `artifacts/installer/SingularityApkInstaller-<version>-win-x64-setup.exe`
- `artifacts/installer/SingularityApkInstaller-win-x64-setup.exe` (stable name for GitHub `/releases/latest/download/`)

Portable `adb` is resolved at pack time into `payloads/tools/adb/` (not committed). Optional Quest USB INF is copied into `payloads/tools/oculus-adb-drivers/` when present locally. See [`PACKAGING.md`](PACKAGING.md).
