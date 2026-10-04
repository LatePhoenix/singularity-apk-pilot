# APK Pilot

[![Download APK Pilot](https://img.shields.io/badge/Download-APK%20Pilot-2FA8C8?style=for-the-badge)](https://github.com/LatePhoenix/singularity-apk-pilot/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe)

[![Latest release](https://img.shields.io/github/v/release/LatePhoenix/singularity-apk-pilot?style=for-the-badge&label=release&color=0F1722)](https://github.com/LatePhoenix/singularity-apk-pilot/releases/latest)

**⬇️ [Download APK Pilot](https://github.com/LatePhoenix/singularity-apk-pilot/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe)** — that link always serves the latest GitHub Release asset (`SingularityApkInstaller-win-x64-setup.exe`).

Windows guided installer for non-technical testers. Plug in a Meta Quest 2, Quest 3, Quest 3S, Quest Pro, or Android phone. **Pilot** tells you what to do next. Follow the green button, then choose APK files to install. After the device has approved this computer, you can switch to Wi-Fi and unplug. **Installed apps** lists third-party apps so you can remove one at a time.

> Part of the [Singularity](https://github.com/LatePhoenix/singularity-hub) app family.

## Download

1. Get **`SingularityApkInstaller-win-x64-setup.exe`** from [Releases](https://github.com/LatePhoenix/singularity-apk-pilot/releases/latest) (or the button above). That stable name is always the current release (**v0.7.3** now).
2. Run the setup wizard. No separate .NET install is required (self-contained win-x64).
3. Launch **APK Pilot** from the Start menu, plug in the device, then choose the APK files to install. Pilot stays on the right and says the next step.

The installer is unsigned unless `pack.ps1` is run with a code-signing certificate configured. Windows SmartScreen may warn on first run of an unsigned build. Choose **More info** → **Run anyway**.

**v0.7.3** gives the Pilot robot a magenta visor glow in the app icon, installer, and in-app mark. **v0.7.2** keeps the device connected through brief adb hiccups, stops hung adb commands, unpacks .apks/.xapk bundles once in the background, and explains wrong-processor, too-old-Android, damaged-APK, and connection-helper failures. **v0.7.1** fixes a launch crash in the Pilot mark (frozen WPF stroke). **v0.7.0** adds **Go Back** and **Start Over**, runs install-problem suggestions instead of a blind retry, opens connection help after a stuck headset Allow prompt, and ships Meta’s Quest USB drivers for **Install Quest USB support**. See [`docs/SUPPORT_RUNBOOK.md`](docs/SUPPORT_RUNBOOK.md).

## Legal

- [Privacy Policy](https://singularity.mhbross725.workers.dev/privacy)
- [Terms of Service](https://singularity.mhbross725.workers.dev/terms)
- Company legal home: [singularity.mhbross725.workers.dev](https://singularity.mhbross725.workers.dev/)
- Product copies: [`docs/legal/PrivacyPolicy.md`](docs/legal/PrivacyPolicy.md), [`docs/legal/TermsOfService.md`](docs/legal/TermsOfService.md)

## Requirements

- Windows 10/11 x64
- USB-C **data** cable (the cable in the Quest box is not suitable)
- Quest USB support on Windows (the helper can install it or open Meta’s page)
- For Quest: Developer Mode in the Meta Horizon app, then **Always allow from this computer** in-headset (there may be two messages; keep the headset on)
- If the device is not detected, **Need help connecting?** walks Quest and phone setup, including Meta’s USB helper on Windows and Samsung Auto Blocker
- Optional Wi-Fi: after that approval, **Switch to Wi-Fi** on Choose apps; later sessions use **Connect over Wi-Fi**. Headset and PC must be on the same network. Turn off VPN. Guest networks will not work.

## Build from source

```powershell
dotnet build SingularityTesterInstaller.sln
dotnet test SingularityTesterInstaller.sln
powershell -ExecutionPolicy Bypass -File build\packaging\scripts\pack.ps1
```

Requires .NET SDK 8.0.424 (`global.json`) and [Inno Setup 7](https://jrsoftware.org/isinfo.php). Output: `artifacts\installer\SingularityApkInstaller-0.7.3-win-x64-setup.exe` plus the stable `SingularityApkInstaller-win-x64-setup.exe` used by the download button above. Details: [`docs/PACKAGING.md`](docs/PACKAGING.md).

## Docs

- [`docs/PRODUCT_SPEC.md`](docs/PRODUCT_SPEC.md)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
- [`docs/COPY_DECK.md`](docs/COPY_DECK.md)
- [`docs/MANIFEST_SCHEMA.md`](docs/MANIFEST_SCHEMA.md)
- [`docs/SUPPORT_RUNBOOK.md`](docs/SUPPORT_RUNBOOK.md)
- [`docs/PACKAGING.md`](docs/PACKAGING.md)
- [`docs/legal/PrivacyPolicy.md`](docs/legal/PrivacyPolicy.md)
- [`docs/legal/TermsOfService.md`](docs/legal/TermsOfService.md)
