; Singularity APK Installer — Inno Setup 7 script
; TODO(signing): add SignTool once a code-signing certificate is available.

#define MyAppName "Singularity APK Installer"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Singularity Solutions and Services"
#define MyAppExeName "SingularityApkInstaller.exe"

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
OutputBaseFilename=SingularityApkInstaller-{#MyAppVersion}
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
Source: "{#PayloadDir}\*"; DestDir: "{app}\payloads"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent unchecked
