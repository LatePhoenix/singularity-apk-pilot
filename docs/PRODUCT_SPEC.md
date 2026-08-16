# Product spec

Windows-first guided installer for non-technical testers. It installs a single bundled test APK onto a Meta Quest 2/3 headset or an Android phone using portable `adb`.

## Promise

Plug in the device, follow one highlighted action per screen, finish. Quest-first, phone second. Not a general ADB toolbox.

## Goals

- One obvious next action on every screen.
- Automatic Quest vs phone classification.
- Typed recovery for common ADB/package failures.
- Reusable JSON app manifest so the same shell can ship different test builds.
- Diagnostics ZIP for support, without collecting unrelated device data.

## Non-goals

- File manager, debloater, store replacement, or developer console.
- Split APK / APKS (Phase 2).
- Wireless ADB pairing (Phase 3).

## Primary flow

Welcome → Connect device → Device detected → Authorization / developer mode only if needed → Ready to install → Installing → Problem or Complete.

Skip steps when device state already satisfies them. Branch Quest vs phone as soon as classification is known.

## Quest flow

Sideloading requires developer mode and USB debugging approval. Current Meta setup (verified 2026-08):

1. Tester belongs to a developer team and has a verified Meta account.
2. Enable Developer Mode in the Meta Horizon mobile app: headset icon → headset → Headset Settings → Developer Mode.
3. Use a USB-C **data** cable (the cable in the Quest box is not suitable).
4. In-headset: Quick Control → Settings → Developer → MTP Notification on.
5. Approve USB debugging and choose **Always allow from this computer**.
6. Install APK, then tell the tester the app may appear under Unknown Sources. Headset UI placement can change across Horizon OS updates.

## Phone flow

1. Detect Android phone.
2. If unauthorized: unlock the phone and accept USB debugging.
3. Install using the manifest install policy.
4. Verify package presence.
5. Show app-drawer launch notes.

## Functional requirements

### Detection

`adb` is the source of truth. Poll `adb devices -l`, then `getprop` for manufacturer, model, and Android version. States: not connected, unauthorized, offline, connected-ready.

### Install

Single APK only. Policies: InstallFresh, ReinstallKeepData, ReinstallAllowDowngrade, UninstallThenInstall, InstallTestBuild. Flags: `-r`, `-d`, `-t`, `-g`. Verify with `pm list packages`.

### Recovery

Map raw ADB/package errors to a typed taxonomy. Show at most three user-facing actions. Safe one-click fixes: restart ADB server, retry install, retry with a valid alternate policy.

### Diagnostics

ZIP with app/build info, manifest id, device metadata, ADB snapshot, install attempt, sanitized stdout/stderr, and filtered logcat for the package.

## Packaging

Mode A for v1: one Windows package contains the WPF shell, portable ADB, one manifest, and one APK. Inno Setup, optional launch-after-install.

## Manual acceptance paths

- Quest: unauthorized → authorized → install.
- Phone: unauthorized → authorized → install.
- Installer package launches the app when the post-install checkbox is selected.
