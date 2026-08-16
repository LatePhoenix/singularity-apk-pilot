# Manifest schema

Optional JSON at `payloads/current/app-manifest.json` for install policy and post-install notes. APK files are chosen in the app after a device is connected. Schema copy: `src/Installer.Contracts/Manifests/install-manifest.schema.json`.

## Fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `schemaVersion` | number | no | Default `1`. Bump when fields are incompatible. |
| `appId` | string | no | Android package name when known. Default `user.selected` (no `pm` verify). |
| `displayName` | string | no | Unused for user-picked files (file name is shown instead). |
| `buildVersion` | string | no | Tester-visible version label when bundling is re-enabled later. |
| `apkPath` | string | no | Unused in v0.2.0. Testers pick APKs in the UI. |
| `targetPlatforms` | string[] | no | `quest`, `android`, or both. Defaults to both. |
| `installPolicy` | string | no | See policies below. Default `ReinstallAllowDowngrade`. |
| `grantPermissions` | bool | no | Default `false` when present in JSON; shipped defaults set `true`. Adds `adb install -g`. |
| `allowTestApk` | bool | no | Default `false` when present in JSON; shipped defaults set `true`. Adds `-t`. |
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
  "installPolicy": "ReinstallAllowDowngrade",
  "grantPermissions": true,
  "allowTestApk": true,
  "targetPlatforms": ["quest", "android"],
  "postInstallNotes": {
    "quest": [
      "Open Library.",
      "Open the filter menu.",
      "Select Unknown Sources.",
      "Find the app you installed."
    ],
    "android": [
      "Find the app in your app drawer and open it."
    ]
  }
}
```

## Load rules

- Missing `app-manifest.json` is not an error. The app uses session defaults (`-r -d -t -g`) and asks for APK files after the device is ready.
- If `apkPath` is present, it is resolved relative to the app install directory, then the working directory. It is not required.
- APK files stay gitignored and are excluded from the Windows setup.
- `targetPlatforms` is used to warn when a detected device family is not listed; it does not block install in v1.
