# Sign published binaries when a certificate is configured.
# Unset credentials: pack still succeeds and this script logs "unsigned."
#
# PFX:
#   SIGNING_PFX              path to .pfx
#   SIGNING_PFX_PASSWORD     password (optional for empty-password files)
#   SIGNING_TIMESTAMP_URL    default http://timestamp.digicert.com
#
# Azure Trusted Signing (optional):
#   AZURE_TRUSTED_SIGNING_ACCOUNT
#   AZURE_TRUSTED_SIGNING_ENDPOINT
#   AZURE_TRUSTED_SIGNING_CERTIFICATE_PROFILE
#   AZURE_TRUSTED_SIGNING_DLIB           path to Azure.CodeSigning.Dlib.dll
#   AZURE_TRUSTED_SIGNING_METADATA       path to metadata JSON (optional)

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Path,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

function Get-SignTool {
    $cmd = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($cmd) {
        return $cmd.Source
    }

    $kits = Join-Path ${env:ProgramFiles(x86)} "Windows Kits\10\bin"
    if (Test-Path $kits) {
        $found = Get-ChildItem -Path $kits -Filter signtool.exe -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "\\x64\\" } |
            Select-Object -First 1
        if ($found) {
            return $found.FullName
        }
    }

    return $null
}

if (-not (Test-Path $Path)) {
    Write-Warning "Nothing to sign (missing): $Path"
    exit 0
}

$pfx = $env:SIGNING_PFX
$azureAccount = $env:AZURE_TRUSTED_SIGNING_ACCOUNT
$hasPfx = -not [string]::IsNullOrWhiteSpace($pfx)
$hasAzure = -not [string]::IsNullOrWhiteSpace($azureAccount) -and
    -not [string]::IsNullOrWhiteSpace($env:AZURE_TRUSTED_SIGNING_ENDPOINT) -and
    -not [string]::IsNullOrWhiteSpace($env:AZURE_TRUSTED_SIGNING_CERTIFICATE_PROFILE) -and
    -not [string]::IsNullOrWhiteSpace($env:AZURE_TRUSTED_SIGNING_DLIB)

if (-not $hasPfx -and -not $hasAzure) {
    Write-Host "unsigned (no SIGNING_PFX or Azure Trusted Signing env). Path: $Path"
    exit 0
}

$signtool = Get-SignTool
if (-not $signtool) {
    Write-Warning "signtool.exe not found. Leaving unsigned: $Path"
    exit 0
}

$timestamp = if ($env:SIGNING_TIMESTAMP_URL) { $env:SIGNING_TIMESTAMP_URL } else { "http://timestamp.digicert.com" }
$args = @("sign", "/fd", "SHA256", "/td", "SHA256", "/tr", $timestamp)

if ($hasPfx) {
    if (-not (Test-Path $pfx)) {
        throw "SIGNING_PFX does not exist: $pfx"
    }
    $args += @("/f", $pfx)
    if (-not [string]::IsNullOrWhiteSpace($env:SIGNING_PFX_PASSWORD)) {
        $args += @("/p", $env:SIGNING_PFX_PASSWORD)
    }
}
else {
    $metadata = $env:AZURE_TRUSTED_SIGNING_METADATA
    if ([string]::IsNullOrWhiteSpace($metadata)) {
        $metadata = Join-Path $env:TEMP "sai-trusted-signing.json"
        @{
            Endpoint = $env:AZURE_TRUSTED_SIGNING_ENDPOINT
            CodeSigningAccountName = $env:AZURE_TRUSTED_SIGNING_ACCOUNT
            CertificateProfileName = $env:AZURE_TRUSTED_SIGNING_CERTIFICATE_PROFILE
        } | ConvertTo-Json | Set-Content -Path $metadata -Encoding utf8
    }
    $args += @("/dlib", $env:AZURE_TRUSTED_SIGNING_DLIB, "/dmdf", $metadata)
}

$args += $Path
Write-Host "Signing $Path"
if ($DryRun) {
    Write-Host "DryRun: $signtool $($args -join ' ')"
    exit 0
}

& $signtool @args
if ($LASTEXITCODE -ne 0) {
    throw "signtool failed with exit $LASTEXITCODE for $Path"
}
