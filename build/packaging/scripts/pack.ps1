# Publish the WPF app and compile the Inno Setup installer.
# Examples:
#   .\pack.ps1
#   .\pack.ps1 -SkipInstaller
#   .\pack.ps1 -Configuration Release -DryRun

[CmdletBinding()]
param(
    [string]$RepoRoot,
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$Version = "0.1.0",
    [switch]$SkipInstaller,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

if (-not $RepoRoot) {
    $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
}

$publishDir = Join-Path $RepoRoot "artifacts\publish\Installer.App"
$installerDir = Join-Path $RepoRoot "artifacts\installer"
$iss = Join-Path $RepoRoot "build\packaging\InnoSetup\Installer.iss"
$bundle = Join-Path $PSScriptRoot "bundle-payload.ps1"
$project = Join-Path $RepoRoot "src\Installer.App\Installer.App.csproj"
$versionedName = "SingularityApkInstaller-$Version-win-x64-setup.exe"
$stableName = "SingularityApkInstaller-win-x64-setup.exe"

Write-Host "Bundling payload..."
if (-not $DryRun) {
    & $bundle -RepoRoot $RepoRoot
}

Write-Host "Publishing $project (win-x64, self-contained $Version)"
if (-not $DryRun) {
    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
    dotnet publish $project `
        -c $Configuration `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=false `
        -p:PublishReadyToRun=true `
        -p:DebugType=none `
        -p:Version=$Version `
        -o $publishDir
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit $LASTEXITCODE"
    }
}

if ($SkipInstaller) {
    Write-Host "Published to $publishDir"
    return
}

$iscc = @(
    "${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 7\ISCC.exe",
    "${env:LocalAppData}\Programs\Inno Setup 7\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    Write-Warning "Inno Setup 7 ISCC.exe not found. Install Inno Setup 7, then re-run pack.ps1."
    Write-Host "Published app is at $publishDir"
    return
}

Write-Host "Compiling installer with $iscc"
if (-not $DryRun) {
    New-Item -ItemType Directory -Force -Path $installerDir | Out-Null
    & $iscc $iss "/DMyAppVersion=$Version"
    if ($LASTEXITCODE -ne 0) {
        throw "ISCC failed with exit $LASTEXITCODE"
    }

    $versionedPath = Join-Path $installerDir $versionedName
    if (-not (Test-Path $versionedPath)) {
        throw "Expected installer not found: $versionedPath"
    }

    Copy-Item $versionedPath (Join-Path $installerDir $stableName) -Force

    $sums = Join-Path $installerDir "SHA256SUMS-$Version.txt"
    $lines = foreach ($name in @($versionedName, $stableName)) {
        $hash = (Get-FileHash (Join-Path $installerDir $name) -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $name"
    }
    Set-Content -Path $sums -Value $lines -Encoding utf8
    Write-Host "Installer: $versionedPath"
    Write-Host "Stable:    $(Join-Path $installerDir $stableName)"
    Write-Host "Checksums: $sums"
}

Write-Host "Done."
