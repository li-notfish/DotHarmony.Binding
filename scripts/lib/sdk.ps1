function Get-HarmonyPackageObject([string]$SdkRoot) {
    foreach ($relative in @(
            "ets\oh-uni-package.json",
            "oh-uni-package.json",
            "toolchains\oh-uni-package.json",
            "package.json")) {
        $file = Join-Path $SdkRoot $relative
        if (-not (Test-Path -LiteralPath $file)) { continue }
        try {
            return Get-Content -LiteralPath $file -Raw | ConvertFrom-Json -ErrorAction Stop
        } catch {
            Write-Warning "无法解析 SDK 元数据 $file：$($_.Exception.Message)"
        }
    }
    return $null
}

function Get-HarmonySdkVersion([string]$SdkRoot) {
    $package = Get-HarmonyPackageObject $SdkRoot
    foreach ($name in @("platformVersion", "version")) {
        $value = $package.$name
        if ($value -is [string] -and $value) { return $value }
    }
    $leaf = Split-Path -Leaf $SdkRoot
    if ($leaf -match '^\d+(?:\.\d+)*(?:-[0-9A-Za-z.-]+)?$') { return $leaf }
    return "unknown"
}

function Get-HarmonySdkApiVersion([string]$SdkRoot) {
    $package = Get-HarmonyPackageObject $SdkRoot
    $value = $package.apiVersion
    if ($value -is [string] -and $value -match '^\d+') { return [int]$Matches[0] }
    if ($value -is [int]) { return $value }
    $version = Get-HarmonySdkVersion $SdkRoot
    if ($version -match '^(\d+)') { return [int]$Matches[1] }
    throw "无法从 SDK 元数据推导 apiVersion：$SdkRoot。请设置 HARMONY_COMPATIBLE_VERSION。"
}

function Sort-HarmonySdkRoots([string[]]$Roots) {
    $items = foreach ($root in $Roots) {
        $version = Get-HarmonySdkVersion $root
        $stableRank = if ($version -match '^\d+(?:\.\d+)*$') { 0 }
            elseif ($version -match '^\d+(?:\.\d+)*-') { 1 } else { 2 }
        $numeric = ($version -replace '-.*$', '') -replace '[^\d.].*$', ''
        $parsed = $null
        if ($numeric -match '^\d+(?:\.\d+)*$') { $parsed = [version]$numeric }
        [pscustomobject]@{
            Root = $root
            StableRank = $stableRank
            ParsedVersion = $parsed
            Version = $version
        }
    }
    return @($items |
        Sort-Object -Property `
            @{ Expression = { $_.StableRank } }, `
            @{ Expression = { $_.ParsedVersion }; Descending = $true }, `
            @{ Expression = { $_.Version }; Descending = $true } |
        ForEach-Object { $_.Root })
}

function Get-HarmonySdkRootCandidates([string]$Candidate) {
    if (-not $Candidate -or -not (Test-Path -LiteralPath $Candidate)) { return @() }
    if (Test-Path -LiteralPath (Join-Path $Candidate "toolchains\hdc.exe")) { return @($Candidate) }

    $found = @()
    $children = @(Get-ChildItem -LiteralPath $Candidate -Directory -ErrorAction SilentlyContinue)
    foreach ($child in $children) {
        if (Test-Path -LiteralPath (Join-Path $child.FullName "toolchains\hdc.exe")) {
            $found += $child.FullName
            continue
        }
        $grandchildren = @(Get-ChildItem -LiteralPath $child.FullName -Directory -ErrorAction SilentlyContinue)
        foreach ($grandchild in $grandchildren) {
            if (Test-Path -LiteralPath (Join-Path $grandchild.FullName "toolchains\hdc.exe")) {
                $found += $grandchild.FullName
            }
        }
    }
    return Sort-HarmonySdkRoots $found
}

function Find-HarmonyHdc {
    $candidates = @(
        $env:OHOS_SDK_BASE,
        $env:OHOS_SDK_HOME,
        $env:OHSDK_HOME,
        $(if ($env:DEVECO_HOME) { Join-Path $env:DEVECO_HOME "sdk" }),
        "D:\Harmony\OpenHarmony\Sdk",
        "C:\Harmony\OpenHarmony\Sdk",
        "D:\Program Files\Huawei\DevEco Studio\sdk",
        "C:\Program Files\Huawei\DevEco Studio\sdk"
    ) | Where-Object { $_ } | Select-Object -Unique

    $tried = @()
    foreach ($candidate in $candidates) {
        if (-not (Test-Path -LiteralPath $candidate)) {
            Write-Warning "SDK 候选路径不存在：$candidate"
            $tried += $candidate
            continue
        }
        $roots = @(Get-HarmonySdkRootCandidates $candidate)
        if ($roots.Count -gt 0) {
            return Join-Path $roots[0] "toolchains\hdc.exe"
        }
        $tried += $candidate
        Write-Warning "SDK 候选中未找到 toolchains\hdc.exe：$candidate"
    }

    $checked = if ($tried.Count) { $tried -join ', ' } else { '(none)' }
    throw "找不到 hdc.exe。请设置 OHOS_SDK_BASE/OHSDK_HOME。已检查：$checked"
}

function Find-HarmonySdkRoot {
    $hdc = Find-HarmonyHdc
    return Split-Path -Parent (Split-Path -Parent $hdc)
}

function Get-HarmonyCompatibleVersion {
    if ($env:HARMONY_COMPATIBLE_VERSION) {
        if ($env:HARMONY_COMPATIBLE_VERSION -notmatch '^\d+$') {
            throw "HARMONY_COMPATIBLE_VERSION 必须是非负整数：$($env:HARMONY_COMPATIBLE_VERSION)"
        }
        return [int]$env:HARMONY_COMPATIBLE_VERSION
    }
    return Get-HarmonySdkApiVersion (Find-HarmonySdkRoot)
}
