Read:

\- .cursor/index.mdc

\- .cursor/plans/master-implementation-plan.md

\- .cursor/rules/packaging-rules.mdc



Task:

Create Windows packaging support.



Create:

\- build/packaging/InnoSetup/Installer.iss

\- build/packaging/scripts/pack.ps1

\- build/packaging/scripts/bundle-payload.ps1

\- payload layout for adb tools and app manifest/apk



Requirements:

\- Install the WPF app.

\- Bundle portable adb in a predictable relative path.

\- Bundle the current app manifest and apk payload.

\- Add optional post-install launch checkbox using the correct Inno Setup \[Run] section behavior.

\- Keep paths relative and portable.

\- Do not implement code signing yet; leave clear TODO markers.



After changes:

Produced artifacts (gitignored under `artifacts/installer/`):

\- `SingularityApkInstaller-<version>-win-x64-setup.exe`

\- `SingularityApkInstaller-win-x64-setup.exe` (stable name for GitHub latest download)

\- `SHA256SUMS-<version>.txt`

Run:

```powershell
powershell -ExecutionPolicy Bypass -File build\packaging\scripts\pack.ps1
```

Prerequisite: Inno Setup 7 (`ISCC.exe`). Portable adb is copied from the Android SDK at pack time. Test APK is optional; missing APK is a tester message, not a pack failure.

Publish by tagging `v<version>` and attaching the three files. README always links the stable filename.

