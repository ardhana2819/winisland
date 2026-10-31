# Packaging

This folder will hold the package identity manifest and the scripts that register it.

Why: the Windows notification listener API may only work for apps that have package identity. A normal WPF app does not have one. A small "package with external location" (also called a sparse package) can give the app an identity without turning it into a full MSIX app.

Status: not set up yet. Spike S1 in Phase 1 checks whether it is needed and, if so, adds the manifest and scripts here with exact steps.

Rules:

- The signing certificate's private key (`.pfx`) is never stored in this repo. `.gitignore` blocks certificate and key files.
- Built packages go in `packaging/out/`, which is ignored by git.
