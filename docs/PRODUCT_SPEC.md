# Product spec

Windows-first guided installer for non-technical testers. It installs one or more user-selected APKs onto a Meta Quest 2/3 headset or an Android phone using portable `adb`.

## Promise

Plug in the device, follow one highlighted action per screen, choose APK files, finish. Quest-first, phone second. Not a general ADB toolbox.

## Goals

- One obvious next action on every screen.
- Automatic Quest vs phone classification.
- Typed recovery for common ADB/package failures.
- Reusable JSON install policy (`app-manifest.json`) for flags and post-install notes. APK files are chosen in the app, not bundled.
- Diagnostics ZIP for support, without collecting unrelated device data.

## Non-goals

- File manager, debloater, store replacement, or developer console.
- Split APK / APKS (Phase 2).
- Wireless ADB pairing (Phase 3).

## Primary flow

Welcome → Connect device → Device detected → Authorization / developer mode only if needed → Choose APKs → Installing → Problem or Complete.

Skip steps when device state already satisfies them. Branch Quest vs phone as soon as classification is known.

## Quest flow

Sideloading requires developer mode and USB debugging approval. Current Meta setup (verified 2026-08):

1. Tester belongs to a developer team and has a verified Meta account.
2. Enable Developer Mode in the Meta Horizon mobile app: headset icon → headset → Headset Settings → Developer Mode.
3. Use a USB-C **data** cable (the cable in the Quest box is not suitable).
4. In-headset: Quick Control → Settings → Developer → MTP Notification on.
5. Approve USB debugging and choose **Always allow from this computer**.
6. Choose one or more APK files, install, then tell the tester the app may appear under Unknown Sources. Headset UI placement can change across Horizon OS updates.

## Phone flow

1. Detect Android phone.
2. If unauthorized: unlock the phone and accept USB debugging.
3. Choose APK files and install using the default install policy (`-r -d -t -g`).
4. Skip package verification when the package id is unknown.
5. Show app-drawer launch notes.

## Functional requirements

### Detection

`adb` is the source of truth. Poll `adb devices -l`, then `getprop` for manufacturer, model, and Android version. States: not connected, unauthorized, offline, connected-ready.

### Install

One or more independent APKs, chosen after the device is ready. Sequential `adb install`. Policies: InstallFresh, ReinstallKeepData, ReinstallAllowDowngrade, UninstallThenInstall, InstallTestBuild. Flags: `-r`, `-d`, `-t`, `-g`. Verify with `pm list packages` only when a real package id is known.

### Recovery

Map raw ADB/package errors to a typed taxonomy. Show at most three user-facing actions. Safe one-click fixes: restart ADB server, retry install, retry with a valid alternate policy.

### Diagnostics

ZIP with app/build info, manifest id, device metadata, ADB snapshot, install attempt, sanitized stdout/stderr, and filtered logcat for the package.

## Packaging

Mode A for v1: one Windows package contains the WPF shell, portable ADB, and optional install-policy JSON. APK files are not packaged. Inno Setup, optional launch-after-install. Testers download **`SingularityApkInstaller-win-x64-setup.exe`** from [GitHub Releases](https://github.com/LatePhoenix/singularity-apk-installer/releases/latest). That stable name is what the repo README download button serves.

**v0.2.0** ships portable `adb` only. Testers connect a device, then choose APK files. The published app is a single-file exe so Windows Application Control does not block unsigned satellite DLLs. Missing selected APK is a typed `MissingPayload` error, not a device/USB failure.

Artifact names and pack steps: [`PACKAGING.md`](PACKAGING.md).

## Legal

Company Privacy Policy and Terms of Service: https://singularity.mhbross725.workers.dev/  
Product copies: [`legal/PrivacyPolicy.md`](legal/PrivacyPolicy.md), [`legal/TermsOfService.md`](legal/TermsOfService.md).

## Manual acceptance paths

- Quest: unauthorized → authorized → install.
- Phone: unauthorized → authorized → install.
- Installer package launches the app when the post-install checkbox is selected.
