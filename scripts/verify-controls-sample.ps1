param(
    [string]$App = "com.arktsbinding.controlssampleapp",
    [string]$Target = ""
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot "lib\sdk.ps1")

$hdc = Find-HarmonyHdc
$hdcArgs = @()
if ($Target) { $hdcArgs += @("-t", $Target) }

function Invoke-Hdc {
    & $hdc @hdcArgs @args
    if ($LASTEXITCODE -ne 0) {
        throw "hdc failed: $($args -join ' ')"
    }
}

function Invoke-HdcShell([string]$Command) {
    $output = Invoke-Hdc shell $Command 2>&1 | Out-String
    return $output.Trim()
}

function Get-LayoutPath([string]$Name) {
    return Join-Path $outputRoot "$Name-layout.json"
}

function Get-ScreenshotPath([string]$Name) {
    return Join-Path $outputRoot "$Name.jpeg"
}

function Invoke-DumpLayout([string]$Name) {
    Invoke-HdcShell "rm -f /data/local/tmp/controls-layout.json" | Out-Null
    Invoke-HdcShell "uitest dumpLayout -p /data/local/tmp/controls-layout.json" | Out-Null
    Invoke-Hdc file recv /data/local/tmp/controls-layout.json (Get-LayoutPath $Name) | Out-Null
}

function Invoke-Screenshot([string]$Name) {
    Invoke-HdcShell "rm -f /data/local/tmp/controls-shot.jpeg" | Out-Null
    Invoke-HdcShell "snapshot_display -i 0 -f /data/local/tmp/controls-shot.jpeg" | Out-Null
    Invoke-Hdc file recv /data/local/tmp/controls-shot.jpeg (Get-ScreenshotPath $Name) | Out-Null
}

function ConvertTo-FlatNodes($Node) {
    $nodes = [System.Collections.Generic.List[object]]::new()
    $visit = {
        param($Current)
        $nodes.Add($Current)
        if ($Current -is [System.Collections.IDictionary]) {
            $children = $Current["children"]
            if ($children) {
                foreach ($child in @($children)) {
                    & $visit $child
                }
            }
        }
    }
    & $visit $Node
    return $nodes
}

function Find-ApplicationRoot($Root) {
    foreach ($node in ConvertTo-FlatNodes $Root) {
        $bundleName = if ($node -is [System.Collections.IDictionary] -and $node.ContainsKey("attributes")) {
            "$($node["attributes"]["bundleName"])"
        } else {
            ""
        }
        if ($bundleName -eq $App) {
            return $node
        }
    }
    return $Root
}

function ConvertFrom-Bounds([string]$Bounds) {
    if ($Bounds -notmatch '^\[(-?\d+),(-?\d+)\]\[(-?\d+),(-?\d+)\]$') {
        return $null
    }
    return [pscustomobject]@{
        Left = [int]$Matches[1]
        Top = [int]$Matches[2]
        Right = [int]$Matches[3]
        Bottom = [int]$Matches[4]
    }
}

function Get-TextNodes($Root) {
    $appRoot = Find-ApplicationRoot $Root
    $nodes = ConvertTo-FlatNodes $appRoot
    $textNodes = foreach ($node in $nodes) {
        $attributes = if ($node -is [System.Collections.IDictionary] -and $node.ContainsKey("attributes")) { $node["attributes"] } else { @{} }
        $text = "$($attributes["text"])"
        if (-not $text) { continue }
        if ("$($attributes["visible"])" -ne "true") { continue }
        $bounds = ConvertFrom-Bounds "$($attributes["bounds"])"
        if ($null -eq $bounds) { continue }
        [pscustomobject]@{ Text = $text; Bounds = $bounds }
    }
    return @($textNodes)
}

function Get-AllTextNodes($Root) {
    $nodes = ConvertTo-FlatNodes $Root
    $textNodes = foreach ($node in $nodes) {
        $attributes = if ($node -is [System.Collections.IDictionary] -and $node.ContainsKey("attributes")) { $node["attributes"] } else { @{} }
        $text = "$($attributes["text"])"
        if (-not $text) { continue }
        $bounds = ConvertFrom-Bounds "$($attributes["origBounds"])"
        if ($null -eq $bounds) { continue }
        [pscustomobject]@{ Text = $text; Bounds = $bounds }
    }
    return @($textNodes)
}

