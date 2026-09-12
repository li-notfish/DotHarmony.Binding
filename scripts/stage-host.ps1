# 宿主工程生成：模板 → 按应用定制的暂存实例（HarmonyOS.Maui.App.targets 的 HarmonyStageHost 调用）。
# 只重写"应用身份"字段（bundleName / 应用名）；权限、Ability、C shim 等与模板保持一致。
# 用法：pwsh stage-host.ps1 -Template <模板目录> -Destination <暂存目录> -BundleId <xxx.yyy.zzz> -AppTitle <显示名>
param(
    [Parameter(Mandatory = $true)][string]$Template,
    [Parameter(Mandatory = $true)][string]$Destination,
    [Parameter(Mandatory = $true)][string]$BundleId,
    [Parameter(Mandatory = $true)][string]$AppTitle
)
$ErrorActionPreference = "Stop"

if (-not (Test-Path "$Template/AppScope/app.json5")) {
    throw "宿主模板无效：$Template（缺 AppScope/app.json5）"
}
$BundleId = $BundleId.ToLowerInvariant()
if ($BundleId -notmatch '^[a-z0-9]+(\.[a-z0-9_-]+)+$') {
    throw "BundleId 非法：'$BundleId'（须为点分小写域名形式，如 com.example.myapp）"
}

$buildStamp = Join-Path $Destination ".stage-stamp"
$upToDate = (Test-Path $buildStamp) -and
    ((Get-Item $buildStamp).LastWriteTimeUtc -ge (Get-Item "$Template/AppScope/app.json5").LastWriteTimeUtc) -and
    ((Get-Content $buildStamp -Raw).Trim() -eq "$BundleId|$AppTitle".Trim())
if ($upToDate) {
    Write-Host "=== host staged (up-to-date): $Destination"
    exit 0
}

Write-Host "=== staging host: $Template -> $Destination (bundle=$BundleId)"
if (Test-Path $Destination) {
    Remove-Item $Destination -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $Destination | Out-Null
# 全量复制模板（含隐藏文件），再剔除构建产物——产物进暂存目录无意义且会把模板里的陈旧输出带进去
Copy-Item "$Template/*" $Destination -Recurse -Force
foreach ($stale in @("entry/build", "entry/.cxx", ".hvigor")) {
    $p = Join-Path $Destination $stale
    if (Test-Path $p) { Remove-Item $p -Recurse -Force }
}

# 1) bundleName
$appJson = Join-Path $Destination "AppScope/app.json5"
(Set-Content $appJson ((Get-Content $appJson -Raw) -replace '"bundleName"\s*:\s*"[^"]*"', "`"bundleName`": `"$BundleId`"")) | Out-Null

# 2) 应用显示名（AppScope 资源，module/ability 的 label 都引用它）
$appString = Join-Path $Destination "AppScope/resources/base/element/string.json"
if (Test-Path $appString) {
    (Set-Content $appString ((Get-Content $appString -Raw) -replace
        ('("name"\s*:\s*"app_name"\s*,\s*"value"\s*:\s*")[^"]*(")'), ('$1' + $AppTitle.Replace('$', '$$') + '$2'))) | Out-Null
}

Set-Content $buildStamp "$BundleId|$AppTitle"
Write-Host "=== host staged OK"
