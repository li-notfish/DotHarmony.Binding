# 布局截图基线回归：对样本应用的若干界面状态截图，与基线做像素 diff。
# 用法：
#   scripts\verify-layout-baseline.ps1 -Update          # 生成/更新基线（换设备或有意改布局后）
#   scripts\verify-layout-baseline.ps1                  # 比对，差异超阈值退出码 1
#   scripts\verify-layout-baseline.ps1 -App com.arktsbinding.helloapp -Target 127.0.0.1:5555
# 基线分辨率相关，存 artifacts/layout-baseline/（不入库）；步骤表见下方 $Steps。
param(
    [switch]$Update,
    [string]$App = "com.arktsbinding.helloapp",
    [string]$Target = "",
    [double]$Tolerance = 0.02  # 允许的差异像素占比（抗锯齿/字体光栅抖动）
)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot "lib\sdk.ps1")
$baselineDir = Join-Path $root "artifacts\layout-baseline"
$workDir = Join-Path $root "tmp\layout-compare"
$helper = Join-Path $PSScriptRoot "find-layout-text.py"
New-Item -ItemType Directory -Force $baselineDir, $workDir | Out-Null

if (-not (Test-Path -LiteralPath $helper)) {
    throw "缺少布局文本辅助脚本：$helper"
}
# 提前失败：Pillow 缺失时不能把 diff 当成 0% 放行
& python -X utf8 -c "from PIL import Image" 2>$null | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Python 无法导入 Pillow（PIL）。请先安装：python -m pip install pillow"
}

# hdc 定位（与 deploy-hap.ps1 共用）
$HDC = Find-HarmonyHdc
$hdcArgs = @()
if ($Target) { $hdcArgs += @("-t", $Target) }

# 步骤表：名称 + 截图前点击的文本（可空）+ 是否截图。布局夹具：首页 / 详情页（路由+query）/ 模态。
$Steps = @(
    @{ Name = "home";   Click = $null;                      Capture = $true },
    @{ Name = "detail"; Click = "probeDetail?value=hello42"; Capture = $true },
    @{ Name = "back";   Click = "GoToAsync ..";             Capture = $false },
    @{ Name = "grid";   Click = "GoToAsync //probeGrid";    Capture = $true },
    @{ Name = "back2";  Click = "GoToAsync ..";             Capture = $false },
    @{ Name = "modal";  Click = "Open modal";               Capture = $true }
)

function Invoke-HdcChecked([string[]]$Arguments, [string]$What) {
    & $HDC @hdcArgs @Arguments | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "$What 失败（hdc 退出码 $LASTEXITCODE）" }
}

function Click-Text([string]$want) {
    Invoke-HdcChecked @("shell", "rm", "-f", "/data/local/tmp/layout.json") "清理布局 dump"
    Invoke-HdcChecked @("shell", "uitest", "dumpLayout", "-p", "/data/local/tmp/layout.json") "dumpLayout"
    Invoke-HdcChecked @("file", "recv", "/data/local/tmp/layout.json", "$workDir\layout.json") "拉取布局 dump"
    if (-not (Test-Path -LiteralPath "$workDir\layout.json")) {
        throw "布局 dump 未落盘：$workDir\layout.json"
    }
    $c = & python -X utf8 $helper "$workDir\layout.json" $want
    if ($LASTEXITCODE -ne 0) { throw "文本未找到：$want" }
    $xy = "$c".Trim() -split '\s+'
    if ($xy.Count -lt 2 -or $xy[0] -notmatch '^\d+$' -or $xy[1] -notmatch '^\d+$') {
        throw "无法解析点击坐标（want='$want'，helper 输出='$c'）"
    }
    Invoke-HdcChecked @("shell", "uitest", "uiInput", "click", $xy[0], $xy[1]) "点击 $want"
    Start-Sleep 2
}

Invoke-HdcChecked @("shell", "aa force-stop $App") "停止旧应用"
Invoke-HdcChecked @("shell", "aa start -a EntryAbility -b $App") "启动应用"
Start-Sleep 8

$failed = @()
foreach ($step in $Steps) {
    if ($step.Click) { Click-Text $step.Click }
    if (-not $step.Capture) { continue }
    $shotRemote = "/data/local/tmp/shot.jpeg"
    $shotLocal = Join-Path $workDir "$($step.Name).jpeg"
    Invoke-HdcChecked @("shell", "rm", "-f", $shotRemote) "清理远端截图"
    Invoke-HdcChecked @("shell", "snapshot_display -i 0 -f $shotRemote") "截图"
    Invoke-HdcChecked @("file", "recv", $shotRemote, $shotLocal) "拉取截图"
    if (-not (Test-Path -LiteralPath $shotLocal) -or (Get-Item $shotLocal).Length -eq 0) {
        throw "截图未落盘或为空：$shotLocal"
    }

    $baseline = Join-Path $baselineDir "$($step.Name).jpeg"
    if ($Update) {
        Copy-Item $shotLocal $baseline -Force
        Write-Host "[baseline] $($step.Name) updated"
        continue
    }
    if (-not (Test-Path $baseline)) {
        $failed += $step.Name
        Write-Host "[FAIL] $($step.Name): 缺少基线（先运行 -Update）"
        continue
    }

    $diff = & python -X utf8 -c @"
import sys
from PIL import Image, ImageChops
a = Image.open(r'$baseline').convert('RGB')
b = Image.open(r'$shotLocal').convert('RGB')
if a.size != b.size:
    print('1.0'); sys.exit(0)
d = ImageChops.difference(a, b)
px = sum(1 for p in d.getdata() if max(p) > 24)
print(px / (a.size[0] * a.size[1]))
"@
    if ($LASTEXITCODE -ne 0) { throw "像素 diff 失败：$($step.Name)（python 退出码 $LASTEXITCODE）" }
    $text = "$diff".Trim()
    if ($text -notmatch '^(0(\.\d+)?|1(\.0+)?)$') {
        throw "像素 diff 输出无法解析：$($step.Name) -> '$text'"
    }
    $ratio = [double]$text
    if ($ratio -gt $Tolerance) {
        $failed += $step.Name
        Write-Host "[FAIL] $($step.Name): diff $([math]::Round($ratio*100,2))% > $([math]::Round($Tolerance*100,2))%"
    } else {
        Write-Host "[ok] $($step.Name): diff $([math]::Round($ratio*100,2))%"
    }
}

if (-not $Update -and $failed.Count -gt 0) {
    Write-Host "=== FAILED: $($failed.Count) step(s): $($failed -join ', ')"
    exit 1
}
if (-not $Update) { Write-Host "=== layout baseline OK" }
