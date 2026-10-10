$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsDir = Split-Path -Parent $scriptDir
$deploy = Join-Path $scriptsDir "deploy-hap.ps1"
$fakeHdc = Join-Path $scriptDir "fake-hdc.ps1"
$pwsh = (Get-Process -Id $PID).Path
$work = Join-Path ([IO.Path]::GetTempPath()) ("dot-harmony-deploy-" + [Guid]::NewGuid().ToString("N"))
$hostDir = Join-Path $work "host"
$appJson = Join-Path $hostDir "AppScope\app.json5"
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $appJson) | Out-Null
Set-Content $appJson '{ "bundleName": "com.example.deploytest" }'

function Invoke-Deploy([string]$Mode, [string[]]$Arguments, [int]$Timeout = 1) {
    $env:FAKE_HDC_MODE = $Mode
    $env:HARMONY_HDC = $fakeHdc
    $env:HOST_DIR = $hostDir
    $env:HARMONY_STARTUP_TIMEOUT_SECONDS = "$Timeout"
    $output = & $pwsh -NoProfile -File $deploy @Arguments 2>&1 | Out-String
    $script:LastExit = $LASTEXITCODE
    $script:LastOutput = $output
}

try {
    Invoke-Deploy "no-device" @("-PreflightOnly")
    if ($script:LastExit -eq 0 -or $script:LastOutput -notmatch "没有已连接的设备") {
        throw "no-device preflight failed: exit=$($script:LastExit) output=$($script:LastOutput)"
    }
    Write-Output "PASS no device"

    Invoke-Deploy "missing-target" @("-PreflightOnly", "-Target", "10.0.0.9:5555")
    if ($script:LastExit -eq 0 -or $script:LastOutput -notmatch "指定目标 10.0.0.9:5555") {
        throw "missing-target preflight failed: exit=$($script:LastExit) output=$($script:LastOutput)"
    }
    Write-Output "PASS missing target"

    $hap = Join-Path $hostDir "entry\build\default\outputs\default\entry-default-unsigned.hap"
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $hap) | Out-Null
    Set-Content $hap "fake"

    Invoke-Deploy "success" @()
    if ($script:LastExit -ne 0 -or $script:LastOutput -notmatch "HarmonyHost startup verified") {
        throw "startup success failed: exit=$($script:LastExit) output=$($script:LastOutput)"
    }
    Write-Output "PASS startup success"

    Invoke-Deploy "timeout" @() 0
    if ($script:LastExit -eq 0 -or $script:LastOutput -notmatch "启动超时" -or $script:LastOutput -notmatch "最近 hilog") {
        throw "startup timeout failed: exit=$($script:LastExit) output=$($script:LastOutput)"
    }
    Write-Output "PASS startup timeout"
} finally {
    Remove-Item -LiteralPath $work -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item Env:FAKE_HDC_MODE, Env:HARMONY_HDC, Env:HOST_DIR, Env:HARMONY_STARTUP_TIMEOUT_SECONDS -ErrorAction SilentlyContinue
}
