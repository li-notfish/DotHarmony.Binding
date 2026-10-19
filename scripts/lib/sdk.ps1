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

# SDK 根判定不依赖 hdc：SDK 可以只有 ets/native/toolchains 元数据（Linux CI、
# 纯打包机），此时仍需 staging/签名。hdc 只在部署路径上才必需。
function Test-HarmonySdkRoot([string]$Path) {
    if (-not $Path -or -not (Test-Path -LiteralPath $Path)) { return $false }
    return (Test-Path -LiteralPath (Join-Path $Path "ets\oh-uni-package.json")) -or
        (Test-Path -LiteralPath (Join-Path $Path "toolchains\hdc.exe")) -or
        (Test-Path -LiteralPath (Join-Path $Path "toolchains\hdc"))
}

function Get-HarmonySdkRootCandidates([string]$Candidate) {
    if (-not $Candidate -or -not (Test-Path -LiteralPath $Candidate)) { return @() }
    if (Test-HarmonySdkRoot $Candidate) { return @($Candidate) }

    $found = @()
    $children = @(Get-ChildItem -LiteralPath $Candidate -Directory -ErrorAction SilentlyContinue)
    foreach ($child in $children) {
        if (Test-HarmonySdkRoot $child.FullName) {
            $found += $child.FullName
            continue
        }
        $grandchildren = @(Get-ChildItem -LiteralPath $child.FullName -Directory -ErrorAction SilentlyContinue)
        foreach ($grandchild in $grandchildren) {
            if (Test-HarmonySdkRoot $grandchild.FullName) {
                $found += $grandchild.FullName
            }
        }
    }
    return Sort-HarmonySdkRoots $found
}

function Get-HarmonySdkCandidates {
    return @(
        $env:OHOS_SDK_BASE,
        $env:OHOS_SDK_HOME,
        $env:OHSDK_HOME,
        $env:DEVECO_SDK_HOME,
        $env:HOS_SDK_HOME,
        $(if ($env:DEVECO_HOME) { Join-Path $env:DEVECO_HOME "sdk" }),
        $(if ($IsMacOS) { Join-Path $env:HOME "Library/Huawei/Sdk" }),
        $(if ($IsLinux) { Join-Path $env:HOME "Huawei/Sdk" }),
        "D:\Harmony\OpenHarmony\Sdk",
        "C:\Harmony\OpenHarmony\Sdk",
        "D:\Program Files\Huawei\DevEco Studio\sdk",
        "C:\Program Files\Huawei\DevEco Studio\sdk"
    ) | Where-Object { $_ } | Select-Object -Unique
}

function Find-HarmonySdkRoot {
    $tried = @()
    foreach ($candidate in Get-HarmonySdkCandidates) {
        $roots = @(Get-HarmonySdkRootCandidates $candidate)
        if ($roots.Count -gt 0) { return $roots[0] }
        $tried += $candidate
    }

    $checked = if ($tried.Count) { $tried -join ', ' } else { '(none)' }
    throw "找不到 HarmonyOS/OpenHarmony SDK。请设置 OHOS_SDK_BASE/OHSDK_HOME/DEVECO_SDK_HOME。已检查：$checked"
}

function Find-HarmonyHdc {
    if ($env:HARMONY_HDC) {
        $resolvedHdc = $env:HARMONY_HDC
        if (-not (Test-Path -LiteralPath $resolvedHdc)) {
            throw "HARMONY_HDC 指向不存在的 hdc：$resolvedHdc"
        }
        return (Resolve-Path -LiteralPath $resolvedHdc).Path
    }

    $root = Find-HarmonySdkRoot
    foreach ($name in @("hdc.exe", "hdc")) {
        $candidate = Join-Path $root "toolchains\$name"
        if (Test-Path -LiteralPath $candidate) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "SDK 中找不到 hdc（$root\toolchains）。请确认 SDK 完整，或设置 HARMONY_HDC。"
}

function Get-HarmonySdkRuntimeOS([string]$SdkRoot) {
    if ($env:HARMONY_RUNTIME_OS) { return $env:HARMONY_RUNTIME_OS }
    foreach ($pkg in @(
            (Join-Path $SdkRoot "sdk-pkg.json"),
            (Join-Path (Split-Path -Parent $SdkRoot) "sdk-pkg.json"))) {
        if (-not (Test-Path -LiteralPath $pkg)) { continue }
        try {
            $data = (Get-Content -LiteralPath $pkg -Raw | ConvertFrom-Json).data
            $label = "$($data.displayName) $($data.path)"
            if ($label -match 'OpenHarmony') { return "OpenHarmony" }
            if ($label -match 'HarmonyOS') { return "HarmonyOS" }
        } catch {
            Write-Warning "无法解析 SDK 元数据 $pkg：$($_.Exception.Message)"
        }
    }
    return "HarmonyOS"
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
