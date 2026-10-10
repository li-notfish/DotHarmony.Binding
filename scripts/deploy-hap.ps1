# 一键部署：重装 HAP → 启动 → 抓取 HarmonyHost 日志。
# SDK/hdc 定位由 scripts/lib/sdk.ps1 统一处理。
param(
    [string]$Target = "",
    [switch]$PreflightOnly
)
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
. (Join-Path $scriptDir "lib\sdk.ps1")

# 宿主目录：HOST_DIR 覆盖（targets 生成的按应用暂存宿主），默认共享模板
$hostDir = if ($env:HOST_DIR) { $env:HOST_DIR } else { Join-Path $projectRoot "samples\HarmonyHost" }
if (-not $env:HOST_DIR) {
    $staged = Get-ChildItem (Join-Path $projectRoot "samples\dotnet") -Directory -ErrorAction SilentlyContinue |
        ForEach-Object { Join-Path $_.FullName "obj\harmony\host" } |
        Where-Object { Test-Path (Join-Path $_ "entry\build\default\outputs\default") }
    if ($staged) {
        Write-Warning "未设置 HOST_DIR，将使用共享模板宿主（可能是旧 HAP）。检测到按应用暂存宿主："
        $staged | ForEach-Object { Write-Warning "  $_  （使用时：`$env:HOST_DIR='...'）" }
    }
}
# bundle 名：HOST_DIR 为暂存宿主时读其 app.json5；默认共享模板
$BUNDLE = "com.arktsbinding.harmonyhost"
if ($hostDir -and (Test-Path (Join-Path $hostDir "AppScope/app.json5"))) {
    $m = Select-String -Path (Join-Path $hostDir "AppScope/app.json5") -Pattern '"bundleName"\s*:\s*"([^"]*)"'
    if ($m) { $BUNDLE = $m.Matches[0].Groups[1].Value }
}
$ABILITY = "EntryAbility"
$MODULE  = "entry"

# ---- 定位 hdc ----
$HDC = Find-HarmonyHdc
Write-Host "hdc: $HDC"

# ---- 目标设备：构建 AOT/Hvigor 前完成预检 ----
$targets = @(& $HDC list targets |
    ForEach-Object { "$_".Trim() } |
    Where-Object { $_ -and $_ -ne "[Empty]" })
if (-not $targets) {
    Write-Error "没有已连接的设备/模拟器（hdc list targets 为空）"
    exit 1
}
$HDC_TARGET = if ($Target) { $Target } else { $env:HDC_TARGET }
if ($HDC_TARGET -and $HDC_TARGET -notin $targets) {
    Write-Error "指定目标 $HDC_TARGET 不在 hdc list targets 中（先 hdc tconn）"
    exit 1
}
Write-Host "targets: $($targets -join ', ')"

if ($HDC_TARGET) {
    function Invoke-Hdc { & $HDC -t $HDC_TARGET @args }
} else {
    function Invoke-Hdc { & $HDC @args }
}

if ($PreflightOnly) {
    Write-Host "=== Harmony device preflight OK"
    exit 0
}

# ---- 定位 HAP：优先安装签名产物 ----
$outputDir = Join-Path $hostDir "entry\build\default\outputs\default"
$HAP = Join-Path $outputDir "$MODULE-default-signed.hap"
if (-not (Test-Path $HAP)) {
    $HAP = Join-Path $outputDir "$MODULE-default-unsigned.hap"
}
if (-not (Test-Path $HAP)) {
    Write-Error "找不到 HAP：$HAP（请先运行 scripts\build-hap.cmd）"
    exit 1
}
Write-Host "HAP: $HAP"

Write-Host "=== 2. 清空 hilog ==="
Invoke-Hdc shell hilog -r 2>$null | Out-Null

Write-Host "=== 3. 安装 HAP ==="
# 先停掉旧实例：install -r 与运行中实例存在时序竞争（新实例可能启动即被销毁）
Invoke-Hdc shell "aa force-stop $BUNDLE" 2>$null | Out-Null
$installOut = Invoke-Hdc install -r $HAP
if (-not ("$installOut" -match "install bundle successfully")) {
    Write-Error "安装失败：$installOut"
    exit 1
}

Write-Host "=== 4. 启动应用 ==="
$startOutput = Invoke-Hdc shell "aa start -a $ABILITY -b $BUNDLE -m $MODULE -W" 2>&1 | Out-String

Write-Host "=== 5. 等待启动就绪 ==="
$timeoutSeconds = 20.0
if ($env:HARMONY_STARTUP_TIMEOUT_SECONDS) {
    if (-not [double]::TryParse($env:HARMONY_STARTUP_TIMEOUT_SECONDS, [ref]$timeoutSeconds) -or $timeoutSeconds -lt 0) {
        throw "HARMONY_STARTUP_TIMEOUT_SECONDS 必须是非负数：$($env:HARMONY_STARTUP_TIMEOUT_SECONDS)"
    }
}
$deadline = [DateTime]::UtcNow.AddSeconds($timeoutSeconds)
$ready = $false
$recentLog = ""
do {
    $processOutput = (Invoke-Hdc shell "pidof $BUNDLE" 2>&1 | Out-String).Trim()
    $recentLog = Invoke-Hdc shell "hilog -x" 2>&1 | Out-String
    if ($processOutput -match '\b\d+\b' -or $recentLog -match 'A00000/HarmonyHost') {
        $ready = $true
        break
    }
    if ([DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 500 }
} while ([DateTime]::UtcNow -lt $deadline)

if (-not $ready) {
    Write-Error ("启动超时（$timeoutSeconds 秒）：未发现 $BUNDLE 进程或 HarmonyHost 日志。" +
        "aa start 输出：$($startOutput.Trim())`n最近 hilog：")
    $recentLog -split "`n" | Select-Object -Last 40 | ForEach-Object { Write-Host $_ }
    exit 1
}

$recentLog -split "`n" |
    Select-String -Pattern 'A00000/HarmonyHost|dlopen|libapp|dotnet|DOTNET' |
    Select-Object -Last 40 |
    ForEach-Object { Write-Host $_ }
Write-Host "=== HarmonyHost startup verified"
