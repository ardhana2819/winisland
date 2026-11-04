# Build-IdentityPackage.ps1: builds and signs the identity package (.msix) for one exe.
# Fills in AppxManifest.template.xml, adds the logos that Windows requires, packs it with
# makeappx, and signs it with the certificate from New-DevCertificate.ps1.
# The logos come from Assets\ next to this script (the WinIsland icon, made from
# src\WinIsland.App\Assets\WinIsland.ico): Windows shows the 44 one on the taskbar for a window of
# a packaged app, so plain black placeholders made WinIsland's Settings window show a blank square
# there (Diva, 4 Nov 2026). Without that folder (the spike packages) it falls back to black.
# Signing uses the certificate's thumbprint from your certificate store, so no .pfx file or
# password is involved. Output goes to packaging\out\, which git ignores.
#
# Example (spike S1):
#   .\Build-IdentityPackage.ps1 -PackageName WinIsland.Spike.NotificationProbe `
#       -ApplicationId NotificationProbe -Executable NotificationProbe.exe `
#       -DisplayName "WinIsland NotificationProbe"

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$PackageName,
    [Parameter(Mandatory)] [string]$ApplicationId,
    [Parameter(Mandatory)] [string]$Executable,
    [Parameter(Mandatory)] [string]$DisplayName,
    [string]$Publisher = "CN=WinIsland Dev",
    [string]$Version = "1.0.0.0"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

function Find-SdkTool([string]$toolName) {
    $sdkBin = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
    $tool = Get-ChildItem $sdkBin -Recurse -Filter $toolName -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '\\x64\\' } |
        Sort-Object FullName -Descending |
        Select-Object -First 1
    if (-not $tool) {
        throw "$toolName not found. Install the Windows SDK: winget install Microsoft.WindowsSDK.10.0.26100"
    }
    return $tool.FullName
}

function New-PlaceholderLogo([string]$path, [int]$size) {
    # Windows requires logo files in the manifest even though AppListEntry="none" hides them.
    $logo = Join-Path $PSScriptRoot ("Assets\" + (Split-Path -Leaf $path))
    if (Test-Path $logo) {
        Copy-Item $logo $path
        return
    }

    $bitmap = New-Object System.Drawing.Bitmap $size, $size
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Black)
    $graphics.Dispose()
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

$certificate = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $Publisher } | Select-Object -First 1
if (-not $certificate) {
    throw "No certificate with subject '$Publisher' found. Run New-DevCertificate.ps1 first (as administrator)."
}

$outFolder = Join-Path $PSScriptRoot "out"
$stagingFolder = Join-Path $outFolder "staging-$PackageName"
$packagePath = Join-Path $outFolder "$PackageName.msix"
Remove-Item $stagingFolder -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force (Join-Path $stagingFolder "Assets") | Out-Null

$manifest = Get-Content (Join-Path $PSScriptRoot "AppxManifest.template.xml") -Raw
$manifest = $manifest.Replace("{{PackageName}}", $PackageName).
    Replace("{{ApplicationId}}", $ApplicationId).
    Replace("{{Executable}}", $Executable).
    Replace("{{DisplayName}}", $DisplayName).
    Replace("{{Publisher}}", $Publisher).
    Replace("{{Version}}", $Version)
Set-Content -Path (Join-Path $stagingFolder "AppxManifest.xml") -Value $manifest -Encoding UTF8

New-PlaceholderLogo (Join-Path $stagingFolder "Assets\StoreLogo.png") 50
New-PlaceholderLogo (Join-Path $stagingFolder "Assets\Square150x150Logo.png") 150
New-PlaceholderLogo (Join-Path $stagingFolder "Assets\Square44x44Logo.png") 44

$makeAppx = Find-SdkTool "makeappx.exe"
$signTool = Find-SdkTool "signtool.exe"

& $makeAppx pack /o /d $stagingFolder /p $packagePath /nv
if ($LASTEXITCODE -ne 0) { throw "makeappx failed with exit code $LASTEXITCODE" }

& $signTool sign /fd SHA256 /sha1 $certificate.Thumbprint /s My $packagePath
if ($LASTEXITCODE -ne 0) { throw "signtool failed with exit code $LASTEXITCODE" }

Write-Host "Built and signed: $packagePath"
Write-Host "Next: Register-IdentityPackage.ps1 -PackagePath `"$packagePath`" -ExternalLocation <folder with $Executable>"
