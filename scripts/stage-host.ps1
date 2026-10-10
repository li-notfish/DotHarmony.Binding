# 宿主工程生成：模板 → 按应用定制的暂存实例（HarmonyOS.Maui.App.targets 的 HarmonyStageHost 调用）。
# 重写"应用身份"字段（bundleName / 应用名），可选注入签名配置（release 用）；
# 权限由 permissions.json 生成，Ability、C shim 等与模板保持一致。
# 用法：pwsh stage-host.ps1 -Template <模板目录> -Destination <暂存目录> -BundleId <xxx.yyy.zzz> -AppTitle <显示名>
#        [-SigningKeystore <p12路径> -SigningKeystorePassword <口令> -SigningCertAlias <别名>
#         -SigningCertPassword <口令> -SigningProfile <p7b路径>]
# 签名参数只参与暂存内容戳；实际签名由 sign-hap.ps1 在 unsigned HAP 生成后执行。
param(
    [Parameter(Mandatory = $true)][string]$Template,
    [Parameter(Mandatory = $true)][string]$Destination,
    [Parameter(Mandatory = $true)][string]$BundleId,
    [Parameter(Mandatory = $true)][string]$AppTitle,
    [string]$PermissionsJson = "",
    [string]$SigningKeystore = "",
    [string]$SigningKeystorePassword = "",
    [string]$SigningCertAlias = "",
    [string]$SigningCertPassword = "",
    [string]$SigningCertPath = "",
    [string]$SigningProfile = ""
)
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrEmpty($SigningKeystorePassword)) {
    $SigningKeystorePassword = $env:HarmonySigningKeystorePassword
}
if ([string]::IsNullOrEmpty($SigningCertPassword)) {
    $SigningCertPassword = $env:HarmonySigningCertPassword
}

if (-not (Test-Path "$Template/AppScope/app.json5")) {
    throw "宿主模板无效：$Template（缺 AppScope/app.json5）"
}
$BundleId = $BundleId.ToLowerInvariant()
if ($BundleId -notmatch '^[a-z0-9]+(\.[a-z0-9_-]+)+$') {
    throw "BundleId 非法：'$BundleId'（须为点分小写域名形式，如 com.example.myapp）"
}

# 签名物料：要么全不配置（未签名 debug 链），要么六项全齐；缺项给清晰报错
$signingSet = @($SigningKeystore, $SigningKeystorePassword, $SigningCertAlias, $SigningCertPassword, $SigningCertPath, $SigningProfile) |
    Where-Object { $_ -ne "" }
$signingEnabled = $signingSet.Count -gt 0
if ($signingEnabled -and $signingSet.Count -lt 6) {
    throw ("签名配置不完整：HarmonySigningKeystore / HarmonySigningKeystorePassword / HarmonySigningCertAlias / " +
           "HarmonySigningCertPassword / HarmonySigningCertPath / HarmonySigningProfile 须全部设置" +
           "（当前只提供了 $($signingSet.Count) 项）")
}
if ($signingEnabled) {
    foreach ($p in @($SigningKeystore, $SigningProfile, $SigningCertPath)) {
        if (-not (Test-Path $p)) { throw "签名物料不存在：$p" }
    }
}

$buildStamp = Join-Path $Destination ".stage-stamp"
# 修改 staging 行为时递增该版本，让已有暂存宿主自动重建。
$stageVersion = "4"

function Get-TemplateContentHash([string]$Root) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $rootPath = (Resolve-Path -LiteralPath $Root).Path.TrimEnd('\', '/')
        $files = @(Get-ChildItem -LiteralPath $rootPath -Recurse -File -Force |
            Sort-Object { $_.FullName.Substring($rootPath.Length) })
        foreach ($file in $files) {
            $relative = $file.FullName.Substring($rootPath.Length).TrimStart('\', '/').Replace('\', '/')
            $pathBytes = [System.Text.Encoding]::UTF8.GetBytes($relative)
            $lengthBytes = [System.BitConverter]::GetBytes([int64]$file.Length)
            [void]$sha.TransformBlock($pathBytes, 0, $pathBytes.Length, $null, 0)
            [void]$sha.TransformBlock($lengthBytes, 0, $lengthBytes.Length, $null, 0)
            $stream = [System.IO.File]::OpenRead($file.FullName)
            try {
                $buffer = [byte[]]::new(81920)
                while (($read = $stream.Read($buffer, 0, $buffer.Length)) -gt 0) {
                    [void]$sha.TransformBlock($buffer, 0, $read, $null, 0)
                }
            } finally {
                $stream.Dispose()
            }
        }
        [void]$sha.TransformFinalBlock([byte[]]::new(0), 0, 0)
        return [System.Convert]::ToHexString($sha.Hash)
    } finally {
        $sha.Dispose()
    }
}

