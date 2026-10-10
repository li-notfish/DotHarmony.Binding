param([switch]$CheckDevice)
$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$script:Failed = 0

function Test-Requirement([string]$Name, [scriptblock]$Action) {
    try {
        $detail = & $Action
        Write-Host "[OK]   $Name$(if ($detail) { ": $detail" })"
    } catch {
        Write-Host "[FAIL] ${Name}: $($_.Exception.Message)"
        $script:Failed++
    }
}

Test-Requirement ".NET SDK" {
    $null = Get-Command dotnet -ErrorAction Stop
    $version = (& dotnet --version 2>&1 | Select-Object -Last 1).ToString().Trim()
    $major = if ($version -match '^\d+') { [int]$Matches[0] } else { 0 }
    if ($major -lt 10) { throw "需要 .NET 10 SDK，当前为 $version" }
    $version
}

Test-Requirement "Node.js" {
    $null = Get-Command node -ErrorAction Stop
    (& node --version 2>&1 | Select-Object -Last 1).ToString().Trim()
}

Test-Requirement "Python" {
    $python = Get-Command python3, python -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $python) { throw "未找到 python3/python" }
    (& $python.Source --version 2>&1 | Select-Object -Last 1).ToString().Trim()
}

$devEco = ""
Test-Requirement "DevEco Studio / hvigor" {
    $candidates = @(
        $env:DEVECO_HOME,
        "D:\Program Files\Huawei\DevEco Studio",
        "C:\Program Files\Huawei\DevEco Studio"
    ) | Where-Object { $_ -and (Test-Path (Join-Path $_ "tools\hvigor\bin\hvigorw.js")) }
    if (-not $candidates) { throw "未找到 hvigorw.js；请设置 DEVECO_HOME" }
    $script:devEco = $candidates | Select-Object -First 1
    $script:devEco
}

Test-Requirement "SDK / hdc" {
    . (Join-Path $scriptDir "lib\sdk.ps1")
    $hdc = Find-HarmonyHdc
    $sdkRoot = Find-HarmonySdkRoot
    $script:HdcPath = $hdc
    "SDK $(Get-HarmonySdkVersion $sdkRoot) / API $(Get-HarmonySdkApiVersion $sdkRoot), hdc=$hdc"
}

if ($CheckDevice) {
    Test-Requirement "设备连接" {
        if (-not $script:HdcPath) { throw "前序 hdc 检查失败" }
        $targets = @(& $script:HdcPath list targets |
            ForEach-Object { "$_".Trim() } |
            Where-Object { $_ -and $_ -ne "[Empty]" })
        if (-not $targets) { throw "hdc list targets 为空；请启动设备或执行 hdc tconn <host:port>" }
        $targets -join ", "
    }
} else {
    Write-Host "[SKIP] 设备连接（使用 -CheckDevice 启用）"
}

if ($script:Failed) {
    Write-Host "Harmony environment check failed: $($script:Failed) issue(s)."
    exit 1
}
Write-Host "Harmony environment check passed."
