# Install-WinIslandIdentity.ps1: gives WinIsland package identity on this PC, in one step, so
# notifications arrive instantly instead of being checked once per second (spike S1).
# Optional: WinIsland works without it.
#
# What it does:
#   1. Checks that the signing certificate from New-DevCertificate.ps1 exists and is trusted.
#   2. Builds and signs the identity package for WinIsland.App.exe (Build-IdentityPackage.ps1).
#   3. Removes an earlier WinIsland registration, if any (Windows refuses a second one).
#   4. Registers the package, pointing at the folder that holds WinIsland.App.exe.
#   5. Removes WinIsland's old start with Windows entry from the registry's Run list. With identity,
#      WinIsland starts with Windows through the package's startup task instead (PRD FR-20), and it
#      cannot remove that entry itself, because Windows shows it only a private copy of the list.
# Needs no admin rights. If the certificate is missing, run New-DevCertificate.ps1 once in a
# PowerShell opened with "Run as administrator" first.
#
# Without -AppFolder it finds WinIsland itself: the unzipped release this script came with, or the
# repo's Release build. Example with a folder:
#   .\Install-WinIslandIdentity.ps1 -AppFolder "$env:LOCALAPPDATA\Programs\WinIsland"
# Restart WinIsland afterwards so it starts with its identity.

[CmdletBinding()]
param(
    [string]$AppFolder = ""
)

$ErrorActionPreference = "Stop"

# Package version: raise it whenever AppxManifest.template.xml changes, otherwise Windows may keep
# using the manifest of the earlier registration with the same version.
$packageVersion = "1.0.3.0"

# These must match the msix element in src/WinIsland.App/app.manifest.
$publisher = "CN=WinIsland Dev"
$packageName = "WinIsland"
$applicationId = "WinIsland"
$executable = "WinIsland.App.exe"

. (Join-Path $PSScriptRoot "WinIslandFolder.ps1")
$resolvedFolder = Resolve-WinIslandFolder $AppFolder

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
    -Version $packageVersion `
    -Publisher $publisher

& (Join-Path $PSScriptRoot "Unregister-IdentityPackage.ps1") -PackageName $packageName

& (Join-Path $PSScriptRoot "Register-IdentityPackage.ps1") `
    -PackagePath (Join-Path $PSScriptRoot "out\$packageName.msix") `
    -ExternalLocation $resolvedFolder

# Step 5. This script runs outside the package, so it changes the real Run list. WinIsland turns its
# startup task on at its next start when "Launch with Windows" is on.
$runKey = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
if (Get-ItemProperty -Path $runKey -Name "WinIsland" -ErrorAction SilentlyContinue) {
    Remove-ItemProperty -Path $runKey -Name "WinIsland"
    Write-Host "Removed the old start with Windows entry; the package's startup task replaces it."
}

Write-Host ""
Write-Host "Done. Quit WinIsland from the tray and start it again from $resolvedFolder."