$templateHash = Get-TemplateContentHash $Template
# 签名参数参与内容戳（变化则重 stage）；口令只参与哈希不落明文
$signingStamp = if ($signingEnabled) {
    $h = [System.BitConverter]::ToString(
        [System.Security.Cryptography.SHA256]::HashData(
            [System.Text.Encoding]::UTF8.GetBytes(
                "$SigningKeystore|$SigningKeystorePassword|$SigningCertAlias|$SigningCertPassword|$SigningCertPath|$SigningProfile")))
    "signed:$h"
} else { "unsigned" }
$permissionsStamp = "none"
if ($PermissionsJson -and (Test-Path -LiteralPath $PermissionsJson)) {
    $permissionsHash = [System.BitConverter]::ToString(
        [System.Security.Cryptography.SHA256]::HashData(
            [System.IO.File]::ReadAllBytes($PermissionsJson)))
    $permissionsStamp = "permissions:$permissionsHash"
}
$stampContent = "$stageVersion|template:$templateHash|$BundleId|$AppTitle|$permissionsStamp|$signingStamp"
$upToDate = (Test-Path $buildStamp) -and
    ((Get-Content $buildStamp -Raw).Trim() -eq $stampContent.Trim())
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
$appText = Get-Content $appJson -Raw
$bundlePattern = [regex]'"bundleName"\s*:\s*"[^"]*"'
if (-not $bundlePattern.IsMatch($appText)) {
    throw "宿主模板结构无法识别：AppScope/app.json5 缺少 bundleName 字段"
}
$appText = $bundlePattern.Replace($appText, "`"bundleName`": `"$BundleId`"", 1)
if ($appText -notmatch ('"bundleName"\s*:\s*"' + [regex]::Escape($BundleId) + '"')) {
    throw "宿主暂存校验失败：bundleName 未更新为 $BundleId"
}
Set-Content $appJson $appText

function Set-RequiredJsonValue([string]$Path, [string]$Name, [string]$Value, [string]$Description) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "宿主模板结构无法识别：缺少 $Description 资源文件 $Path"
    }
    $text = Get-Content -LiteralPath $Path -Raw
    $pattern = [regex]('("' + [regex]::Escape($Name) + '"\s*,\s*"value"\s*:\s*")[^"]*(")')
    if (-not $pattern.IsMatch($text)) {
        throw "宿主模板结构无法识别：$Description 中缺少 $Name"
    }
    $escaped = $Value.Replace('\', '\\').Replace('"', '\"').Replace('$', '$$')
    $updated = $pattern.Replace($text, '${1}' + $escaped + '${2}', 1)
    if ($updated -eq $text) { throw "宿主暂存替换未生效：$Description / $Name" }
    Set-Content -LiteralPath $Path -Value $updated
    $json = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    $entry = @($json.string) | Where-Object { $_.name -eq $Name } | Select-Object -First 1
    if (-not $entry -or $entry.value -cne $Value) {
        throw "宿主暂存校验失败：$Description / $Name 最终值不是 '$Value'"
    }
}

# 2) 应用显示名（AppScope 资源，module/ability 的 label 都引用它）
$appString = Join-Path $Destination "AppScope/resources/base/element/string.json"
if (Test-Path $appString) {
    Set-RequiredJsonValue $appString "app_name" $AppTitle "AppScope 应用名"
} else {
    throw "宿主模板结构无法识别：缺少 AppScope 应用名资源文件"
}

