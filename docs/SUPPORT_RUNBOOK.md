# Support runbook

Use this when a tester says the installer “didn’t work.” Ask for the diagnostics ZIP first. Do not ask them to run ADB.

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
5. If still empty, collect diagnostics from the Connect screen if offered; otherwise have them screenshot Advanced details.

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

**Tester sees:** A different copy of this app is already installed.

**Operator response:** Uninstall the existing app, then install. This deletes that app’s local data. Do not factory-reset the headset.

### Insufficient storage

**Operator response:** Ask them to free space in headset/phone storage settings. The installer cannot safely delete unrelated apps.

### Install succeeded but tester cannot find the Quest app

**Operator response:** Library → filter → Unknown Sources. UI labels move across Horizon OS versions. If still missing, check diagnostics for `pm list packages` verification. If the package is present, it is a launcher/UI issue, not an install failure.

### More than one device

**Tester sees:** Wrong device detected, or a generic failure.

**Operator response:** Unplug everything except the target device and retry. Multi-device picker is not in v1.

### ADB server unhappy

**Symptoms:** Empty device list despite a known-good cable, or stale unauthorized state.

**Auto-fix:** Restart ADB server, then rescan.

**Operator response:** If restart loops, reboot the PC and the device, then reopen the installer. Confirm only one installer/ADB client is running (close Android Studio / MQDH if they have it).

## What to send back to engineering

1. Diagnostics ZIP.
2. Device family (Quest 2/3 vs Pixel vs Samsung) and roughly when it failed in the wizard.
3. Whether they ever saw the USB debugging prompt.
4. Whether this was a first install or a replacement of an older test build.
