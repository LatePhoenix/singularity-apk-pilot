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
6. Optional: on Choose apps, tap **Switch to Wi-Fi**, then unplug. Later sessions can tap **Connect over Wi-Fi** or enter an address / pairing code on Connect device. Quest 2 / 3 walkthrough is on that screen.
7. Choose APK, APKS, or XAPK files, install (including split sets), then tell the tester the app may appear under Unknown Sources. Headset UI placement can change across Horizon OS updates. **Open on device** is optional after install.

## Phone flow

1. Detect Android phone.
2. If unauthorized: unlock the phone and accept USB debugging.
3. Choose APK / APKS / XAPK files and install using the default install policy (`-r -d -t -g`). Split files for the same package use `install-multiple`.
4. Verify with `pm list packages` when a real package id was read from the file.
5. Show app-drawer launch notes and an optional Open on device action.

## Functional requirements

### Detection

`adb` is the source of truth. Poll `adb devices -l`, then `getprop` for manufacturer, model, and Android version. States: not connected, unauthorized, offline, connected-ready. USB is the default path. After the device is authorized, testers can switch to Wi-Fi (`tcpip` then connect to the device address). Later sessions can reconnect the last address, or enter an address and optional pairing code. When two or more ready devices are present, the Device detected step lists them (model, USB vs Wi-Fi) and does not auto-pick on refresh. After two failed connect attempts, a Windows USB-presence snapshot distinguishes “headset plugged in but not visible to this installer” from “nothing on USB.”

### Install

One or more install sets, chosen after the device is ready. Same package id across `.apk` files becomes one split set (`adb install-multiple`). `.apks` / `.xapk` extract to a temp folder and install as one set. Different packages install sequentially. Policies: InstallFresh, ReinstallKeepData, ReinstallAllowDowngrade, UninstallThenInstall, InstallTestBuild. Flags: `-r`, `-d`, `-t`, `-g`. Verify with `pm list packages` when a real package id is known. Do not auto-launch; Complete can offer **Open on device**.

### Recovery

Map raw ADB/package errors to a typed taxonomy. Show at most three user-facing actions. Safe one-click fixes: restart ADB server, retry install, retry with a valid alternate policy.

### Diagnostics

ZIP with app/build info, manifest id, device metadata, ADB snapshot, install attempt, sanitized stdout/stderr, and filtered logcat for the package.

## Packaging

Mode A for v1: one Windows package contains the WPF shell, portable ADB, and optional install-policy JSON. APK files are not packaged. Inno Setup, optional launch-after-install. Testers download **`SingularityApkInstaller-win-x64-setup.exe`** from [GitHub Releases](https://github.com/LatePhoenix/singularity-apk-installer/releases/latest). That stable name is what the repo README download button serves.

**v0.3.0** ships portable `adb` only. Testers connect a device, then choose APK / APKS / XAPK files. The published app is a single-file exe so Windows Application Control does not block unsigned satellite DLLs. Missing selected APK is a typed `MissingPayload` error, not a device/USB failure. Split packages, verify-by-package-id, optional Open on device, last-files recents, and gated Authenticode are in this release.

Artifact names and pack steps: [`PACKAGING.md`](PACKAGING.md).

## Legal

Company Privacy Policy and Terms of Service: https://singularity.mhbross725.workers.dev/  
Product copies: [`legal/PrivacyPolicy.md`](legal/PrivacyPolicy.md), [`legal/TermsOfService.md`](legal/TermsOfService.md).

## Manual acceptance paths

- Quest: unauthorized → authorized → install.
- Quest: authorized USB → Switch to Wi-Fi → unplug → install.
- Phone: unauthorized → authorized → install.
- Quest: two ready devices → picker → continue with the selected headset.
- Quest: install a split set / `.apks`, then Open on device.
- Installer package launches the app when the post-install checkbox is selected.
