# Singularity APK Installer

Windows guided installer for non-technical testers. Plug in a Meta Quest 2/3 or Android phone, follow one highlighted action per screen, and install a bundled test APK with portable `adb`.

> Part of the [Singularity](https://github.com/LatePhoenix/singularity-hub) app family.

## Requirements

- Windows 10/11 x64
- .NET SDK 8.0.424 (`global.json`)
- USB-C **data** cable (the cable in the Quest box is not suitable)
- Oculus ADB Drivers for Quest on Windows
- For Quest: Developer Mode in the Meta Horizon app, then **Always allow from this computer** in-headset

## Build

```powershell
dotnet build SingularityTesterInstaller.sln
dotnet test SingularityTesterInstaller.sln
```

Packaging (Inno Setup 7) lives in `build/packaging/`. Payload APK and bundled `adb.exe` are not in git; see `payloads/` and `docs/SUPPORT_RUNBOOK.md`.

## Docs

- `docs/PRODUCT_SPEC.md`
- `docs/ARCHITECTURE.md`
- `docs/COPY_DECK.md`
- `docs/SUPPORT_RUNBOOK.md`
