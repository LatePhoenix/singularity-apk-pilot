# Support runbook

Testers should run **`SingularityApkInstaller-win-x64-setup.exe`** from the [latest GitHub Release](https://github.com/LatePhoenix/singularity-apk-installer/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe). Direct them to the repo README download button, not a random Dropbox/email copy.

Legal: [Privacy Policy](https://singularity.mhbross725.workers.dev/privacy) · [Terms of Service](https://singularity.mhbross725.workers.dev/terms) · [legal home](https://singularity.mhbross725.workers.dev/).

Use this when a tester says the installer “didn’t work.” Ask for the diagnostics ZIP first. Do not ask them to run ADB.

## Getting the Windows installer

1. Open [Releases](https://github.com/LatePhoenix/singularity-apk-installer/releases/latest) or the green **Download** badge on the repo home page.
2. Run `SingularityApkInstaller-win-x64-setup.exe`. No separate .NET runtime install is required.
3. If SmartScreen appears (unsigned build), **More info** → **Run anyway**.
4. Confirm they launched **Singularity APK Installer** from the Start menu after setup. If the window never appears, they are on v0.1.1 or earlier and Windows Application Control may be blocking satellite DLLs. Install **v0.3.0** (single-file).

**v0.1.0:** no test `.apk` in the GitHub installer; install failed as `failed to stat`. **v0.1.1** bundled Halo to unblock testers. **v0.2.0** does not ship any app APK: connect a device first, then choose APK files. **v0.3.0** adds split packages, verify/open, recents, multi-device picker, Connect-screen diagnostics, and optional Authenticode. Publish is a single-file exe so Windows Application Control does not block unsigned DLLs.

## Bundle contents

- `metadata.json` — installer version, manifest app id / version, timestamp
- `environment.json` — OS, 64-bit, ADB path used
- `device.json` — kind, model, manufacturer, Android version, hashed serial, connection state
- `adb-devices.txt` — sanitized `adb devices -l`
- `install-attempt.json` — policy, flags, exit code, timestamps
- `adb-output.txt` — sanitized stdout/stderr
- `logcat-filtered.txt` — package-filtered logcat if the device was authorized

Serials are hashed. Do not ask testers to paste raw serial numbers in email.

## Failure cases

### No device found

**Tester sees:** Connect screen never advances, or “No device was found.”

**Likely causes:** Charge-only cable (including the in-box Quest cable), USB hub, missing Windows driver, Quest developer mode off, phone USB mode is charge-only.

**Operator response:**

1. Confirm they used a USB-C **data** cable into a motherboard port.
2. Quest: walk Meta Horizon app → headset → Headset Settings → Developer Mode on. Then in-headset Developer → MTP Notification on.
3. Phone: File transfer / MTP, then retry.
4. Windows: if the device never appears, install Oculus ADB Drivers for Quest, or the phone OEM USB driver.
5. If still empty, collect diagnostics from the Connect screen (**Export diagnostics**). If Windows sees a Quest and the installer does not, use a data cable and the Oculus ADB driver. If Windows sees nothing, try another cable or port and avoid a hub.

### Unauthorized

**Tester sees:** Put on headset / unlock phone.

**Likely causes:** Prompt dismissed, headset off the head, phone locked, “Always allow” not checked.

**Operator response:** Unplug/replug, keep the device awake, choose **Always allow from this computer**. Quest prompt is inside the headset, not on the PC.

### Developer mode likely disabled (Quest)

**Tester sees:** Developer mode page, or repeated no-device after they swear it is plugged in.

**Likely causes:** Not on a developer team, unverified Meta account, wrong account in the Horizon app, under 18, device restriction.

**Operator response:** Confirm they are signed into the Horizon app with the same Meta account that is on the developer team. Account verification is required. Do not try to bypass Meta’s developer-mode gate.

### Offline / disconnected during install

**Tester sees:** Device disconnected.

**Operator response:** Replace the cable, avoid hubs, keep the headset on and awake, retry. If it fails at Verifying, check whether the package actually installed (`install-attempt` + logcat).

### Version downgrade

**Tester sees:** Older incompatible version is already installed.

**Auto-fix:** Retry with `ReinstallAllowDowngrade` (`-r -d`) when the manifest allows it.

**Operator response:** If auto-fix fails, use UninstallThenInstall (data loss for that app only). Signature mismatch will still fail until uninstall.

### Signature mismatch / already exists

**Tester sees:** A different copy of this app is already installed, or Replace this app / Remove this app and install.

**Operator response:** Use **Replace this app** when it is the same signing key, or **Remove this app and install** when signatures differ. This deletes that app’s local data. Do not factory-reset the headset.

### Missing split

**Tester sees:** “This looks like only part of the app.”

**Operator response:** They selected a config/split APK without the base. Add the rest of the files or an `.apks` / `.xapk` package. Do not treat this as a cable failure.

### App never opens

**Tester sees:** Setup finishes, Start menu shortcut does nothing, or a .NET Runtime Event Log `FileLoadException` / `0x800711C7` (Application Control blocked `Installer.Infrastructure.dll`).

**Operator response:** That is v0.1.1’s multi-DLL publish. Give them **v0.3.0** (single-file). Do not treat this as a missing APK or cable issue.

### Selected APK missing

**Tester sees:** “The APK file could not be found.”

**Likely causes:** File moved after they picked it, or they never added an APK.

**Operator response:** Add the `.apk` again on the choose-apps screen. This installer does not include a test app.

### Insufficient storage

**Operator response:** Ask them to free space in headset/phone storage settings. The installer cannot safely delete unrelated apps.

### Install succeeded but tester cannot find the Quest app

**Operator response:** Library → filter → Unknown Sources. UI labels move across Horizon OS versions. If still missing, check diagnostics for `pm list packages` verification. If the package is present, it is a launcher/UI issue, not an install failure.

### More than one device

**Tester sees:** Wrong device detected, or a generic failure.

**Operator response:** Unplug extras, or pick the right row on Device detected (v0.3.0 lists ready devices by model and USB vs Wi-Fi). The installer does not auto-switch the selected device while that list is showing.

### Wi-Fi connection failed

**Tester sees:** “Wi-Fi connection did not work,” or Connect over Wi-Fi does nothing.

**Likely causes:** Device and PC are on different networks, headset rebooted (USB-first Wi-Fi is cleared until they plug in again), they entered the pairing port instead of the connect port, or the pairing code expired.

**Operator response:**

1. Same Wi-Fi as the PC. Guest networks and client isolation will fail.
2. Plug in USB, approve debugging, then tap **Use Wi-Fi** on the choose-apps screen. That is the reliable Quest path.
3. Pairing: the six-digit code and pairing port are only for first-time wireless debugging. After pairing, connect to the install address (usually port 5555), not the pairing port.
4. After a headset reboot, USB-first switch is required again unless they re-pair.

### ADB server unhappy

**Symptoms:** Empty device list despite a known-good cable, or stale unauthorized state.

**Auto-fix:** Restart ADB server, then rescan.

**Operator response:** If restart loops, reboot the PC and the device, then reopen the installer. Confirm only one installer/ADB client is running (close Android Studio / MQDH if they have it).

## What to send back to engineering

1. Diagnostics ZIP.
2. Device family (Quest 2/3 vs Pixel vs Samsung) and roughly when it failed in the wizard.
3. Whether they ever saw the USB debugging prompt.
4. Whether this was a first install or a replacement of an older test build.
