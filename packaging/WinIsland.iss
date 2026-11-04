; WinIsland.iss: the Inno Setup script that builds WinIsland-<version>-setup.exe (PRD FR-44).
; It installs WinIsland for the current Windows account only, without administrator rights, into
; %LOCALAPPDATA%\Programs\WinIsland, and lists it in Windows Settings > Apps with the publisher
; "Diva". Uninstalling removes the files, the Start menu shortcut, the start with Windows entry and
; the instant notifications package, and keeps %USERPROFILE%\.winisland (Diva, 4 Nov 2026).
; Build it with Build-Installer.ps1, which passes the version and the published app folder. It runs
; on GitHub's build machine, where Inno Setup is already installed; no certificate is used.

#ifndef AppVersion
  #error Pass the version, for example /DAppVersion=v1.6.0-beta.1 (Build-Installer.ps1 does this).
#endif
#ifndef NumericVersion
  #error Pass the numeric version, for example /DNumericVersion=1.6.0 (Build-Installer.ps1 does this).
#endif
#ifndef SourceFolder
  #error Pass the published app folder, for example /DSourceFolder=publish\WinIsland-v1.6.0-beta.1-win-x64.
#endif
#ifndef OutputFolder
  #define OutputFolder "."
#endif

[Setup]
; The id ties every version together, so a newer setup updates the installed WinIsland. Never change it.
AppId={{F4DCDDF9-E3D7-49B6-AA7D-0FCDE657FA12}
AppName=WinIsland
AppVersion={#AppVersion}
AppVerName=WinIsland {#AppVersion}
AppPublisher=Diva
VersionInfoVersion={#NumericVersion}
; For this account only, no administrator rights: {autopf} is then %LOCALAPPDATA%\Programs.
PrivilegesRequired=lowest
DefaultDirName={autopf}\WinIsland
; A fixed folder, so the instant notifications package (packaging/README.md) keeps pointing at it.
DisableDirPage=yes
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Windows 11 only.
MinVersion=10.0.22000
; Closes a running WinIsland before its files are replaced.
CloseApplications=force
RestartApplications=no
SetupIconFile=..\src\WinIsland.App\Assets\WinIsland.ico
UninstallDisplayIcon={app}\WinIsland.App.exe
UninstallDisplayName=WinIsland
WizardStyle=modern
Compression=lzma2/max
SolidCompression=yes
OutputDir={#OutputFolder}
OutputBaseFilename=WinIsland-{#AppVersion}-setup

[Files]
Source: "{#SourceFolder}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; The same name as the zip's Add-StartMenuShortcut.ps1 uses, so it replaces an old shortcut.
Name: "{autoprograms}\WinIsland"; Filename: "{app}\WinIsland.App.exe"

[Registry]
; WinIsland writes this start with Windows entry itself (its Settings); uninstall removes it.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "WinIsland"; ValueType: none; Flags: uninsdeletevalue dontcreatekey

[Run]
Filename: "{app}\WinIsland.App.exe"; Description: "Start WinIsland"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Quit WinIsland, so its files can be removed.
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM WinIsland.App.exe"; Flags: runhidden; RunOnceId: "QuitWinIsland"
; Remove the instant notifications package, if it was set up; it needs no administrator rights.
Filename: "powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -Command ""Get-AppxPackage WinIsland | Remove-AppxPackage"""; Flags: runhidden; RunOnceId: "RemoveIdentityPackage"

[UninstallDelete]
; Made by the instant notifications setup inside the WinIsland folder.
Type: filesandordirs; Name: "{app}\packaging\out"
Type: dirifempty; Name: "{app}"
