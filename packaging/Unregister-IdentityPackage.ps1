# Unregister-IdentityPackage.ps1: removes a registered identity package for the current user.
# Use it to undo Register-IdentityPackage.ps1, for example after the spike. The exe itself is
# not touched; it simply runs unpackaged again.

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$PackageName
)

$ErrorActionPreference = "Stop"

$package = Get-AppxPackage -Name $PackageName
if (-not $package) {
    Write-Host "Package '$PackageName' is not registered. Nothing to do."
    return
}

Remove-AppxPackage -Package $package.PackageFullName
Write-Host "Removed $($package.PackageFullName)"
