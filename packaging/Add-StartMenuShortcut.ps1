# Add-StartMenuShortcut.ps1: puts WinIsland in the Windows Start menu, so it is easy to start
# again after Quit (Diva's request, 31 Oct 2026). Creates a "WinIsland" shortcut in this user's
# Start menu programs folder; no admin rights, nothing for other users.
# Windows does not let apps pin themselves: to pin it, open Start > All, right-click WinIsland,
# and choose Pin to Start or Pin to taskbar.
#
# Without -AppFolder it finds WinIsland itself: the unzipped release this script came with, or
# the repo's Release build. Run it again after moving the WinIsland folder.
# To remove the shortcut: .\Add-StartMenuShortcut.ps1 -Remove

[CmdletBinding()]
param(
    [string]$AppFolder = "",
    [switch]$Remove
)

$ErrorActionPreference = "Stop"

$shortcutPath = Join-Path ([Environment]::GetFolderPath("Programs")) "WinIsland.lnk"

if ($Remove) {
    if (Test-Path $shortcutPath) {
        Remove-Item $shortcutPath
        Write-Host "Removed $shortcutPath"
    } else {
        Write-Host "There is no WinIsland shortcut in the Start menu. Nothing to do."
    }
    return
}

. (Join-Path $PSScriptRoot "WinIslandFolder.ps1")
$folder = Resolve-WinIslandFolder $AppFolder
$executable = Join-Path $folder "WinIsland.App.exe"

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $executable
$shortcut.WorkingDirectory = $folder
$shortcut.Description = "WinIsland: Dynamic Island for Windows"
$shortcut.Save()

Write-Host "Added WinIsland to the Start menu: $shortcutPath"
Write-Host "To pin it: Start > All, right-click WinIsland, Pin to Start."
