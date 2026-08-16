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

- **Adb:** `IAdbClient`, `AdbCommandFactory`, output parsers. Command strings are built here; process launch is not.
- **Devices:** parse `adb devices -l`, classify Quest vs phone, poll for WPF binding.
- **Install:** plan flags from `InstallPolicy` + manifest, execute, verify package.
- **Flow:** deterministic wizard state machine with Quest and Android strategies.
- **Recovery:** classify stderr into `InstallError`, return ≤3 actions, optional auto-fix.
- **Diagnostics:** assemble a sanitized ZIP from snapshots already in memory plus filtered logcat.
- **Content:** load `app-manifest.json`, resolve copy for the current step/device.

## Infrastructure modules

- Process execution (`ProcessService`, `AdbProcessRunner`).
- Portable ADB and payload path resolution.
- File logger / session log.
- ZIP writer and temp files.

## Wizard state machine

```
Welcome
 → ConnectDevice          (until a device serial exists)
 → DeviceDetected         (classification known)
 → Authorization          (if unauthorized)
 → DeveloperMode          (Quest, if developer mode likely missing)
 → ReadyToInstall
 → Installing
 → InstallProblem | Complete
```

Transitions are driven by `DeviceInfo` and `InstallResult`, not by button order. An already-authorized Quest skips to ReadyToInstall.

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
