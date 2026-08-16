# Packaging

Windows-first. The GitHub landing page (this repo’s README) always points at the latest Release asset:

`https://github.com/LatePhoenix/singularity-apk-installer/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe`

## Build the setup.exe

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File build\packaging\scripts\pack.ps1
```

This will:

1. Copy portable `adb` from Android SDK `platform-tools` (or `ANDROID_HOME` / `-AdbSource`) into `payloads\tools\adb\`
2. Publish a self-contained `win-x64` build of the WPF app
3. Compile the Inno Setup 7 wizard
4. Write checksums

### Output (`artifacts/installer/`, gitignored)

| File | Role |
| --- | --- |
| `SingularityApkInstaller-<version>-win-x64-setup.exe` | Versioned installer (Studio naming) |
| `SingularityApkInstaller-win-x64-setup.exe` | Stable name used by the README download button |
| `SHA256SUMS-<version>.txt` | SHA-256 of both exes |

Requires [Inno Setup 7](https://jrsoftware.org/isinfo.php). If `ISCC.exe` is missing, `pack.ps1` still publishes the app to `artifacts\publish\Installer.App`.

```powershell
# Skip the Inno compile
.\build\packaging\scripts\pack.ps1 -SkipInstaller

# Override version (must match csproj / release tag)
.\build\packaging\scripts\pack.ps1 -Version 0.1.1
```

Code signing is not implemented (`build/packaging/scripts/sign.ps1`). Testers may see SmartScreen on first run.

## Payload

| Path | Git | Packaged |
| --- | --- | --- |
| `payloads/current/app-manifest.json` | yes | yes |
| `payloads/current/*.apk` | gitignored | included when present |
| `payloads/tools/adb/` | gitignored | copied at pack time |

Missing APK is a tester-facing missing-payload message, not a pack failure.

## Publish a GitHub Release

1. Merge the work to `main`.
2. Tag `v<version>` (example: `v0.1.1`).
3. Attach the three files from `artifacts/installer/`.

```powershell
gh release create v0.1.1 `
  --title "APK Installer 0.1.1" `
  --notes-file docs/releases/v0.1.1.md `
  artifacts/installer/SingularityApkInstaller-0.1.1-win-x64-setup.exe `
  artifacts/installer/SingularityApkInstaller-win-x64-setup.exe `
  artifacts/installer/SHA256SUMS-0.1.1.txt
```

Keep the stable filename on every release so `/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe` keeps working.
