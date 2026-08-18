# Privacy Policy

**Product:** APK Pilot  
**Publisher:** Singularity Solutions and Services LLC  
**Last updated:** August 18, 2026  
**Effective:** August 15, 2026

This Privacy Policy describes how APK Pilot (the “Software”), published by Singularity Solutions and Services LLC (a Wisconsin LLC, Entity ID S165572, principal office 206 S River St, Waterford, WI 53185, United States of America), handles information on your computer and a connected headset or phone.

The publisher’s company Privacy Policy is hosted at  
https://singularity.mhbross725.workers.dev/privacy

## 1. No account; local-first

The Software does not require an account and does not operate a user registration service. Session logs and diagnostics you export are stored on your PC under `%LocalAppData%\SingularityApkInstaller\`. This is a local-first tester tool. We do not collect consumer analytics or marketing profiles through the Software.

## 2. Information the Software processes locally

Depending on how you use the Software, it may process:

- USB or Wi-Fi device connection state from portable Android Debug Bridge (`adb`)
- Last Wi-Fi address used to reconnect a device (stored locally; pairing codes are not saved)
- Last report recipient email, stored locally after you send a report
- Device manufacturer, model, and Android version used to tell Quest from phone
- A hashed form of the device serial in exported diagnostics (the raw serial is not written into the ZIP)
- File names of APKs you choose to install
- Command results from install, uninstall, and verification
- Third-party app names and package ids, read locally when you open Installed apps (not uploaded)
- Package-filtered logcat and the session log, only when you send a report and (for logcat) the device is authorized

This processing happens on your machine so the wizard can install the APK files you select and, if you ask, remove an app you select. The Software does not scan the device filesystem for unrelated files, accounts, or contacts. Diagnostics export does not include a full list of apps on the device.

## 3. Diagnostics export

**Send a report** is user-initiated. It asks for an email address (the person who asked you to test), writes a ZIP on this computer, and opens your email app with that file attached. You still press Send in the email app. The address is stored locally so the next report can reuse it. The Software does not upload the ZIP or send the email itself. The ZIP includes this session’s log, connection state, and (if the device allowed this computer) a filtered device log. Serial numbers in the bundle are hashed. Do not share that ZIP if it contains information you do not want others to see.

## 4. Bundled tools

The Software may bundle portable `adb` (Android SDK platform-tools). That component remains subject to its own license and Google’s terms. The Software does not send your device data to Singularity servers.

## 5. No selling of personal data

Singularity Solutions and Services LLC does not sell, rent, trade, or commercialize personal information collected through the Software.

## 6. Children

The Software is a tester tool for installing development builds. It is not directed at children under 13.

## 7. Changes

We may update this Privacy Policy when the product changes. The “Last updated” date at the top will change when we do. The publisher’s hosted company policy remains at the URL above.

## 8. Contact

For product privacy questions, open an issue at  
https://github.com/LatePhoenix/singularity-apk-pilot/issues

For formal or legal notices:

Singularity Solutions and Services LLC  
206 S River St  
Waterford, WI 53185  
United States of America  
Email: matt.brossard323@gmail.com

© 2026 Singularity Solutions and Services LLC. All rights reserved.
