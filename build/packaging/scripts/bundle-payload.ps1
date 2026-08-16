# Bundle portable adb and confirm payload files exist.
# Examples:
#   .\bundle-payload.ps1
#   .\bundle-payload.ps1 -AdbSource "C:\Android\platform-tools"
#   .\bundle-payload.ps1 -DryRun

[CmdletBinding()]
param(
    [string]$RepoRoot,
    [string]$AdbSource,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

if (-not $RepoRoot) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
}

$adbDest = Join-Path $RepoRoot "payloads\tools\adb"
$manifest = Join-Path $RepoRoot "payloads\current\app-manifest.json"

function Find-PlatformTools {
    param([string]$Explicit)
    if ($Explicit -and (Test-Path (Join-Path $Explicit "adb.exe"))) {
        return (Resolve-Path $Explicit).Path
    }

    $sdkRoots = @(
        $env:ANDROID_HOME,
        $env:ANDROID_SDK_ROOT,
        (Join-Path $env:LOCALAPPDATA "Android\Sdk")
    ) | Where-Object { $_ }

    foreach ($root in $sdkRoots) {
        $pt = Join-Path $root "platform-tools"
        if (Test-Path (Join-Path $pt "adb.exe")) {
            return $pt
        }
    }

    $onPath = Get-Command adb.exe -ErrorAction SilentlyContinue
    if ($onPath) {
        return (Split-Path -Parent $onPath.Source)
    }

    return $null
}

$AdbSource = Find-PlatformTools -Explicit $AdbSource

New-Item -ItemType Directory -Force -Path $adbDest | Out-Null

if ($AdbSource) {
    Write-Host "Copying adb from $AdbSource"
    if (-not $DryRun) {
        Copy-Item -Path (Join-Path $AdbSource "*") -Destination $adbDest -Force
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
