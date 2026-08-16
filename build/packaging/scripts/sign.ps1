# TODO(signing): invoke signtool after a certificate is configured.
# Example (do not run until SignTool and a cert are available):
#   signtool sign /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 /a <file>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Path,
    [switch]$DryRun
)

Write-Warning "Code signing is not implemented. Path: $Path DryRun: $DryRun"
exit 0
