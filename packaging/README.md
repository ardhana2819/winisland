# Packaging

Scripts and a manifest template that give a normal desktop exe **package identity**, using a "package with external location" (also called a sparse package). The exe stays a normal WPF or console program in its own folder; a small signed `.msix` registers an identity that points at that folder.

Why: some Windows APIs only work for apps with identity. Spike S1 checks whether WinIsland's notification listener needs it. The first unpackaged run showed that polling works without identity, but the push event does not (see `docs/spike-results.md`). If the event does not work with identity either, the product may not need this folder at all.

## Files

| File | What it does | Needs admin? |
|---|---|---|
| `AppxManifest.template.xml` | Manifest with `{{...}}` placeholders. Declares external content, full trust, and the `userNotificationListener` capability. | |
| `New-DevCertificate.ps1` | Creates a self-signed code signing certificate (`CN=WinIsland Dev`) in your certificate store and trusts it on this PC. | **Yes** |
| `Build-IdentityPackage.ps1` | Fills in the template, packs it with `makeappx`, signs it with `signtool`. | No |
| `Register-IdentityPackage.ps1` | Registers the signed package for your user, pointing at the exe's folder. | No |
| `Unregister-IdentityPackage.ps1` | Removes the registered package again. | No |

The exe must embed a matching `app.manifest` with an `msix` element (same publisher, package name, and application id). See `spikes/NotificationProbe/app.manifest`.

## Keys and certificates

- The private key is created inside your Windows certificate store (`CurrentUser\My`) as **non-exportable**. It never exists as a file, so it cannot end up in the repo.
- Signing uses the certificate's thumbprint from the store, so no `.pfx` file or password is involved.
- The public certificate is exported to `packaging/out/WinIslandDev.cer`. Everything in `packaging/out/` is ignored by git, and `.gitignore` also blocks all certificate and key file types.
- Requirements on this PC: Windows SDK (`makeappx.exe`, `signtool.exe`), installed with `winget install Microsoft.WindowsSDK.10.0.26100`.

## What another PC needs

Only relevant if S1 shows identity is needed. Two options, to be decided when packaging the real app:

- **Option A: reuse this PC's signature.** Copy `WinIslandDev.cer` (public certificate only) and the signed `.msix` to the other PC. There, in an admin PowerShell, trust the certificate:
  `Import-Certificate -FilePath WinIslandDev.cer -CertStoreLocation Cert:\LocalMachine\TrustedPeople`
  Then run `Register-IdentityPackage.ps1` pointing at the folder where WinIsland was unzipped. No Windows SDK needed there. The private key never leaves this PC. A new `.msix` has to be signed here whenever the manifest or version changes.
- **Option B: sign on the other PC.** Install the Windows SDK there, then run `New-DevCertificate.ps1` (admin), `Build-IdentityPackage.ps1`, and `Register-IdentityPackage.ps1`. Each PC has its own certificate.

Both options need administrator rights once, to trust the certificate.
