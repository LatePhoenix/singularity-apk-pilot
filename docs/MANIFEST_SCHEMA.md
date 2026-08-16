# Manifest schema

One JSON file per packaged test build, shipped at `payloads/current/app-manifest.json`. Schema copy: `src/Installer.Contracts/Manifests/install-manifest.schema.json`.

## Fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `schemaVersion` | number | no | Default `1`. Bump when fields are incompatible. |
| `appId` | string | yes | Android package name, e.g. `com.singularity.exampleapp`. |
| `displayName` | string | yes | User-facing app name. |
| `buildVersion` | string | yes | Tester-visible version label. |
| `apkPath` | string | yes | Path relative to the payload root, e.g. `payloads/current/example-app.apk`. |
| `targetPlatforms` | string[] | yes | `quest`, `android`, or both. |
| `installPolicy` | string | yes | See policies below. |
| `grantPermissions` | bool | no | Default `false`. Adds `adb install -g`. |
| `allowTestApk` | bool | no | Default `false`. Adds `-t`. |
| `launchAfterInstall` | bool | no | Default `false`. Offer in-app launch when the device supports it. |
| `preferredDeviceFamilies` | string[] | no | Hints only: `meta-quest-2`, `meta-quest-3`, `pixel`, `samsung`. |
| `postInstallNotes` | object | no | `quest` / `android` string arrays shown on Complete. |
| `support.contactLabel` | string | no | Button/label text. |
| `support.contactEmail` | string | no | Used in diagnostics metadata, not auto-mailed. |

## `installPolicy`

- `InstallFresh`
- `ReinstallKeepData`
- `ReinstallAllowDowngrade`
- `UninstallThenInstall`
- `InstallTestBuild`

Unknown values fail manifest load. Do not guess.

## Example

```json
{
  "schemaVersion": 1,
  "appId": "com.singularity.exampleapp",
  "displayName": "Example App",
  "buildVersion": "0.9.3-test7",
  "apkPath": "payloads/current/example-app.apk",
  "targetPlatforms": ["quest", "android"],
  "installPolicy": "ReinstallAllowDowngrade",
  "grantPermissions": true,
  "allowTestApk": true,
  "launchAfterInstall": false,
  "preferredDeviceFamilies": ["meta-quest-2", "meta-quest-3", "pixel", "samsung"],
  "postInstallNotes": {
    "quest": [
      "Open Library.",
      "Open the filter menu.",
      "Select Unknown Sources.",
      "Launch Example App."
    ],
    "android": [
      "Find Example App in your app drawer and open it."
    ]
  },
  "support": {
    "contactLabel": "Send diagnostics to support",
    "contactEmail": "support@example.com"
  }
}
```

## Load rules

- Resolve `apkPath` relative to the app install directory, then the working directory.
- Missing APK is a startup error with a plain-language message, not a crash.
- GitHub Release **v0.1.0** does not attach a test APK. Operators add one under `payloads/current/` after installing the Windows setup, or before running `pack.ps1`.
- `targetPlatforms` is used to warn when a detected device family is not listed; it does not block install in v1.
