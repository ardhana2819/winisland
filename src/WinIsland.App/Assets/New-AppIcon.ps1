# New-AppIcon.ps1: draws WinIsland's app icon, concept B "Mono pill" (PRD FR-34, DESIGN_SPEC
# section 10), and saves it next to this script as WinIsland.ico.
# The icon is drawn from the values in the design spec instead of being exported from a drawing
# program, so anyone can make it again and check it matches the spec. Run it only when the icon
# design changes; the .ico it makes is kept in the repo, because Windows needs a real .ico file
# for the exe icon.
#
# Uses WPF, which ships with Windows, so nothing has to be installed. Run it in Windows PowerShell:
#   .\New-AppIcon.ps1

[CmdletBinding()]
param(
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
# Windows PowerShell does not fill $PSScriptRoot inside param defaults, so the default is set here.
if (-not $OutputPath) {
    $OutputPath = Join-Path $PSScriptRoot "WinIsland.ico"
}
Add-Type -AssemblyName PresentationCore, WindowsBase

# The sizes Windows asks for in File Explorer, the Start menu, the taskbar and title bars.
$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)

function New-Brush([string]$hex) {
    $brush = New-Object System.Windows.Media.SolidColorBrush ([System.Windows.Media.ColorConverter]::ConvertFromString($hex))
    $brush.Freeze()
    return $brush
}

function New-Rect([double]$x, [double]$y, [double]$width, [double]$height) {
    return New-Object System.Windows.Rect $x, $y, $width, $height
}

# Draws the icon on the spec's 256 x 256 grid, scaled to one size, and returns it as PNG bytes.
function New-IconPng([int]$size) {
    $tile = New-Brush "#0B0B0C"
    $outline = New-Object System.Windows.Media.Pen (New-Brush "#3A3A3C"), 6
    $white = New-Brush "#FFFFFF"
    $black = New-Brush "#000000"

    $visual = New-Object System.Windows.Media.DrawingVisual
    $context = $visual.RenderOpen()
    $context.PushTransform((New-Object System.Windows.Media.ScaleTransform ($size / 256), ($size / 256)))
    # Tile with its outline, the white pill, the artwork square and three equalizer bars.
    $context.DrawRoundedRectangle($tile, $outline, (New-Rect 11 11 234 234), 54, 54)
    $context.DrawRoundedRectangle($white, $null, (New-Rect 36 98 184 60), 30, 30)
    $context.DrawRoundedRectangle($black, $null, (New-Rect 54 112 32 32), 9, 9)
    $context.DrawRoundedRectangle($black, $null, (New-Rect 166 120 8 24), 3, 3)
    $context.DrawRoundedRectangle($black, $null, (New-Rect 180 112 8 32), 3, 3)
    $context.DrawRoundedRectangle($black, $null, (New-Rect 194 126 8 18), 3, 3)
    $context.Pop()
    $context.Close()

    $bitmap = New-Object System.Windows.Media.Imaging.RenderTargetBitmap $size, $size, 96, 96, ([System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream = New-Object System.IO.MemoryStream
    $encoder.Save($stream)
    return , $stream.ToArray()
}

# An .ico file is a small header, one 16-byte entry per size, then each image (PNG is allowed).
$images = @($sizes | ForEach-Object { , (New-IconPng $_) })
$file = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter $file
$writer.Write([UInt16]0)               # reserved
$writer.Write([UInt16]1)               # 1 = icon
$writer.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $side = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }   # 0 means 256
    $writer.Write([byte]$side)         # width
    $writer.Write([byte]$side)         # height
    $writer.Write([byte]0)             # no colour palette
    $writer.Write([byte]0)             # reserved
    $writer.Write([UInt16]1)           # colour planes
    $writer.Write([UInt16]32)          # bits per pixel
    $writer.Write([UInt32]$images[$i].Length)
    $writer.Write([UInt32]$offset)
    $offset += $images[$i].Length
}
foreach ($image in $images) {
    $writer.Write($image)
}
$writer.Flush()

$fullPath = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Force (Split-Path $fullPath) | Out-Null
[System.IO.File]::WriteAllBytes($fullPath, $file.ToArray())
Write-Host "Saved $fullPath ($($file.Length) bytes, sizes $($sizes -join ', '))"