function Test-TextNodesHaveArea([object[]]$TextNodes) {
    foreach ($node in $TextNodes) {
        $width = $node.Bounds.Right - $node.Bounds.Left
        $height = $node.Bounds.Bottom - $node.Bounds.Top
        if ($width -le 0 -or $height -le 0) {
            throw "Text node has zero-sized bounds: $($node.Text) [$($node.Bounds.Left),$($node.Bounds.Top)][$($node.Bounds.Right),$($node.Bounds.Bottom)]"
        }
    }
}

function Test-TextNodesDoNotOverlap([object[]]$TextNodes) {
    for ($i = 0; $i -lt $TextNodes.Count; $i++) {
        for ($j = $i + 1; $j -lt $TextNodes.Count; $j++) {
            $a = $TextNodes[$i]
            $b = $TextNodes[$j]
            if ($a.Text -eq $b.Text) { continue }
            if ($a.Text.Contains($b.Text) -or $b.Text.Contains($a.Text)) { continue }
            if ($a.Text -match '(年|月|日|上午|下午|:|点|分钟|小时)' -and $b.Text -match '(年|月|日|上午|下午|:|点|分钟|小时)') { continue }
            if (($a.Text -match '(年|月|日|上午|下午|:|点|分钟|小时)' -and $b.Text -match '^\d+$') -or
                ($b.Text -match '(年|月|日|上午|下午|:|点|分钟|小时)' -and $a.Text -match '^\d+$')) { continue }

            $left = [Math]::Max($a.Bounds.Left, $b.Bounds.Left)
            $top = [Math]::Max($a.Bounds.Top, $b.Bounds.Top)
            $right = [Math]::Min($a.Bounds.Right, $b.Bounds.Right)
            $bottom = [Math]::Min($a.Bounds.Bottom, $b.Bounds.Bottom)
            $overlap = [Math]::Max(0, $right - $left) * [Math]::Max(0, $bottom - $top)
            $smallerArea = [Math]::Min(
                ($a.Bounds.Right - $a.Bounds.Left) * ($a.Bounds.Bottom - $a.Bounds.Top),
                ($b.Bounds.Right - $b.Bounds.Left) * ($b.Bounds.Bottom - $b.Bounds.Top))
            if ($smallerArea -gt 0 -and $overlap / $smallerArea -gt 0.05) {
                throw "Text nodes overlap: '$($a.Text)' and '$($b.Text)'"
            }
        }
    }
}

function Test-ScreenshotIsNotBlank([string]$Path) {
    Add-Type -AssemblyName System.Drawing
    $bitmap = [System.Drawing.Bitmap]::FromFile($Path)
    try {
        $colors = [System.Collections.Generic.HashSet[int]]::new()
        # 低对比深色页在稀疏网格下可能只采到背景/前景两色；
        # 10px 网格能覆盖文本边缘和控件描边，同时保持开销可忽略。
        $xStep = 10
        $yStep = 10
        for ($x = 0; $x -lt $bitmap.Width; $x += $xStep) {
            for ($y = 0; $y -lt $bitmap.Height; $y += $yStep) {
                [void]$colors.Add($bitmap.GetPixel($x, $y).ToArgb())
            }
        }
        if ($colors.Count -lt 2) {
            throw "Screenshot appears blank: $Path"
        }
    }
    finally {
        $bitmap.Dispose()
    }
}

function Start-ControlsApp {
    Invoke-HdcShell "aa force-stop $App" | Out-Null
    $startOutput = Invoke-HdcShell "aa start -a EntryAbility -b $App -m entry -W"
    if ($startOutput -notmatch 'start ability successfully') {
        throw "Failed to start $App`: $startOutput"
    }

    $deadline = [DateTime]::UtcNow.AddSeconds(15)
    do {
        $processId = Invoke-HdcShell "pidof $App"
        if ($processId -notmatch '\d+') {
            Start-Sleep -Milliseconds 500
            continue
        }

        Invoke-DumpLayout "startup"
        $layout = Get-Content (Get-LayoutPath "startup") -Raw | ConvertFrom-Json -Depth 100 -AsHashtable
        if (Get-TextNodes $layout | Where-Object { $_.Text -eq "CS-ALL-MARK" }) {
            return
        }
        Start-Sleep -Milliseconds 500
    } while ([DateTime]::UtcNow -lt $deadline)

    throw "Timed out waiting for $App main page text."
}

function Click-Text([string]$Text) {
    Invoke-DumpLayout "click-$Text"
    $layout = Get-Content (Get-LayoutPath "click-$Text") -Raw | ConvertFrom-Json -Depth 100 -AsHashtable
    $node = Get-TextNodes $layout | Where-Object { $_.Text -eq $Text } | Select-Object -First 1
    if (-not $node) {
        throw "Text not found on screen: $Text"
    }
    $x = [int](($node.Bounds.Left + $node.Bounds.Right) / 2)
    $y = [int](($node.Bounds.Top + $node.Bounds.Bottom) / 2)
    Invoke-HdcShell "uitest uiInput click $x $y" | Out-Null
    Start-Sleep -Seconds 2
}

