param(
    [string]$Project = "samples/dotnet/PerfApp",
    [string]$Configuration = "Release",
    [int]$Runs = 3,
    [string]$OutputPath = "scripts/perf/device-matrix.json",
    [string]$GCHeapHardLimit = "",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
. (Join-Path $scriptDir "..\lib\sdk.ps1")

Write-Host "=== Device Matrix: enumerate connected devices ==="

# Locate hdc through the shared SDK resolver.
$hdc = Find-HarmonyHdc

Write-Host "hdc: $hdc"

$deviceTargets = & $hdc list targets | Where-Object { $_ -match '\S' }
if (-not $deviceTargets) {
    throw "No connected HarmonyOS device or emulator was found."
}

Write-Host "Found $($deviceTargets.Count) device(s): $($deviceTargets -join ', ')"

$deviceResults = @()
foreach ($target in $deviceTargets) {
    $deviceType = "unknown"
    $deviceInfo = & $hdc -t $target shell param get const.product.model 2>$null
    if ($deviceInfo) {
        $deviceType = ($deviceInfo | Where-Object { $_ -match '\S' } | Select-Object -First 1).Trim()
    }

    Write-Host "`n=== Collecting baseline for: $target ($deviceType) ==="

    $baselinePath = Join-Path $projectRoot "scripts/perf/baseline-$target.json"
    $collectArgs = @(
        "-File", (Join-Path $scriptDir "collect-baseline.ps1"),
        "-Project", $Project,
        "-Configuration", $Configuration,
        "-Runs", $Runs,
        "-Target", $target,
        "-OutputPath", $baselinePath
    )
    if ($GCHeapHardLimit) {
        $collectArgs += @("-GCHeapHardLimit", $GCHeapHardLimit)
    }
    if ($SkipBuild) {
        $collectArgs += "-SkipBuild"
    }

    & "pwsh" @collectArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Baseline collection failed for $target. Recording failure."
        $deviceResults += [ordered]@{
            target = $target
            deviceType = $deviceType
            status = "failed"
        }
        continue
    }

    $baseline = Get-Content -LiteralPath $baselinePath -Raw | ConvertFrom-Json
    $deviceResults += [ordered]@{
        target = $target
        deviceType = $deviceType
        status = "ok"
        coldStartMsAverage = $baseline.summary.coldStartMsAverage
        runtimeInitMsAverage = $baseline.summary.runtimeInitMsAverage
        firstFrameMsAverage = $baseline.summary.firstFrameMsAverage
        tsfnThroughputOpsPerSecAverage = $baseline.summary.tsfnThroughputOpsPerSecAverage
        gcAllocBytesAverage = $baseline.summary.gcAllocBytesAverage
        gcCountAverage = $baseline.summary.gcCountAverage
        gcPauseMsAverage = $baseline.summary.gcPauseMsAverage
    }
}

$result = [ordered]@{
    timestamp = (Get-Date).ToUniversalTime().ToString("o")
    gitCommit = (git rev-parse HEAD).Trim()
    project = $Project
    configuration = $Configuration
    gcHeapHardLimit = $GCHeapHardLimit
    deviceCount = $deviceTargets.Count
    devices = $deviceResults
}

$resolvedOutputPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath
} else {
    Join-Path $projectRoot $OutputPath
}

$outputDir = Split-Path -Parent $resolvedOutputPath
if (-not (Test-Path -LiteralPath $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $resolvedOutputPath
Write-Host "`nDevice matrix written: $resolvedOutputPath"

$failed = $deviceResults | Where-Object { $_.status -eq "failed" }
if ($failed) {
    Write-Warning "$($failed.Count) device(s) failed baseline collection."
}
