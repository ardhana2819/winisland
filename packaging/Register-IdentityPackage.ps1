# Register-IdentityPackage.ps1: registers a signed identity package for the current user and
# points it at the folder that holds the real exe (the "external location").
# After this, starting the exe from that folder gives it package identity.
# Needs no admin rights, but the signing certificate must already be trusted on this PC.

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$PackagePath,
    [Parameter(Mandatory)] [string]$ExternalLocation
)

$ErrorActionPreference = "Stop"

$resolvedPackage = (Resolve-Path $PackagePath).Path
$resolvedLocation = (Resolve-Path $ExternalLocation).Path

Add-AppxPackage -Path $resolvedPackage -ExternalLocation $resolvedLocation
Write-Host "Registered $resolvedPackage with external location $resolvedLocation"
