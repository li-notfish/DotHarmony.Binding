# 一键部署：重装 HAP → 启动 → 抓取 HarmonyHost 日志。
# SDK/hdc 定位顺序：OHOS_SDK_BASE > OHSDK_HOME > D:\Harmony\OpenHarmony\Sdk > DevEco sdk。
param([string]$Target = "")
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

# 宿主目录：HOST_DIR 覆盖（targets 生成的按应用暂存宿主），默认共享模板
$hostDir = if ($env:HOST_DIR) { $env:HOST_DIR } else { Join-Path $projectRoot "samples\HarmonyHost" }
# bundle 名：HOST_DIR 为暂存宿主时读其 app.json5；默认共享模板
$BUNDLE = "com.arktsbinding.harmonyhost"
if ($hostDir -and (Test-Path (Join-Path $hostDir "AppScope/app.json5"))) {
    $m = Select-String -Path (Join-Path $hostDir "AppScope/app.json5") -Pattern '"bundleName"\s*:\s*"([^"]*)"'
    if ($m) { $BUNDLE = $m.Matches[0].Groups[1].Value }
}
$ABILITY = "EntryAbility"
$MODULE  = "entry"

# ---- 定位 hdc ----
$candidates = @($env:OHOS_SDK_BASE, $env:OHSDK_HOME,
    "D:\Harmony\OpenHarmony\Sdk", "C:\Program Files\Huawei\DevEco Studio\sdk") | Where-Object { $_ }
$HDC = $null
foreach ($base in $candidates) {
    foreach ($rel in @("26.0.0\toolchains\hdc.exe", "toolchains\hdc.exe")) {
        $p = Join-Path $base $rel
        if (Test-Path $p) { $HDC = $p; break }
    }
    if ($HDC) { break }
}
if (-not $HDC) {
    Write-Error "找不到 hdc.exe。请设置 OHOS_SDK_BASE 指向 OpenHarmony SDK 根目录（含 26.0.0\toolchains）"
    exit 1
}
Write-Host "hdc: $HDC"

# ---- 目标设备：-Target <t> 或 $env:HDC_TARGET（多设备在线时指定）----
$HDC_TARGET = if ($Target) { $Target } else { $env:HDC_TARGET }
if ($HDC_TARGET) {
    $known = & $HDC list targets | Where-Object { $_.Trim() -eq $HDC_TARGET }
    if (-not $known) {
        Write-Error "指定目标 $HDC_TARGET 不在 hdc list targets 中（先 hdc tconn）"
        exit 1
    }
    function Invoke-Hdc { & $HDC -t $HDC_TARGET @args }
} else {
    function Invoke-Hdc { & $HDC @args }
}

# ---- 定位 HAP ----
$HAP = Join-Path $hostDir "entry\build\default\outputs\default\$MODULE-default-unsigned.hap"
if (-not (Test-Path $HAP)) {
    Write-Error "找不到 HAP：$HAP（请先运行 scripts\build-hap.cmd）"
    exit 1
}

Write-Host "=== 1. 检查设备 ==="
$targets = & $HDC list targets | Where-Object { $_ -match '\S' }
if (-not $targets) {
    Write-Error "没有已连接的设备/模拟器（hdc list targets 为空）"
    exit 1
}
Write-Host "targets: $($targets -join ', ')"

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
Invoke-Hdc shell aa start -a $ABILITY -b $BUNDLE -m $MODULE

Write-Host "=== 5. 等待后抓取日志 ==="
Start-Sleep -Seconds 6
Invoke-Hdc shell "hilog -x" |
    Select-String -Pattern 'A00000/HarmonyHost|dlopen|libapp|dotnet|DOTNET' |
    Select-Object -Last 40