function Assert-Page([string]$Name, [string]$Marker) {
    Invoke-DumpLayout $Name
    Invoke-Screenshot $Name
    $layoutPath = Get-LayoutPath $Name
    $screenshotPath = Get-ScreenshotPath $Name
    if (-not (Test-Path $layoutPath) -or -not (Test-Path $screenshotPath)) {
        throw "Verification artifacts were not captured for $Name."
    }

    $layout = Get-Content $layoutPath -Raw | ConvertFrom-Json -Depth 100 -AsHashtable
    $textNodes = @(Get-TextNodes $layout)
    if (-not ($textNodes | Where-Object { $_.Text -eq $Marker })) {
        throw "Marker $Marker was not visible on $Name."
    }
    if ($textNodes.Count -lt 1) {
        throw "No application text nodes were found on $Name."
    }
    Test-TextNodesHaveArea $textNodes
    Test-TextNodesDoNotOverlap $textNodes
    Test-ScreenshotIsNotBlank $screenshotPath
    Write-Host "[ok] $Name ($($textNodes.Count) text nodes)"
}

function Scroll-ToMarker([string]$Marker) {
    $targetIndex = [array]::IndexOf(($sections | ForEach-Object { $_.Marker }), $Marker)
    for ($attempt = 1; $attempt -le 12; $attempt++) {
        Invoke-DumpLayout "scroll-$Marker"
        $layout = Get-Content (Get-LayoutPath "scroll-$Marker") -Raw | ConvertFrom-Json -Depth 100 -AsHashtable
        $node = Get-AllTextNodes $layout | Where-Object { $_.Text -eq $Marker } | Select-Object -First 1
        if ($node -and $node.Bounds.Top -ge 137 -and $node.Bounds.Bottom -le 2856) {
            return
        }

        $visibleMarkers = Get-TextNodes $layout | Where-Object { $_.Text -like "CS-*-MARK" }
        $currentIndex = -1
        foreach ($visibleMarker in $visibleMarkers) {
            $index = [array]::IndexOf(($sections | ForEach-Object { $_.Marker }), $visibleMarker.Text)
            if ($index -gt $currentIndex) { $currentIndex = $index }
        }

        if ($currentIndex -gt $targetIndex) {
            Invoke-HdcShell "uitest uiInput swipe 660 1600 660 2200 300" | Out-Null
        } else {
            Invoke-HdcShell "uitest uiInput swipe 660 2200 660 1600 300" | Out-Null
        }
        Start-Sleep -Milliseconds 700
    }

    throw "Could not scroll to marker $Marker."
}

$outputRoot = Join-Path $repoRoot "artifacts\controls-sample"
New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null
Remove-Item (Join-Path $outputRoot "*") -Force -ErrorAction SilentlyContinue

$sections = @(
    @{ Name = "base"; Marker = "CS-BASICS-MARK" },
    @{ Name = "inputs"; Marker = "CS-INPUTS-MARK" },
    @{ Name = "layouts"; Marker = "CS-LAYOUTS-MARK" },
    @{ Name = "items"; Marker = "CS-ITEMS-MARK" },
    @{ Name = "media"; Marker = "CS-MEDIA-MARK" },
    @{ Name = "menus"; Marker = "CS-MENUS-MARK" }
)

$targets = @(& $hdc list targets | ForEach-Object { "$_".Trim() } | Where-Object { $_ -and $_ -ne "[Empty]" })
if (-not $targets) {
    throw "No HarmonyOS device is connected."
}
$resolvedTarget = if ($Target) { $Target } else { $env:HDC_TARGET }
if ($resolvedTarget -and $resolvedTarget -notin $targets) {
    throw "Target $resolvedTarget is not in hdc list targets."
}
if ($resolvedTarget -and -not $Target) {
    $hdcArgs += @("-t", $resolvedTarget)
}

Write-Host "hdc: $hdc"
Write-Host "target: $(if ($resolvedTarget) { $resolvedTarget } else { $targets[0] })"
Write-Host "app: $App"
Write-Host "artifacts: $outputRoot"

foreach ($page in $sections) {
    Click-Text $page.Name
    Assert-Page $page.Name $page.Marker
}

Write-Host "ControlsSampleApp device verification passed for $($sections.Count) pages."
