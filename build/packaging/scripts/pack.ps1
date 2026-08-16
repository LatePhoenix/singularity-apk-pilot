# Publish the WPF app and optionally compile the Inno Setup installer.
# Examples:
#   .\pack.ps1
#   .\pack.ps1 -SkipInstaller
#   .\pack.ps1 -Configuration Release -DryRun

[CmdletBinding()]
param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path,
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$SkipInstaller,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$publishDir = Join-Path $RepoRoot "artifacts\publish\Installer.App"
$iss = Join-Path $RepoRoot "build\packaging\InnoSetup\Installer.iss"
$bundle = Join-Path $PSScriptRoot "bundle-payload.ps1"

Write-Host "Bundling payload..."
if (-not $DryRun) {
    & $bundle -RepoRoot $RepoRoot
}

$project = Join-Path $RepoRoot "src\Installer.App\Installer.App.csproj"
Write-Host "Publishing $project"
if (-not $DryRun) {
    New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
    dotnet publish $project -c $Configuration -r win-x64 --self-contained false -o $publishDir
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
    & $iscc $iss
}

Write-Host "Done."
