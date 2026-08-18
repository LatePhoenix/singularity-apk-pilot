# Singularity APK Installer

[![Download the installer](https://img.shields.io/badge/Download-SingularityApkInstaller--win--x64--setup.exe-2FA8C8?style=for-the-badge)](https://github.com/LatePhoenix/singularity-apk-installer/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe)

[![Latest release](https://img.shields.io/github/v/release/LatePhoenix/singularity-apk-installer?style=for-the-badge&label=release&color=0F1722)](https://github.com/LatePhoenix/singularity-apk-installer/releases/latest)

**⬇️ [Download SingularityApkInstaller-win-x64-setup.exe](https://github.com/LatePhoenix/singularity-apk-installer/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe)** — that link always serves the latest GitHub Release asset.

Windows guided installer for non-technical testers. Plug in a Meta Quest 2/3 or Android phone, follow one highlighted action per screen, then choose APK files to install with portable `adb`. After the device has approved this computer, you can switch to Wi-Fi and unplug. **Installed apps** lists third-party apps so you can remove one at a time.

> Part of the [Singularity](https://github.com/LatePhoenix/singularity-hub) app family.

## Download

1. Get **`SingularityApkInstaller-win-x64-setup.exe`** from [Releases](https://github.com/LatePhoenix/singularity-apk-installer/releases/latest) (or the button above).
2. Run the setup wizard. No separate .NET install is required (self-contained win-x64).
3. Launch **Singularity APK Installer** from the Start menu, plug in the device, then choose the APK files to install.

The installer is unsigned unless `pack.ps1` is run with a code-signing certificate configured. Windows SmartScreen may warn on first run of an unsigned build. Choose **More info** → **Run anyway**.

**v0.4.0** adds Quest Wi-Fi setup on Connect, **Switch to Wi-Fi** after USB approval, and **Installed apps** so testers can remove a third-party app one at a time. It still ships portable `adb` only — no bundled test app. See [`docs/SUPPORT_RUNBOOK.md`](docs/SUPPORT_RUNBOOK.md).

## Legal

- [Privacy Policy](https://singularity.mhbross725.workers.dev/privacy)
- [Terms of Service](https://singularity.mhbross725.workers.dev/terms)
- Company legal home: [singularity.mhbross725.workers.dev](https://singularity.mhbross725.workers.dev/)
- Product copies: [`docs/legal/PrivacyPolicy.md`](docs/legal/PrivacyPolicy.md), [`docs/legal/TermsOfService.md`](docs/legal/TermsOfService.md)

## Requirements

- Windows 10/11 x64
- USB-C **data** cable (the cable in the Quest box is not suitable)
- Oculus ADB Drivers for Quest on Windows
- For Quest: Developer Mode in the Meta Horizon app, then **Always allow from this computer** in-headset
- Optional Wi-Fi: after that approval, **Switch to Wi-Fi** on Choose apps; later sessions use **Connect over Wi-Fi**. Headset and PC must be on the same network.

## Build from source

```powershell
dotnet build SingularityTesterInstaller.sln
dotnet test SingularityTesterInstaller.sln
powershell -ExecutionPolicy Bypass -File build\packaging\scripts\pack.ps1
```

Requires .NET SDK 8.0.424 (`global.json`) and [Inno Setup 7](https://jrsoftware.org/isinfo.php). Output: `artifacts\installer\SingularityApkInstaller-0.4.0-win-x64-setup.exe`. Details: [`docs/PACKAGING.md`](docs/PACKAGING.md).

## Docs

- [`docs/PRODUCT_SPEC.md`](docs/PRODUCT_SPEC.md)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
- [`docs/COPY_DECK.md`](docs/COPY_DECK.md)
- [`docs/MANIFEST_SCHEMA.md`](docs/MANIFEST_SCHEMA.md)
- [`docs/SUPPORT_RUNBOOK.md`](docs/SUPPORT_RUNBOOK.md)
- [`docs/PACKAGING.md`](docs/PACKAGING.md)
- [`docs/legal/PrivacyPolicy.md`](docs/legal/PrivacyPolicy.md)
- [`docs/legal/TermsOfService.md`](docs/legal/TermsOfService.md)
