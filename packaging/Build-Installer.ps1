# Build-Installer.ps1: turns a published WinIsland folder into WinIsland-<version>-setup.exe with
# Inno Setup (PRD FR-44), using WinIsland.iss next to this script.
# Used by the Release and Installer workflows on GitHub's build machine, where Inno Setup is already
# installed. No certificate or key is used; the setup is not signed.
#
# Example: .\Build-Installer.ps1 -Version v1.6.0-beta.1 -SourceFolder ..\publish\WinIsland-v1.6.0-beta.1-win-x64 -OutputFolder ..\publish

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$Version,
    [Parameter(Mandatory)] [string]$SourceFolder,
    [Parameter(Mandatory)] [string]$OutputFolder
)

$ErrorActionPreference = "Stop"

# "v1.6.0-beta.1" gives 1.6.0 for Windows' file version, which takes numbers only.
if ($Version -notmatch '^v?(\d+\.\d+\.\d+)') {
    throw "The version '$Version' does not start with three numbers, like v1.6.0-beta.1."
}
$numericVersion = $Matches[1]

$source = (Resolve-Path $SourceFolder).Path
if (-not (Test-Path (Join-Path $source "WinIsland.App.exe"))) {
    throw "No WinIsland.App.exe in '$source'. Publish WinIsland there first."
}
New-Item -ItemType Directory -Force $OutputFolder | Out-Null
$output = (Resolve-Path $OutputFolder).Path

$compiler = Get-Command iscc.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $compiler) {
    $compiler = Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"
}
if (-not (Test-Path $compiler)) {
    throw "Inno Setup 6 (ISCC.exe) was not found. It is installed on GitHub's Windows build machines; this script is meant to run there."
}

& $compiler "/DAppVersion=$Version" "/DNumericVersion=$numericVersion" "/DSourceFolder=$source" "/DOutputFolder=$output" (Join-Path $PSScriptRoot "WinIsland.iss")
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup failed with exit code $LASTEXITCODE."
}

Write-Host "Built $(Join-Path $output "WinIsland-$Version-setup.exe")"
