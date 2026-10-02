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
$baselineDir = Join-Path $root "artifacts\layout-baseline"
$workDir = Join-Path $root "tmp\layout-compare"
New-Item -ItemType Directory -Force $baselineDir, $workDir | Out-Null

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

# 步骤表：名称 + 截图前点击的文本（可空）+ 是否截图。布局夹具：首页 / 详情页（路由+query）/ 模态。
$Steps = @(
    @{ Name = "home";   Click = $null;                      Capture = $true },
    @{ Name = "detail"; Click = "probeDetail?value=hello42"; Capture = $true },
    @{ Name = "back";   Click = "GoToAsync ..";             Capture = $false },
    @{ Name = "modal";  Click = "Open modal";               Capture = $true }
)

function Click-Text([string]$want) {
    & $HDC @hdcArgs shell rm -f /data/local/tmp/layout.json | Out-Null
    & $HDC @hdcArgs shell uitest dumpLayout -p /data/local/tmp/layout.json | Out-Null
    & $HDC @hdcArgs file recv /data/local/tmp/layout.json "$workDir\layout.json" | Out-Null
    $c = python -X utf8 "$root\tmp\find_text.py" "$workDir\layout.json" $want
    if ($LASTEXITCODE -ne 0) { throw "text not found on screen: $want" }
    $xy = "$c" -split ' '
    & $HDC @hdcArgs shell uitest uiInput click $xy[0] $xy[1] | Out-Null
    Start-Sleep 2
}

& $HDC @hdcArgs shell "aa force-stop $App" | Out-Null
& $HDC @hdcArgs shell "aa start -a EntryAbility -b $App" | Out-Null
Start-Sleep 8

$failed = @()
foreach ($step in $Steps) {
    if ($step.Click) { Click-Text $step.Click }
    if (-not $step.Capture) { continue }
    & $HDC @hdcArgs shell "snapshot_display -i 0 -f /data/local/tmp/shot.jpeg" | Out-Null
    & $HDC @hdcArgs file recv /data/local/tmp/shot.jpeg "$workDir\$($step.Name).jpeg" | Out-Null
    $baseline = Join-Path $baselineDir "$($step.Name).jpeg"
    if ($Update) {
        Copy-Item "$workDir\$($step.Name).jpeg" $baseline -Force
        Write-Host "[baseline] $($step.Name) updated"
        continue
    }
    if (-not (Test-Path $baseline)) { Write-Host "[skip] $($step.Name): no baseline (run -Update)"; continue }
    $diff = python -X utf8 -c @"
import sys
from PIL import Image, ImageChops
a = Image.open(r'$baseline').convert('RGB')
b = Image.open(r'$workDir\$($step.Name).jpeg').convert('RGB')
if a.size != b.size:
    print('1.0'); sys.exit(0)
d = ImageChops.difference(a, b)
px = sum(1 for p in d.getdata() if max(p) > 24)
print(px / (a.size[0] * a.size[1]))
"@
    $ratio = [double]$diff
    if ($ratio -gt $Tolerance) {
        $failed += $step.Name
        Write-Host "[FAIL] $($step.Name): diff $([math]::Round($ratio*100,2))% > $([math]::Round($Tolerance*100,2))%"
    } else {
        Write-Host "[ok] $($step.Name): diff $([math]::Round($ratio*100,2))%"
    }
}
if (-not $Update -and $failed.Count -gt 0) { exit 1 }
