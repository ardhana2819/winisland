# Publish-WinIsland.ps1: builds the self-contained win-x64 WinIsland folder that a release ships,
# as a zip and inside the installer (PRD FR-44). No .NET install is needed on the target PC.
# The folder also gets the packaging scripts a user needs (Start menu shortcut, optional instant
# notifications), but never packaging\out, certificates or keys, and not the build-only scripts.
# Used by the Release and Installer workflows; it prints the folder it made.
#
# Example: .\Publish-WinIsland.ps1 -Version v1.6.0-beta.1 -OutputFolder ..\publish

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$Version,
    [Parameter(Mandatory)] [string]$OutputFolder
)

$ErrorActionPreference = "Stop"

$repository = Split-Path -Parent $PSScriptRoot
$appFolder = Join-Path $OutputFolder "WinIsland-$Version-win-x64"

dotnet publish (Join-Path $repository "src\WinIsland.App\WinIsland.App.csproj") -c Release -r win-x64 --self-contained true -o $appFolder
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$buildOnly = @("Build-Installer.ps1", "Publish-WinIsland.ps1")
$packaging = Join-Path $appFolder "packaging"
New-Item -ItemType Directory -Force $packaging | Out-Null
Get-ChildItem $PSScriptRoot -File |
    Where-Object { $_.Extension -in ".ps1", ".xml" -or $_.Name -eq "README.md" } |
    Where-Object { $_.Name -notin $buildOnly } |
    Copy-Item -Destination $packaging
# The identity package's logos (Build-IdentityPackage.ps1).
Copy-Item (Join-Path $PSScriptRoot "Assets") -Destination $packaging -Recurse

Write-Output (Resolve-Path $appFolder).Path
