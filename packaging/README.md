# Packaging

Scripts and a manifest template that give WinIsland **package identity**, using a "package with external location" (also called a sparse package). WinIsland stays a normal WPF program in its own folder; a small signed `.msix` registers an identity that points at that folder.

**Why, and is it needed?** It is optional. Spike S1 showed WinIsland can read notifications without identity by checking once per second. With identity, Windows pushes each notification at once: measured on Diva's PC, banners appeared 0.07 to 0.11 s after the notification instead of up to 1 s. CPU use measured the same either way.

## Set it up on this PC

1. **Once per PC, as administrator:** open PowerShell with "Run as administrator" and run `.\New-DevCertificate.ps1`. It creates the signing certificate and trusts it. (Already done on Diva's PC during the spike.)
2. **Each time WinIsland's folder changes:** in a normal PowerShell, run `.\Install-WinIslandIdentity.ps1`. Run from a WinIsland folder's own `packaging` folder (installed or unzipped), it uses that WinIsland; run from the repo, it uses the repo's Release build folder. For another folder pass `-AppFolder "<folder with WinIsland.App.exe>"`, for example `-AppFolder "$env:LOCALAPPDATA\Programs\WinIsland"` for the installed WinIsland (v1.6). The installer keeps that folder fixed, so this is needed once, not after each update.
3. Quit WinIsland from the tray and start it again from that folder.

To undo: `.\Unregister-IdentityPackage.ps1 -PackageName WinIsland`. WinIsland then simply checks notifications once per second again.

## Files

| File | What it does | Needs admin? |
|---|---|---|
| `Add-StartMenuShortcut.ps1` | Puts WinIsland in this user's Start menu (`-Remove` takes it out again). Pin it from Start > All yourself; Windows does not let apps pin themselves. | No |
| `Install-WinIslandIdentity.ps1` | One step for WinIsland: checks the certificate, builds and signs the package, replaces an earlier registration, registers it for the folder with `WinIsland.App.exe`. | No |
| `WinIslandFolder.ps1` | Shared helper: finds the folder with `WinIsland.App.exe` (the unzipped release, or the repo's Release build). | |
| `Assets\` | The package's logos, made from the WinIsland icon. Windows shows the 44 one on the taskbar for WinIsland's Settings window. | |
| `AppxManifest.template.xml` | Manifest with `{{...}}` placeholders. Declares external content, full trust, and the `userNotificationListener` capability. | |
| `New-DevCertificate.ps1` | Creates a self-signed code signing certificate (`CN=WinIsland Dev`) in your certificate store and trusts it on this PC. | **Yes** |
| `Build-IdentityPackage.ps1` | Fills in the template, packs it with `makeappx`, signs it with `signtool`. | No |
| `Register-IdentityPackage.ps1` | Registers a signed package for your user, pointing at the exe's folder. | No |
| `Unregister-IdentityPackage.ps1` | Removes a registered package again. | No |
| `Update-WinIsland.ps1` | Installs the newest WinIsland release through the GitHub CLI signed in on this PC (PRD FR-46). Started by Settings > Update and by Start > Update WinIsland. `-CheckOnly` only reports. | No |
| `Publish-WinIsland.ps1` | Build only (not shipped): makes the self-contained folder a release ships, with these scripts in it. Used by the Release and Installer workflows. | No |
| `WinIsland.iss`, `Build-Installer.ps1` | Build only (not shipped): the Inno Setup script for `WinIsland-<version>-setup.exe` (PRD FR-44) and the script that builds it on GitHub's build machine. Uninstalling removes the identity package too. | No |

The package turns off Windows' file and registry virtualization (`unvirtualizedResources`), so WinIsland's own files stay where it expects them. Start with Windows is the exception: Windows still shows the package a private copy of the registry's Run list, so with identity WinIsland starts with Windows through the package's startup task (`WinIslandStartup`, visible in Windows Settings > Apps > Startup), and `Install-WinIslandIdentity.ps1` removes the old Run entry (PRD FR-20). An identity package registered before v1.0.3.0 has no startup task: run `Install-WinIslandIdentity.ps1` again, otherwise Settings says so under Launch with Windows. Raise `$packageVersion` in `Install-WinIslandIdentity.ps1` whenever the manifest template changes; with the same version Windows may keep the old manifest.

WinIsland.App.exe embeds a matching `msix` element in `src/WinIsland.App/app.manifest` (publisher `CN=WinIsland Dev`, package `WinIsland`, application `WinIsland`). Without a registered package, Windows ignores it and WinIsland runs as a normal app.

## Keys and certificates

- The private key is created inside your Windows certificate store (`CurrentUser\My`) as **non-exportable**. It never exists as a file, so it cannot end up in the repo.
- Signing uses the certificate's thumbprint from the store, so no `.pfx` file or password is involved.
- The public certificate is exported to `packaging/out/WinIslandDev.cer`. Everything in `packaging/out/` is ignored by git, and `.gitignore` also blocks all certificate and key file types.
- Requirements on the PC that builds the package: Windows SDK (`makeappx.exe`, `signtool.exe`), installed with `winget install Microsoft.WindowsSDK.10.0.26100`.

## Another PC

Copy the `packaging` folder (or clone the repo) to the other PC, then pick one:

- **Option A: reuse this PC's signature (no Windows SDK there).** After running `Install-WinIslandIdentity.ps1` here, copy `packaging/out/WinIslandDev.cer` (public certificate only) and `packaging/out/WinIsland.msix` to the other PC. There, in PowerShell opened as administrator, trust the certificate once:
  `Import-Certificate -FilePath WinIslandDev.cer -CertStoreLocation Cert:\LocalMachine\TrustedPeople`
  Then, in a normal PowerShell: `.\Register-IdentityPackage.ps1 -PackagePath WinIsland.msix -ExternalLocation "<folder with WinIsland.App.exe>"`. The private key never leaves this PC.
- **Option B: sign on the other PC.** Install the Windows SDK there, run `New-DevCertificate.ps1` as administrator, then `Install-WinIslandIdentity.ps1 -AppFolder "<folder with WinIsland.App.exe>"`. Each PC has its own certificate.

Both options need administrator rights once, to trust the certificate.

## The spike package

`WinIsland.Spike.NotificationProbe` was registered during spike S1. It is not used by WinIsland and can be removed with `.\Unregister-IdentityPackage.ps1 -PackageName WinIsland.Spike.NotificationProbe`.
