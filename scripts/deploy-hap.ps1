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

# 宿主目录：HOST_DIR 覆盖（targets 生成的按应用暂存宿主）。
# 未设置时只在“恰好一个已暂存宿主”时自动采用；绝不回退共享模板（AGENTS.md）。
$hostDir = $env:HOST_DIR
if (-not $hostDir) {
    $staged = @(Get-ChildItem (Join-Path $projectRoot "samples\dotnet") -Directory -ErrorAction SilentlyContinue |
        ForEach-Object { Join-Path $_.FullName "obj\harmony\host" } |
        Where-Object { Test-Path (Join-Path $_ "AppScope\app.json5") })
    if ($staged.Count -eq 1) {
        $hostDir = $staged[0]
        Write-Host "HOST_DIR not set; using the only staged host: $hostDir"
    } elseif ($staged.Count -gt 1) {
        Write-Error ("未设置 HOST_DIR，且检测到多个暂存宿主。请显式指定一个：`n  " +
            ($staged -join "`n  "))
        exit 1
    } else {
        Write-Error "未找到暂存宿主。请先运行 dotnet build <工程> -t:HarmonyRun 或 -t:HarmonyStageHost。"
        exit 1
    }
}
if (-not (Test-Path (Join-Path $hostDir "AppScope/app.json5"))) {
    Write-Error "宿主目录无效（缺少 AppScope/app.json5）：$hostDir"
    exit 1
}
# bundle 名：从暂存宿主的 app.json5 读取
$BUNDLE = "com.arktsbinding.harmonyhost"
if (Test-Path (Join-Path $hostDir "AppScope/app.json5")) {
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

# ---- 定位 HAP：优先安装签名产物，但签名产物必须不旧于 unsigned ----
$outputDir = Join-Path $hostDir "entry\build\default\outputs\default"
$signedHap = Join-Path $outputDir "$MODULE-default-signed.hap"
$unsignedHap = Join-Path $outputDir "$MODULE-default-unsigned.hap"
$HAP = $null
if (Test-Path $signedHap) {
    if ((Test-Path $unsignedHap) -and
        ((Get-Item $signedHap).LastWriteTimeUtc -lt (Get-Item $unsignedHap).LastWriteTimeUtc)) {
        Write-Warning "signed HAP 旧于 unsigned（签名未随本次构建重跑）；改为安装 unsigned。"
        $HAP = $unsignedHap
    } else {
        $HAP = $signedHap
    }
} elseif (Test-Path $unsignedHap) {
    $HAP = $unsignedHap
}
if (-not $HAP) {
    Write-Error "找不到 HAP：$outputDir（请先运行 scripts\build-hap.cmd，或用 -t:HarmonyRun）"
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
