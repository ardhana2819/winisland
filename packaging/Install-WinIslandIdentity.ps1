# Install-WinIslandIdentity.ps1: gives WinIsland package identity on this PC, in one step, so
# notifications arrive instantly instead of being checked once per second (spike S1).
# Optional: WinIsland works without it.
#
# What it does:
#   1. Checks that the signing certificate from New-DevCertificate.ps1 exists and is trusted.
#   2. Builds and signs the identity package for WinIsland.App.exe (Build-IdentityPackage.ps1).
#   3. Removes an earlier WinIsland registration, if any (Windows refuses a second one).
#   4. Registers the package, pointing at the folder that holds WinIsland.App.exe.
# Needs no admin rights. If the certificate is missing, run New-DevCertificate.ps1 once in a
# PowerShell opened with "Run as administrator" first.
#
# Example, from the repo after a Release build:
#   .\Install-WinIslandIdentity.ps1
# Example, for an unzipped release:
#   .\Install-WinIslandIdentity.ps1 -AppFolder "$env:LOCALAPPDATA\Programs\WinIsland"
# Restart WinIsland afterwards so it starts with its identity.

[CmdletBinding()]
param(
    [string]$AppFolder = (Join-Path $PSScriptRoot "..\src\WinIsland.App\bin\Release\net10.0-windows10.0.26100.0")
)

$ErrorActionPreference = "Stop"

# These must match the msix element in src/WinIsland.App/app.manifest.
$publisher = "CN=WinIsland Dev"
$packageName = "WinIsland"
$applicationId = "WinIsland"
$executable = "WinIsland.App.exe"

$resolvedFolder = (Resolve-Path $AppFolder -ErrorAction SilentlyContinue).Path
if (-not $resolvedFolder -or -not (Test-Path (Join-Path $resolvedFolder $executable))) {
    throw "Could not find $executable in '$AppFolder'. Build WinIsland in Release, or pass -AppFolder with the folder that holds $executable."
}

$certificate = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $publisher } | Select-Object -First 1
$trusted = Get-ChildItem Cert:\LocalMachine\TrustedPeople | Where-Object { $_.Subject -eq $publisher } | Select-Object -First 1
if (-not $certificate -or -not $trusted) {
    throw "The WinIsland signing certificate is missing or not trusted on this PC. Run New-DevCertificate.ps1 once in PowerShell opened with 'Run as administrator', then run this script again."
}

& (Join-Path $PSScriptRoot "Build-IdentityPackage.ps1") `
    -PackageName $packageName `
    -ApplicationId $applicationId `
    -Executable $executable `
    -DisplayName "WinIsland" `
    -Publisher $publisher

& (Join-Path $PSScriptRoot "Unregister-IdentityPackage.ps1") -PackageName $packageName

& (Join-Path $PSScriptRoot "Register-IdentityPackage.ps1") `
    -PackagePath (Join-Path $PSScriptRoot "out\$packageName.msix") `
    -ExternalLocation $resolvedFolder

Write-Host ""
Write-Host "Done. Quit WinIsland from the tray and start it again from $resolvedFolder."
