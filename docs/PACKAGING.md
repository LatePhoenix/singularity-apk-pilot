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
2. Publish a self-contained **single-file** `win-x64` build of the WPF app
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
.\build\packaging\scripts\pack.ps1 -Version 0.4.0
```

Code signing runs from `pack.ps1` → `build/packaging/scripts/sign.ps1` when credentials are present. If unset, pack still succeeds and logs **unsigned**. Testers may see SmartScreen on first run of an unsigned build.

### Authenticode (optional)

Purchase an OV or EV code-signing certificate from a public CA (or use Azure Trusted Signing). Do not invent a signature.

**PFX (local):**

```powershell
$env:SIGNING_PFX = "C:\certs\codesign.pfx"
$env:SIGNING_PFX_PASSWORD = "<password>"
# optional: $env:SIGNING_TIMESTAMP_URL = "http://timestamp.digicert.com"
.\build\packaging\scripts\pack.ps1 -Version 0.4.0
```

**Azure Trusted Signing:**

```powershell
$env:AZURE_TRUSTED_SIGNING_ACCOUNT = "<account>"
$env:AZURE_TRUSTED_SIGNING_ENDPOINT = "https://<region>.codesigning.azure.net/"
$env:AZURE_TRUSTED_SIGNING_CERTIFICATE_PROFILE = "<profile>"
$env:AZURE_TRUSTED_SIGNING_DLIB = "C:\path\Azure.CodeSigning.Dlib.dll"
# optional: $env:AZURE_TRUSTED_SIGNING_METADATA = "C:\path\metadata.json"
.\build\packaging\scripts\pack.ps1 -Version 0.4.0
```

`signtool.exe` must be on PATH or under Windows Kits 10 `bin\**\x64`. The published `SingularityApkInstaller.exe` is signed before Inno Setup compiles, then both setup exes are signed, then checksums are written.

## Payload

| Path | Git | Packaged |
| --- | --- | --- |
| `payloads/current/app-manifest.json` | yes | yes (install policy / notes only) |
| `payloads/current/*.apk` | gitignored | **not packaged** — testers choose APKs in the app |
| `payloads/tools/adb/` | gitignored | copied at pack time |

APK files in `payloads/current` are excluded from the setup. Testers pick files after a device is connected.

## Publish a GitHub Release

1. Merge the work to `main`.
2. Tag `v<version>` (example: `v0.4.0`).
3. Attach the three files from `artifacts/installer/`.

```powershell
gh release create v0.4.0 `
  --title "APK Installer 0.4.0" `
  --notes-file docs/releases/v0.4.0.md `
  artifacts/installer/SingularityApkInstaller-0.4.0-win-x64-setup.exe `
  artifacts/installer/SingularityApkInstaller-win-x64-setup.exe `
  artifacts/installer/SHA256SUMS-0.4.0.txt
```

Keep the stable filename on every release so `/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe` keeps working.
