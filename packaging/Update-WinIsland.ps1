# Update-WinIsland.ps1: installs the newest WinIsland release in one step (PRD FR-46).
# Started by the "Update" button in WinIsland's Settings and by the "Update WinIsland" shortcut in
# the Start menu; it can also be run by hand. WinIsland itself never goes online: this script does,
# through the GitHub CLI (gh) that is already signed in on this PC, because the repository is
# private. No password or token passes through WinIsland or this script.
#
# What it does:
#   1. Reads the installed WinIsland's version from its program file.
#   2. Asks GitHub for the releases, beta releases included, and picks the newest.
#   3. If that is newer: downloads its setup to a temporary folder, runs it silently (the setup
#      closes WinIsland, updates it in place and keeps the settings, FR-44), deletes the download
#      and starts WinIsland again. Otherwise it says WinIsland is up to date.
# With -CheckOnly it stops after step 2 and changes nothing.

[CmdletBinding()]
param(
    [switch]$CheckOnly
)

$ErrorActionPreference = "Stop"

$repository = "ardhana2819/WinIsland"
$releasesPage = "https://github.com/$repository/releases"
$secondsBeforeClosing = 10

$Host.UI.RawUI.WindowTitle = "Update WinIsland"

function Close-AfterSuccess {
    Write-Host ""
    Write-Host "This window closes in $secondsBeforeClosing seconds."
    Start-Sleep -Seconds $secondsBeforeClosing
    exit 0
}

# After a problem the window waits, so the message can be read.
function Close-AfterProblem([string]$message, [switch]$OfferReleasesPage) {
    Write-Host ""
    Write-Host $message -ForegroundColor Yellow
    if ($OfferReleasesPage) {
        $answer = Read-Host "Open the Releases page in your browser instead? (y/n)"
        if ($answer -match '^[yY]') {
            Start-Process $releasesPage
        }
    } else {
        Read-Host "Press Enter to close"
    }
    exit 1
}

# "v1.7.0-beta.1" or "1.7.0-beta.1" -> the three numbers and the part after "-" ("" for a release).
function ConvertTo-WinIslandVersion([string]$text) {
    if ($text -notmatch '^v?(\d+)\.(\d+)\.(\d+)(?:-([0-9A-Za-z.]+))?') {
        return $null
    }
    return [pscustomobject]@{
        Text = $text.TrimStart('v')
        Numbers = @([int]$Matches[1], [int]$Matches[2], [int]$Matches[3])
        Label = if ($Matches[4]) { $Matches[4] } else { "" }
    }
}

# Like semantic versioning: the numbers first; then a release beats a pre-release ("1.7.0" beats
# "1.7.0-beta.1"); then the labels part by part, numbers as numbers ("beta.2" beats "beta.1").
function Compare-WinIslandVersion($left, $right) {
    for ($index = 0; $index -lt 3; $index++) {
        if ($left.Numbers[$index] -ne $right.Numbers[$index]) {
            return [Math]::Sign($left.Numbers[$index] - $right.Numbers[$index])
        }
    }
    if ($left.Label -eq $right.Label) { return 0 }
    if ($left.Label -eq "") { return 1 }
    if ($right.Label -eq "") { return -1 }

    $leftParts = $left.Label.Split('.')
    $rightParts = $right.Label.Split('.')
    for ($index = 0; $index -lt [Math]::Min($leftParts.Count, $rightParts.Count); $index++) {
        $leftNumber = 0
        $rightNumber = 0
        $bothNumbers = [int]::TryParse($leftParts[$index], [ref]$leftNumber) -and [int]::TryParse($rightParts[$index], [ref]$rightNumber)
        $order = if ($bothNumbers) { [Math]::Sign($leftNumber - $rightNumber) } else { [Math]::Sign([string]::CompareOrdinal($leftParts[$index], $rightParts[$index])) }
        if ($order -ne 0) { return $order }
    }
    return [Math]::Sign($leftParts.Count - $rightParts.Count)
}

