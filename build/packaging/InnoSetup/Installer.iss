; Singularity APK Installer — Inno Setup 7 script
; Signing is handled by pack.ps1 → sign.ps1 when SIGNING_PFX or Azure Trusted Signing env is set.

#define MyAppName "Singularity APK Installer"
#define MyAppPublisher "Singularity Solutions and Services"
#define MyAppExeName "SingularityApkInstaller.exe"

#ifndef MyAppVersion
  #define MyAppVersion "0.4.0"
#endif

#ifndef PublishDir
  #define PublishDir "..\\..\\..\\artifacts\\publish\\Installer.App"
#endif

#ifndef PayloadDir
  #define PayloadDir "..\\..\\..\\payloads"
#endif

#ifndef OutputDir
  #define OutputDir "..\\..\\..\\artifacts\\installer"
#endif

[Setup]
AppId={{A7C3E8D1-4B2F-4E9A-9C11-8F3D2B1A0C77}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Singularity\APK Installer
DefaultGroupName={#MyAppName}
OutputDir={#OutputDir}
OutputBaseFilename=SingularityApkInstaller-{#MyAppVersion}-win-x64-setup
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}
AppPublisherURL=https://github.com/LatePhoenix/singularity-apk-installer
AppSupportURL=https://github.com/LatePhoenix/singularity-apk-installer/issues
AppUpdatesURL=https://github.com/LatePhoenix/singularity-apk-installer/releases/latest
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\..\..\src\Installer.App\Assets\app-icon.ico
; TODO(signing): SignTool=signtool

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PayloadDir}\*"; DestDir: "{app}\payloads"; Flags: ignoreversion recursesubdirs createallsubdirs; Excludes: "*.apk"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent unchecked
