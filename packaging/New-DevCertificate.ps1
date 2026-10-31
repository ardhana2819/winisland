# New-DevCertificate.ps1: creates a personal self-signed code signing certificate for the
# identity package, and trusts it on this PC so Windows accepts the signed package.
#
# Run it yourself in PowerShell "Run as administrator" (trusting a certificate needs admin).
# The private key is created inside your Windows certificate store (CurrentUser\My), marked
# non-exportable, so it never exists as a file and can never end up in the repo.
# The public certificate (.cer) is exported to packaging\out\, which git ignores.

[CmdletBinding()]
param(
    [string]$Subject = "CN=WinIsland Dev"
)

$ErrorActionPreference = "Stop"

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    throw "Please run this script in PowerShell opened with 'Run as administrator'."
}

$existing = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -eq $Subject }
if ($existing) {
    Write-Host "A certificate with subject '$Subject' already exists (thumbprint $($existing[0].Thumbprint)). Reusing it."
    $certificate = $existing[0]
}
else {
    # 1.3.6.1.5.5.7.3.3 = code signing. 2.5.29.19 with empty value = not a certificate authority.
    $certificate = New-SelfSignedCertificate `
        -Type Custom `
        -Subject $Subject `
        -FriendlyName "WinIsland Dev package signing" `
        -KeyUsage DigitalSignature `
        -KeyExportPolicy NonExportable `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -NotAfter (Get-Date).AddYears(3) `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
    Write-Host "Created certificate. Thumbprint: $($certificate.Thumbprint)"
}

$outFolder = Join-Path $PSScriptRoot "out"
New-Item -ItemType Directory -Force $outFolder | Out-Null
$publicCertificatePath = Join-Path $outFolder "WinIslandDev.cer"
Export-Certificate -Cert $certificate -FilePath $publicCertificatePath | Out-Null
Write-Host "Exported public certificate (no private key) to $publicCertificatePath"

Import-Certificate -FilePath $publicCertificatePath -CertStoreLocation "Cert:\LocalMachine\TrustedPeople" | Out-Null
Write-Host "Trusted the certificate in LocalMachine\TrustedPeople."
Write-Host "Done. Next: run Build-IdentityPackage.ps1 in a normal (non-admin) PowerShell."
