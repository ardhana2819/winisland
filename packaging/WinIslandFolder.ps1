# WinIslandFolder.ps1: finds the folder that holds WinIsland.App.exe, for the other packaging
# scripts (they load this file with ". .\WinIslandFolder.ps1").
# In an unzipped release, the exe sits one folder above "packaging". In the repo, it is the Release
# build folder. A folder passed by the user always wins.

function Resolve-WinIslandFolder([string]$AppFolder) {
    $executable = "WinIsland.App.exe"
    $candidates = if ($AppFolder) {
        @($AppFolder)
    } else {
        @(
            (Join-Path $PSScriptRoot ".."),
            (Join-Path $PSScriptRoot "..\src\WinIsland.App\bin\Release\net10.0-windows10.0.26100.0")
        )
    }

    foreach ($candidate in $candidates) {
        $resolved = (Resolve-Path $candidate -ErrorAction SilentlyContinue).Path
        if ($resolved -and (Test-Path (Join-Path $resolved $executable))) {
            return $resolved
        }
    }

    throw "Could not find $executable. Unzip the release (or build WinIsland in Release), or pass -AppFolder with the folder that holds $executable."
}
