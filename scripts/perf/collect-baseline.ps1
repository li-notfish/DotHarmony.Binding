param(
    [string]$Project = "samples/dotnet/PerfApp",
    [string]$Configuration = "Release",
    [string]$Target = "",
    [int]$Runs = 3,
    [string]$OutputPath = "scripts/perf/baseline.json",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$resolvedProject = Join-Path $projectRoot $Project

if (-not (Test-Path -LiteralPath (Join-Path $resolvedProject "PerfApp.csproj"))) {
    throw "PerfApp project not found: $resolvedProject"
}

if (-not $SkipBuild) {
    Write-Host "=== 1. Build NativeAOT libraries and stage host ==="
    dotnet build $resolvedProject -t:HarmonyBuildLibApp -c $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "NativeAOT build failed."
    }

    Write-Host "=== 2. Build HAP ==="
    & (Join-Path $projectRoot "scripts\build-hap.cmd") $resolvedProject
    if ($LASTEXITCODE -ne 0) {
        throw "HAP build failed."
    }
}

$hostDir = Join-Path $resolvedProject "obj\harmony\host"
$hapDir = Join-Path $hostDir "entry\build\default\outputs\default"
$hapPath = Join-Path $hapDir "entry-default-signed.hap"
if (-not (Test-Path -LiteralPath $hapPath)) {
    $hapPath = Join-Path $hapDir "entry-default-unsigned.hap"
}
if (-not (Test-Path -LiteralPath $hapPath)) {
    throw "HAP not found under $hapDir."
}

$arm64SoPath = Join-Path $hostDir "entry\libs\arm64-v8a\libapp.so"
$x64SoPath = Join-Path $hostDir "entry\libs\x86_64\libapp.so"
foreach ($path in @($arm64SoPath, $x64SoPath)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "NativeAOT library not found: $path"
    }
}

$arm64SoBytes = (Get-Item -LiteralPath $arm64SoPath).Length
$x64SoBytes = (Get-Item -LiteralPath $x64SoPath).Length
$hapBytes = (Get-Item -LiteralPath $hapPath).Length

Write-Host "=== 3. Locate hdc ==="
$hdcCandidates = @(
    $env:OHOS_SDK_BASE,
    $env:OHSDK_HOME,
    "C:\Program Files\Huawei\DevEco Studio\sdk",
    "D:\Program Files\Huawei\DevEco Studio\sdk\default\openharmony",
    "C:\Program Files\Huawei\DevEco Studio\sdk\default\openharmony",
    "D:\Harmony\OpenHarmony\Sdk"
) | Where-Object { $_ }

$hdc = $null
foreach ($base in $hdcCandidates) {
    foreach ($relative in @("26.0.0\toolchains\hdc.exe", "toolchains\hdc.exe")) {
        $candidate = Join-Path $base $relative
        if (Test-Path -LiteralPath $candidate) {
            $hdc = $candidate
            break
        }
    }
    if ($hdc) {
        break
    }
}

if (-not $hdc) {
    throw "hdc.exe not found. Set OHOS_SDK_BASE to the OpenHarmony SDK root."
}

Write-Host "hdc: $hdc"

function Invoke-Hdc {
    param(
        [string[]]$Arguments,
        [switch]$SuppressErrors
    )

    if ($Target) {
        & $hdc -t $Target @Arguments
    } else {
        & $hdc @Arguments
    }

    if (-not $SuppressErrors -and $LASTEXITCODE -ne 0) {
        throw "hdc failed: $($Arguments -join ' ')"
    }
}

$targets = & $hdc list targets | Where-Object { $_ -match '\S' }
if (-not $targets) {
    throw "No connected HarmonyOS device or emulator was found."
}

$appJson5Path = Join-Path $hostDir "AppScope\app.json5"
$bundleName = "com.arktsbinding.harmonyhost"
if (Test-Path -LiteralPath $appJson5Path) {
    $match = Select-String -Path $appJson5Path -Pattern '"bundleName"\s*:\s*"([^"]*)"'
    if ($match) {
        $bundleName = $match.Matches[0].Groups[1].Value
    }
}

Write-Host "bundle: $bundleName"
Write-Host "HAP: $hapPath"

