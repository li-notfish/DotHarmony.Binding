# 原生 z 平局仲裁探针验证：进入探针页 → 按 PROBE STEP 标记逐步截屏 → 本地采样重叠区像素。
# 用法：pwsh scripts/verify-zindex-probe.ps1 [-Target 127.0.0.1:5555]
# 判定输出：每步各重叠区的最上层颜色 → 推断仲裁规则（最后写入置顶 / 同值写入短路 / 插入顺序）。
param([string]$Target = "127.0.0.1:5555")
$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
. (Join-Path $scriptDir "lib\sdk.ps1")

# ---- 定位 hdc（共享候选与版本排序） ----
$HDC = Find-HarmonyHdc
function Invoke-Hdc { & $HDC -t $Target @args }

$outDir = Join-Path $projectRoot "tmp\zprobe"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

Write-Host "=== 1. 清空日志 ==="
Invoke-Hdc shell hilog -r 2>$null | Out-Null

function Find-TextBounds([string]$want) {
    Invoke-Hdc shell "uitest dumpLayout -p /data/local/tmp/_zp_dump.json" | Out-Null
    Invoke-Hdc file recv /data/local/tmp/_zp_dump.json (Join-Path $outDir "_dump.json") | Out-Null
    $json = Get-Content (Join-Path $outDir "_dump.json") -Raw | ConvertFrom-Json
    $queue = [System.Collections.Generic.Queue[object]]::new()
    $queue.Enqueue($json)
    while ($queue.Count -gt 0) {
        $n = $queue.Dequeue()
        if ("$($n.attributes.text)" -eq $want) {
            return [int[]]([regex]::Matches("$($n.attributes.bounds)", "-?\d+") | ForEach-Object { [int]$_.Value })
        }
        foreach ($c in @($n.children)) { if ($null -ne $c) { $queue.Enqueue($c) } }
    }
    return $null
}
function Find-TextCenter([string]$want) {
    $b = Find-TextBounds $want
    if (-not $b) { return $null }
    return @([int](($b[0] + $b[2]) / 2), [int](($b[1] + $b[3]) / 2))
}

Write-Host "=== 2. 定位并点击进入探针页 ==="
$click = Find-TextCenter "ZIndex arbitration probe"
if (-not $click) { Write-Error "找不到探针入口按钮（应用未运行或页面未渲染）"; exit 1 }
Invoke-Hdc shell "uitest uiInput click $($click[0]) $($click[1])" 2>$null | Out-Null
Start-Sleep -Seconds 2

Write-Host "=== 3. 定位 PROBE NEXT 按钮 ==="
$nextBounds = Find-TextBounds "PROBE NEXT"
if (-not $nextBounds) { Write-Error "找不到 PROBE NEXT 按钮（探针页未进入）"; exit 1 }
$next = @([int](($nextBounds[0] + $nextBounds[2]) / 2), [int](($nextBounds[1] + $nextBounds[3]) / 2))
# 密度+原点自校准：状态 Label 在 Grid 首行（固定 360vp，MAUI 默认 Fill），
# 其 px bounds → density = 高px/360、content 原点 = bounds 左上角；
# 再用 NEXT 按钮顶边（恰好起于 360vp 刻度线）做交叉校验，超差即拒收结果
$statusBounds = Find-TextBounds "probe ready"
if (-not $statusBounds) { Write-Error "找不到状态行（探针页未渲染）"; exit 1 }
$density = ($statusBounds[3] - $statusBounds[1]) / 360.0
$originX = $statusBounds[0]; $originY = $statusBounds[1]
$expectedNextTop = $originY + 360.0 * $density
if ([Math]::Abs($nextBounds[1] - $expectedNextTop) -gt 2.0) {
    Write-Error ("密度/原点自校准交叉校验失败：NEXT 顶边 {0}px，按 360vp×density 推算 {1:F1}px（采样结果不可信，请自查页面状态行高度）" -f $nextBounds[1], $expectedNextTop)
    exit 1
}
Write-Host "next button at $($next -join ','), density~$density, origin=($originX,$originY)px"

$steps = @("s0_decl_000", "s1_rev_000", "s2_g_raise10", "s3_restore_rewall", "s4_restore_onlyothers", "s5_distinct_012", "s6_distinct_210")

Write-Host "=== 4. 逐步点击 NEXT → 等标记 → 截屏 ==="
foreach ($s in $steps) {
    Invoke-Hdc shell "uitest uiInput click $($next[0]) $($next[1])" 2>$null | Out-Null
    $ok = $false
    $d2 = (Get-Date).AddSeconds(10)
    while ((Get-Date) -lt $d2) {
        if ((Invoke-Hdc shell "hilog -x" | Select-String "PROBE STEP $s APPLIED")) { $ok = $true; break }
        Start-Sleep -Milliseconds 500
    }
    if (-not $ok) { Write-Error "未等到 $s（探针未走完）"; exit 1 }
    Start-Sleep -Milliseconds 600  # 等一帧渲染
    Invoke-Hdc shell "snapshot_display -f /data/local/tmp/zprobe_$s.jpeg" | Out-Null
    Invoke-Hdc file recv "/data/local/tmp/zprobe_$s.jpeg" (Join-Path $outDir "$s.jpeg") | Out-Null
    Write-Host "$s captured"
}

Write-Host "=== 5. 像素采样（重叠区颜色表） ==="
Add-Type -AssemblyName System.Drawing
# 区块几何（vp，须与 ProbeZIndexPage.cs 保持一致）：R(30,30) G(75,75) B(120,120)，边长 170，
# 容器 320x300 悬于页面内容原点 (0,0)
function Read-Pixel($step, $vx, $vy) {
    $bmp = [System.Drawing.Bitmap]::FromFile((Join-Path $outDir "$step.jpeg"))
    $px = $bmp.GetPixel([int]($originX + $vx * $density), [int]($originY + $vy * $density))
    $bmp.Dispose()
    $c = @{ R = $px.R; G = $px.G; B = $px.B }
    $classes = [ordered]@{
        red   = @(255, 0, 0); green = @(0, 200, 0); blue = @(0, 0, 255)
        dark  = @(20, 20, 20); black = @(0, 0, 0)
    }
    $best = "unknown"; $bestD = 1e9
    foreach ($k in $classes.Keys) {
        $t = $classes[$k]
        $d = [Math]::Sqrt(($c.R - $t[0]) * ($c.R - $t[0]) + ($c.G - $t[1]) * ($c.G - $t[1]) + ($c.B - $t[2]) * ($c.B - $t[2]))
        if ($d -lt $bestD) { $bestD = $d; $best = $k }
    }
    return "$best(rgb=$($c.R),$($c.G),$($c.B))"
}
# 重叠区代表点（vp）：R 独占 [30,75)² → (45,45)；R∩G\B [75,120)² → (97,97)；
# R∩G∩B [120,200)² → (160,160)；G∩B\R x∈[200,245) → (222,150)；B 独占 [200,290)×[245,290) → (270,270)
$points = [ordered]@{ Ron_R = @(45, 45); RG = @(97, 97); RGB = @(160, 160); GB = @(222, 150); Bonly = @(270, 270) }
foreach ($s in $steps) {
    $row = foreach ($p in $points.Keys) { "$p=$(Read-Pixel $s $points[$p][0] $points[$p][1])" }
    Write-Host ("{0,-24} {1}" -f $s, ($row -join "  "))
}