# 3) Ability 显示名（入口模块资源：launcher/最近任务/权限弹窗上看到的名字走这条，
#    不改则全部共用模板的 HarmonyHost —— staging 语义就是把这一层也拨过去）
$abilityString = Join-Path $Destination "entry/src/main/resources/base/element/string.json"
if (Test-Path $abilityString) {
    Set-RequiredJsonValue $abilityString "EntryAbility_label" $AppTitle "入口 Ability 标签"
} else {
    throw "宿主模板结构无法识别：缺少入口 Ability 标签资源文件"
}

# 4) 清空模板继承的 signingConfigs。签名由 hap-sign-tool 处理，避免 hvigor
#    对 storePassword/keyPassword 明文字段的 32 字符校验限制。
$buildProfile = Join-Path $Destination "build-profile.json5"
$profileText = Get-Content $buildProfile -Raw
$signingPattern = [regex]'"signingConfigs"\s*:\s*\[[\s\S]*?\]\s*,\s*"products"'
if (-not $signingPattern.IsMatch($profileText)) {
    throw "宿主模板结构无法识别：build-profile.json5 缺少 signingConfigs/products"
}
$profileText = $signingPattern.Replace($profileText, "`"signingConfigs`": [],`r`n    `"products`"", 1)
if ($profileText -notmatch '"signingConfigs"\s*:\s*\[\s*\]\s*,\s*"products"') {
    throw "宿主暂存校验失败：signingConfigs 未清空"
}
Set-Content $buildProfile $profileText

# 5) 根据应用代码推导出的权限生成 module.json5 与权限说明资源。
$permissions = @()
if ($PermissionsJson -and (Test-Path -LiteralPath $PermissionsJson)) {
    $permissionManifest = Get-Content -LiteralPath $PermissionsJson -Raw | ConvertFrom-Json
    if ($permissionManifest.permissions) {
        $permissions = @($permissionManifest.permissions)
    }
}

$requestPermissions = @()
$permissionStrings = @()
foreach ($permission in $permissions) {
    $permissionName = $permission.name
    $when = if ($permission.when) { $permission.when } else { "always" }
    $shortName = $permissionName -replace '^ohos\.permission\.', ''
    $reasonName = "permission_${shortName}_reason"
    $requestPermissions += [ordered]@{
        name = $permissionName
        reason = "`$string:$reasonName"
        usedScene = [ordered]@{
            abilities = @("EntryAbility")
            when = $when
        }
    }
    $permissionStrings += [ordered]@{
        name = $reasonName
        value = "Allow the app to use $permissionName"
    }
}

$moduleJson = [ordered]@{
    module = [ordered]@{
        requestPermissions = $requestPermissions
        name = "entry"
        type = "entry"
        description = "`$string:module_desc"
        mainElement = "EntryAbility"
        deviceTypes = @("phone", "tablet", "2in1")
        deliveryWithInstall = $true
        installationFree = $false
        pages = "`$profile:main_pages"
        abilities = @(
            [ordered]@{
                name = "EntryAbility"
                srcEntry = "./ets/entryability/EntryAbility.ets"
                description = "`$string:ability_desc"
                icon = "`$media:icon"
                label = "`$string:EntryAbility_label"
                startWindowIcon = "`$media:icon"
                startWindowBackground = "`$color:start_window_background"
                exported = $true
                skills = @(
                    [ordered]@{
                        entities = @("entity.system.home")
                        actions = @("action.system.home")
                    }
                )
            }
        )
    }
}

$moduleJsonPath = Join-Path $Destination "entry\src\main\module.json5"
$moduleJson | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $moduleJsonPath

$entryStringJson = [ordered]@{
    string = @(
        [ordered]@{
            name = "module_desc"
            value = "HarmonyOS host module"
        },
        [ordered]@{
            name = "EntryAbility_label"
            value = $AppTitle
        },
        [ordered]@{
            name = "ability_desc"
            value = "HarmonyOS host ability"
        }
    ) + $permissionStrings
}

$entryStringJsonPath = Join-Path $Destination "entry\src\main\resources\base\element\string.json"
$entryStringJson | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $entryStringJsonPath

Set-Content $buildStamp $stampContent
Write-Host "=== host staged OK"
