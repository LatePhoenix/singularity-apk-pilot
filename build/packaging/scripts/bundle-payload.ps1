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

$infDestDir = Join-Path $RepoRoot "payloads\tools\oculus-adb-drivers"

function Find-QuestUsbDriverDir {
    $named = @()
    if ($env:OCULUS_ADB_INF) {
        $named += $env:OCULUS_ADB_INF
    }
    $named += @(
        (Join-Path $RepoRoot "tools\oculus-adb-drivers\android_winusb.inf")
        (Join-Path $infDestDir "android_winusb.inf")
    )
    foreach ($path in $named) {
        if ([string]::IsNullOrWhiteSpace($path)) {
            continue
        }
        if (Test-Path $path -PathType Leaf) {
            return (Split-Path -Parent $path)
        }
        $nested = Join-Path $path "android_winusb.inf"
        if ((Test-Path $path -PathType Container) -and (Test-Path $nested)) {
            return $path
        }
    }

    $odh = Join-Path $env:APPDATA "odh\packages\other-packages\oculus-adb-drivers"
    if (Test-Path $odh) {
        $found = Get-ChildItem -Path $odh -Recurse -Filter "android_winusb.inf" -ErrorAction SilentlyContinue |
            Select-Object -First 1
        if ($found) {
            return $found.DirectoryName
        }
    }

    return $null
}

$infSourceDir = Find-QuestUsbDriverDir
if ($infSourceDir) {
    Write-Host "Copying Quest USB driver package from $infSourceDir"
    if (-not $DryRun) {
        New-Item -ItemType Directory -Force -Path $infDestDir | Out-Null
        $srcFull = (Resolve-Path $infSourceDir).Path
        $destFull = (Resolve-Path $infDestDir).Path
        if ($srcFull -ne $destFull) {
            Copy-Item -Path (Join-Path $srcFull "*") -Destination $destFull -Recurse -Force
        }
    }
}
else {
    Write-Warning "Quest USB driver package not found. Testers will get Get Quest USB support (Meta download page) instead of Install Quest USB support."
}

if (-not (Test-Path $manifest)) {
    throw "Missing $manifest"
}

$apk = Get-ChildItem (Join-Path $RepoRoot "payloads\current") -Filter *.apk -ErrorAction SilentlyContinue
if ($apk) {
    Write-Warning "APK files in payloads\current are not packaged. Testers choose APKs in the app after a device is connected."
}

Write-Host "Payload root: $(Join-Path $RepoRoot 'payloads')"
