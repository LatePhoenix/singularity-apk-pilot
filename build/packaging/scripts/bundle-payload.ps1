# Bundle portable adb and confirm payload files exist.
# Examples:
#   .\bundle-payload.ps1
#   .\bundle-payload.ps1 -AdbSource "C:\Android\platform-tools"
#   .\bundle-payload.ps1 -DryRun

[CmdletBinding()]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path,
    [string]$AdbSource,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$adbDest = Join-Path $RepoRoot "payloads\tools\adb"
$manifest = Join-Path $RepoRoot "payloads\current\app-manifest.json"

if (-not $AdbSource) {
    $sdk = $env:ANDROID_HOME
    if (-not $sdk) { $sdk = $env:ANDROID_SDK_ROOT }
    if ($sdk) {
        $AdbSource = Join-Path $sdk "platform-tools"
    }
}

New-Item -ItemType Directory -Force -Path $adbDest | Out-Null

if ($AdbSource -and (Test-Path (Join-Path $AdbSource "adb.exe"))) {
    Write-Host "Copying adb from $AdbSource"
    if (-not $DryRun) {
        Copy-Item -Path (Join-Path $AdbSource "*") -Destination $adbDest -Recurse -Force
    }
}
else {
    Write-Warning "adb.exe not found. Place platform-tools in payloads\tools\adb or pass -AdbSource."
}

if (-not (Test-Path $manifest)) {
    throw "Missing $manifest"
}

$apk = Get-ChildItem (Join-Path $RepoRoot "payloads\current") -Filter *.apk -ErrorAction SilentlyContinue
if (-not $apk) {
    Write-Warning "No APK in payloads\current. Testers will get a missing-payload message until one is added."
}

Write-Host "Payload root: $(Join-Path $RepoRoot 'payloads')"