# A test build (for example "1.7.0-test.2", made to try a change before a release) is always
# replaced by the release with the same numbers.
function Test-ShouldUpdate($newest, $installed) {
    if ($installed.Label -like "test*" -and ($newest.Numbers -join ".") -eq ($installed.Numbers -join ".")) {
        return $true
    }
    return (Compare-WinIslandVersion $newest $installed) -gt 0
}

Write-Host "Update WinIsland"
Write-Host ""

# 1. The installed WinIsland: next to this script (installed or unzipped), or the installer's folder.
$installedFolder = Join-Path $env:LOCALAPPDATA "Programs\WinIsland"
$candidates = @((Join-Path $PSScriptRoot ".."), $installedFolder)
$program = $candidates | ForEach-Object { Join-Path $_ "WinIsland.App.exe" } | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $program) {
    Close-AfterProblem "WinIsland is not installed in $installedFolder. Install it once with its setup from the Releases page." -OfferReleasesPage
}
$installed = ConvertTo-WinIslandVersion (Get-Item $program).VersionInfo.ProductVersion
if (-not $installed) {
    Close-AfterProblem "Could not read the version of $program." -OfferReleasesPage
}
Write-Host "Installed: $($installed.Text)"

# 2. The newest release, through the GitHub CLI.
if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Close-AfterProblem ("The GitHub CLI is not installed. Install it once, then sign in:`n" +
        "  winget install GitHub.cli`n" +
        "  gh auth login") -OfferReleasesPage
}
gh auth status --hostname github.com *> $null
if ($LASTEXITCODE -ne 0) {
    Close-AfterProblem ("The GitHub CLI is not signed in. Sign in once with:`n" +
        "  gh auth login") -OfferReleasesPage
}

Write-Host "Checking GitHub for the newest release..."
$tags = gh release list --repo $repository --limit 30 --exclude-drafts --json tagName --jq ".[].tagName"
if ($LASTEXITCODE -ne 0 -or -not $tags) {
    Close-AfterProblem "Could not read the releases from GitHub. Check the internet connection and try again." -OfferReleasesPage
}
$newest = $null
foreach ($candidate in ($tags | ForEach-Object { ConvertTo-WinIslandVersion $_ } | Where-Object { $_ })) {
    if (-not $newest -or (Compare-WinIslandVersion $candidate $newest) -gt 0) { $newest = $candidate }
}
if (-not $newest) {
    Close-AfterProblem "GitHub lists no WinIsland release." -OfferReleasesPage
}
Write-Host "Newest release: $($newest.Text)"

if (-not (Test-ShouldUpdate $newest $installed)) {
    Write-Host ""
    Write-Host "WinIsland $($installed.Text) is up to date." -ForegroundColor Green
    Close-AfterSuccess
}
if ($CheckOnly) {
    Write-Host ""
    Write-Host "WinIsland $($newest.Text) is available (check only, nothing changed)." -ForegroundColor Green
    Close-AfterSuccess
}

# 3. Download, install silently, clean up, start again. The download is removed before any
#    message, so nothing is left behind.
$download = Join-Path $env:TEMP ("WinIsland-update-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $download | Out-Null
$problem = $null
Write-Host "Downloading WinIsland $($newest.Text)..."
gh release download "v$($newest.Text)" --repo $repository --pattern "*-setup.exe" --dir $download
$setup = Get-ChildItem $download -Filter "*-setup.exe" | Select-Object -First 1
if ($LASTEXITCODE -ne 0 -or -not $setup) {
    $problem = "Release v$($newest.Text) has no setup to download."
} else {
    Write-Host "Installing (WinIsland closes for a moment)..."
    $process = Start-Process $setup.FullName -ArgumentList "/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART" -Wait -PassThru
    if ($process.ExitCode -ne 0) {
        $problem = "The setup stopped with code $($process.ExitCode). WinIsland was not updated; try again, or run the setup from the Releases page."
    }
}
Remove-Item $download -Recurse -Force -ErrorAction SilentlyContinue
if ($problem) {
    Close-AfterProblem $problem -OfferReleasesPage
}

Start-Process (Join-Path $installedFolder "WinIsland.App.exe")
Write-Host ""
Write-Host "Updated to WinIsland $($newest.Text)." -ForegroundColor Green
Close-AfterSuccess
