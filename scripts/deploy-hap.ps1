# 一键部署：重装 HAP → 启动 → 抓取 HarmonyHost 日志
$ErrorActionPreference = "Stop"

# 获取脚本所在目录，然后跳转到上级（项目根目录）
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir

# 从环境变量读取 HDC 路径
if (-not $env:OHSDK_HOME) {
    Write-Error "请先设置环境变量 OHSDK_HOME"
    exit 1
}
$HDC = Join-Path $env:OHSDK_HOME "26.0.0\toolchains\hdc.exe"
if (-not (Test-Path $HDC)) {
    Write-Error "找不到 hdc.exe，请检查 OHSDK_HOME 是否指向正确的 toolchains 目录"
    exit 1
}

# 拼接 HAP 的相对路径（从项目根目录开始）
$HAP = Join-Path $projectRoot "samples\HarmonyHost\entry\build\default\outputs\default\entry-default-unsigned.hap"

Write-Host "=== 1. 清空 hilog ==="
& $HDC shell hilog -r > $null

Write-Host "=== 2. 安装 HAP ==="
& $HDC install -r $HAP

Write-Host "=== 3. 启动应用 ==="
& $HDC shell aa start -a EntryAbility -b com.arktsbinding.harmonyhost -m entry

Write-Host "=== 4. 等待后抓取日志 ==="
Start-Sleep -Seconds 6
& $HDC shell "hilog -x" | Select-String -Pattern 'A00000/HarmonyHost|dlopen|libapp|dotnet|DOTNET' 