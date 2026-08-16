# Privacy Policy

**Product:** Singularity APK Installer  
**Publisher:** Singularity Solutions and Services LLC  
**Last updated:** August 16, 2026  
**Effective:** August 15, 2026

This Privacy Policy describes how Singularity APK Installer (the “Software”), published by Singularity Solutions and Services LLC (a Wisconsin LLC, Entity ID S165572, principal office 206 S River St, Waterford, WI 53185, United States of America), handles information on your computer and a connected headset or phone.

The publisher’s company Privacy Policy is hosted at  
https://singularity.mhbross725.workers.dev/privacy

## 1. No account; local-first

The Software does not require an account and does not operate a user registration service. Session logs and diagnostics you export are stored on your PC under `%LocalAppData%\SingularityApkInstaller\`. This is a local-first tester tool. We do not collect consumer analytics or marketing profiles through the Software.

## 2. Information the Software processes locally

Depending on how you use the Software, it may process:

- USB device connection state from portable Android Debug Bridge (`adb`)
- Device manufacturer, model, and Android version used to tell Quest from phone
- A hashed form of the device serial in exported diagnostics (the raw serial is not written into the ZIP)
- Package id and version of the bundled test APK
- Command results from install, uninstall, and verification
- Package-filtered logcat, only when you export diagnostics and the device is authorized

This processing happens on your machine so the wizard can install the bundled test app. The Software does not scan the device filesystem for unrelated files, accounts, contacts, or other packages.

## 3. Diagnostics export

**Export diagnostics** is user-initiated. It writes a ZIP you can send to support. Do not share that ZIP if it contains information you do not want others to see. Serial numbers in the bundle are hashed.

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
https://github.com/LatePhoenix/singularity-apk-installer/issues

For formal or legal notices:

Singularity Solutions and Services LLC  
206 S River St  
Waterford, WI 53185  
United States of America  
Email: matt.brossard323@gmail.com

© 2026 Singularity Solutions and Services LLC. All rights reserved.
