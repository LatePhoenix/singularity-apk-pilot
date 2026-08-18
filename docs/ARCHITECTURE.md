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
| Core | Contracts | Process start, file ZIP, WPF types |
| Infrastructure | Core, Contracts | Wizard copy, WPF |
| Contracts | nothing | Behavior |

ViewModels call service interfaces only. `App/Bootstrap` is the only place that binds interfaces to Infrastructure types.

## Core modules

- **Adb:** `IAdbClient`, `AdbCommandFactory`, output parsers. Command strings are built here; process launch is not. Wireless commands: `tcpip`, `connect`, `disconnect`, `pair`, and Wi-Fi address via `ip`. Install commands include `install-multiple`, `cmd package resolve-activity`, and `am start`.
- **Packages:** `IApkInspector` reads package id / version / split name from APK zip manifests (binary AXML or XML). `.apks` / `.xapk` are zip-of-apks. `IInstallSetFactory` groups files into install sets.
- **Wireless:** `IWirelessAdbService` enables USB-first Wi-Fi, reconnects a saved endpoint, and pairs then connects. Last address is stored in AppData, never pairing codes. Connect device shows the Quest 2/3 walkthrough and a Wi-Fi form; Choose apps offers **Switch to Wi-Fi** after USB approval.
- **Devices:** parse `adb devices -l`, classify Quest vs phone, prefer a Wi-Fi record when USB and Wi-Fi both appear, poll for WPF binding. `IDeviceHealthService` plus an Infrastructure USB registry probe distinguish empty `adb` from Windows seeing a headset.
- **Install:** plan flags from `InstallPolicy` + manifest + `InstallSet`, execute `install` or `install-multiple`, verify package, optional launch.
- **Flow:** deterministic wizard state machine with Quest and Android strategies. Device refresh does not leave Device detected while two or more ready devices are unresolved.
- **Recovery:** classify stderr into `InstallError`, return ≤3 actions, optional auto-fix, explicit replace/remove using the known package id.
- **Diagnostics:** assemble a sanitized ZIP from snapshots already in memory plus filtered logcat. Export is available on Connect, Authorization, Developer mode, Problem, and Complete.
- **Content:** load `app-manifest.json`, resolve copy for the current step/device. Recents store last files/folder next to the Wi-Fi endpoint.

## Infrastructure modules

- Process execution (`ProcessService`, `AdbProcessRunner`).
- Portable ADB and payload path resolution.
- File logger / session log.
- ZIP writer, temp files, recents JSON, GitHub latest-release check, USB presence probe.

## Wizard state machine

```
Welcome
 → ConnectDevice          (until a device serial exists)
 → DeviceDetected         (classification known)
 → Authorization          (if unauthorized)
 → DeveloperMode          (Quest, if developer mode likely missing)
 → ReadyToInstall (choose APK files)
 → Installing
 → InstallProblem | Complete
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

Log command lines, exit codes, and sanitized output. Hash device serials in exported bundles. Do not scan the device filesystem. Do not collect accounts, contacts, or unrelated packages.

## Packaging output

`build/packaging/scripts/pack.ps1` publishes a self-contained `win-x64` build, then Inno Setup 7 writes:

- `artifacts/installer/SingularityApkInstaller-<version>-win-x64-setup.exe`
- `artifacts/installer/SingularityApkInstaller-win-x64-setup.exe` (stable name for GitHub `/releases/latest/download/`)

Portable `adb` is resolved at pack time into `payloads/tools/adb/` (not committed). See [`PACKAGING.md`](PACKAGING.md).
