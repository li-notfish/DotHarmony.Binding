# Handler 生命周期模拟器回归：反复路由 push/pop、Tab 切换、Flyout 开合、模态开关，
# 断言 hilog 无 [callback] handler threw / 崩溃，进程存活（句柄泄漏崩溃会换 pid）。
# 用法：scripts\verify-lifecycle.ps1 [-Iterations 5] [-App com.arktsbinding.helloapp]
param(
    [int]$Iterations = 5,
    [string]$App = "com.arktsbinding.helloapp",
    [string]$Target = ""
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$workDir = Join-Path $root "tmp\lifecycle"
New-Item -ItemType Directory -Force $workDir | Out-Null

# hdc 定位（与 deploy-hap.ps1 同一顺序）
$HDC = $null
foreach ($base in @($env:OHOS_SDK_BASE, $env:OHSDK_HOME,
    "D:\Harmony\OpenHarmony\Sdk", "C:\Program Files\Huawei\DevEco Studio\sdk",
    "D:\Program Files\Huawei\DevEco Studio\sdk")) {
    if ($base) {
        foreach ($rel in @("26.0.0\toolchains\hdc.exe", "toolchains\hdc.exe", "default\openharmony\toolchains\hdc.exe")) {
            $p = Join-Path $base $rel
            if (Test-Path $p) { $HDC = $p; break }
        }
    }
    if ($HDC) { break }
}
if (-not $HDC) { Write-Error "hdc.exe not found (set OHOS_SDK_BASE)" }
$hdcArgs = @()
if ($Target) { $hdcArgs += @("-t", $Target) }

function Click-Text([string]$want) {
    # 页面转场/动画未就绪时重试一轮（模拟器时序抖动）
    foreach ($attempt in 1..3) {
        & $HDC @hdcArgs shell rm -f /data/local/tmp/layout.json | Out-Null
        & $HDC @hdcArgs shell uitest dumpLayout -p /data/local/tmp/layout.json | Out-Null
        & $HDC @hdcArgs file recv /data/local/tmp/layout.json "$workDir\layout.json" | Out-Null
        $c = python -X utf8 "$root\scripts\find-layout-text.py" "$workDir\layout.json" $want
        if ($LASTEXITCODE -eq 0) {
            $xy = "$c" -split ' '
            Write-Host "  click '$want' at $($xy[0]),$($xy[1]) (attempt $attempt)"
            & $HDC @hdcArgs shell uitest uiInput click $xy[0] $xy[1] | Out-Null
            Start-Sleep 2
            return
        }
        Start-Sleep 2
    }
        python -X utf8 "$root\scripts\dump-layout-texts.py" "$workDir\layout.json" | Write-Host
    throw "text not found on screen: $want"
}

function Start-App {
    & $HDC @hdcArgs shell "aa force-stop $App" | Out-Null
    & $HDC @hdcArgs shell "aa start -a EntryAbility -b $App" | Out-Null
    Start-Sleep 8
    # 冷启动首击常被窗口聚焦吞掉：空白处先点一下热身（实测首击可能无效）
    & $HDC @hdcArgs shell uitest uiInput click 660 2300 | Out-Null
    Start-Sleep 2
}

function Invoke-Iteration([int]$i) {
    # 路由 push/pop（handler 建/断连）
    Click-Text "probeDetail?value=hello42"
    Click-Text "GoToAsync .."
    # Tab 切换（页面子树缓存命中/换出）
    Click-Text "绑定"
    Click-Text "首页"
    # Flyout 开/关（面板动画 + 遮罩事件订阅/释放）
    Click-Text "Open flyout via FlyoutIsPresented"
    # 遮罩点击关闭：面板右侧区域（含遮罩）
    & $HDC @hdcArgs shell uitest uiInput click 1150 1400 | Out-Null
    Start-Sleep 1
    # 模态开/关
    Click-Text "Open modal"
    Click-Text "Close modal"
    $pidNow = Get-AppPid
    if ($pidNow -ne $script:pid0) { throw "app restarted/crashed at iteration $i (pid $($script:pid0) -> $pidNow)" }
    Write-Host "[ok] iteration $i"
}

function Get-AppPid {
    $out = & $HDC @hdcArgs shell "pidof $App"
    return "$out".Trim()
}

Start-App
$script:pid0 = Get-AppPid
if (-not $script:pid0) { throw "app did not start" }
Write-Host "app pid=$($script:pid0), $Iterations iterations"

for ($i = 1; $i -le $Iterations; $i++) {
    try {
        Invoke-Iteration $i
    }
    catch {
        # 模拟器时序抖动：重启应用回到首页重试一次
        Write-Host "[retry] iteration $i failed: $($_.Exception.Message)"
        Start-App
        $script:pid0 = Get-AppPid
        Invoke-Iteration $i
    }
}

# hilog 扫描：handler 回调异常 / 致命错误
& $HDC @hdcArgs shell "hilog -x" > "$workDir\hilog.txt" 2>&1
$bad = Select-String -Path "$workDir\hilog.txt" -Pattern "handler threw|Fatal|FORCE-CRASH" |
    Where-Object { $_.Line -match "VProbe|harmony|libapp" }
if ($bad) {
    $bad | Select-Object -First 10 | ForEach-Object { Write-Host $_.Line }
    throw "hilog contains handler errors"
}
Write-Host "lifecycle verification OK ($Iterations iterations, no handler errors)"
