# measure-idle.ps1: measures CPU and memory of a running process while it sits idle.
# Used for the OverlayProbe idle check (PRD goal G4: under 100 MB RAM, near-zero CPU).
# Samples once per second and prints average and peak values. Reads only the process's
# CPU time and memory counters, nothing inside the process.
#
# Example: .\measure-idle.ps1 -ProcessName OverlayProbe -Seconds 60

[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$ProcessName,
    [int]$Seconds = 60
)

$ErrorActionPreference = "Stop"

$process = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $process) {
    throw "No running process named '$ProcessName'. Start it first."
}

$logicalCores = [Environment]::ProcessorCount
$cpuSamples = New-Object System.Collections.Generic.List[double]
$workingSetSamples = New-Object System.Collections.Generic.List[double]
$privateSamples = New-Object System.Collections.Generic.List[double]

$process.Refresh()
$previousCpu = $process.TotalProcessorTime.TotalMilliseconds
$previousTime = Get-Date

Write-Host "Measuring '$ProcessName' (PID $($process.Id)) for $Seconds seconds. Do not touch it."
for ($second = 1; $second -le $Seconds; $second++) {
    Start-Sleep -Seconds 1
    $process.Refresh()
    if ($process.HasExited) {
        throw "Process exited during measurement."
    }

    $now = Get-Date
    $cpuNow = $process.TotalProcessorTime.TotalMilliseconds
    $elapsedMs = ($now - $previousTime).TotalMilliseconds
    # Percent of the whole machine (all cores), like Task Manager shows it.
    $cpuSamples.Add((($cpuNow - $previousCpu) / $elapsedMs) * 100 / $logicalCores)
    $workingSetSamples.Add($process.WorkingSet64 / 1MB)
    $privateSamples.Add($process.PrivateMemorySize64 / 1MB)
    $previousCpu = $cpuNow
    $previousTime = $now
}

function Format-Stats([System.Collections.Generic.List[double]]$samples, [string]$unit, [string]$format) {
    $stats = $samples | Measure-Object -Average -Maximum
    return "average {0} {2}, peak {1} {2}" -f $stats.Average.ToString($format), $stats.Maximum.ToString($format), $unit
}

Write-Host ""
Write-Host "Results over $Seconds s ($logicalCores logical cores):"
Write-Host "  CPU:            $(Format-Stats $cpuSamples '%' 'F2')"
Write-Host "  Working set:    $(Format-Stats $workingSetSamples 'MB' 'F1')"
Write-Host "  Private memory: $(Format-Stats $privateSamples 'MB' 'F1')"