$baselineRuns = @()
for ($runIndex = 1; $runIndex -le $Runs; $runIndex++) {
    Write-Host "=== 4.$runIndex Run baseline ($runIndex/$Runs) ==="

    Invoke-Hdc @("shell", "aa force-stop $bundleName") -SuppressErrors | Out-Null
    Invoke-Hdc @("shell", "hilog -r") -SuppressErrors | Out-Null

    $installOutput = Invoke-Hdc @("install", "-r", $hapPath)
    if (-not ("$installOutput" -match "install bundle successfully")) {
        throw "HAP install failed: $installOutput"
    }

    $startOutput = Invoke-Hdc @("shell", "aa start -a EntryAbility -b $bundleName -m entry -W")

    $log = ""
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        $log = Invoke-Hdc @("shell", "hilog -x") |
            Select-String -Pattern "PERF_" |
            ForEach-Object { $_.Line }
        $log = $log -join "`n"

        if ($log -match "PERF_DONE|PERF_FAILED") {
            break
        }

        Start-Sleep -Milliseconds 500
    }

    if ($log -notmatch "PERF_DONE") {
        throw "Performance markers were not found in hilog. Last log:`n$log"
    }
    if ($log -match "PERF_FAILED") {
        throw "PerfApp reported a benchmark failure. Last log:`n$log"
    }

    $coldStartMatch = [regex]::Match("$startOutput", "TotalTime:\s*(\d+)")
    $runtimeInitMatch = [regex]::Match($log, "PERF_RUNTIME_INIT_MS=(\d+)")
    $firstFrameMatch = [regex]::Match($log, "PERF_FIRST_FRAME_MS=(\d+)")
    $throughputMatch = [regex]::Match($log, "PERF_TSFN_THROUGHPUT_OPS_PER_SEC=([0-9]+\.[0-9]+)")
    if (-not $coldStartMatch.Success -or -not $runtimeInitMatch.Success -or -not $firstFrameMatch.Success -or -not $throughputMatch.Success) {
        throw "Incomplete performance markers. Last log:`n$log"
    }

    $coldStartMs = [int]$coldStartMatch.Groups[1].Value
    $runtimeInitMs = [int]$runtimeInitMatch.Groups[1].Value
    $throughput = [double]$throughputMatch.Groups[1].Value
    $baselineRuns += [ordered]@{
        run = $runIndex
        coldStartMs = $coldStartMs
        runtimeInitMs = $runtimeInitMs
        firstFrameMs = [int]$firstFrameMatch.Groups[1].Value
        tsfnThroughputOpsPerSec = $throughput
    }

    Write-Host "cold start: $coldStartMs ms"
    Write-Host "runtime init: $runtimeInitMs ms"
    Write-Host "TSFN throughput: $throughput ops/s"
}

$coldStartAverage = [Math]::Round(($baselineRuns | ForEach-Object { $_.coldStartMs } | Measure-Object -Average).Average, 2)
$runtimeInitAverage = [Math]::Round(($baselineRuns | ForEach-Object { $_.runtimeInitMs } | Measure-Object -Average).Average, 2)
$firstFrameAverage = [Math]::Round(($baselineRuns | ForEach-Object { $_.firstFrameMs } | Measure-Object -Average).Average, 2)
$throughputAverage = [Math]::Round(($baselineRuns | ForEach-Object { $_.tsfnThroughputOpsPerSec } | Measure-Object -Average).Average, 2)

$result = [ordered]@{
    timestamp = (Get-Date).ToUniversalTime().ToString("o")
    gitCommit = (git rev-parse HEAD).Trim()
    project = $Project
    configuration = $Configuration
    device = if ($Target) { $Target } else { ($targets | Select-Object -First 1) }
    sizes = [ordered]@{
        arm64LibAppBytes = $arm64SoBytes
        x64LibAppBytes = $x64SoBytes
        hapBytes = $hapBytes
    }
    runs = $baselineRuns
    summary = [ordered]@{
        coldStartMsAverage = $coldStartAverage
        runtimeInitMsAverage = $runtimeInitAverage
        firstFrameMsAverage = $firstFrameAverage
        tsfnThroughputOpsPerSecAverage = $throughputAverage
    }
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
Write-Host "Baseline written: $resolvedOutputPath"
