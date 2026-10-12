# 模板包验收：pack 产品包 + HarmonyOS.Templates → dotnet new install → 实例化 →
# HarmonyStageHost（验证 buildTransitive targets/宿主模板随包分发）+ 托管编译。
# 版本号每次随机，避免 NuGet 全局缓存命中旧同版本包。
param(
    [string]$FeedDir = "",
    [string]$WorkDir = "",
    [switch]$Full
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $FeedDir) {
    $FeedDir = Join-Path $repoRoot "artifacts\local-feed"
}
if (-not $WorkDir) {
    $WorkDir = Join-Path $env:TEMP "harmony-template-consumer"
}

if (Test-Path -LiteralPath $WorkDir) {
    Remove-Item -LiteralPath $WorkDir -Recurse -Force
}
New-Item -ItemType Directory -Path $WorkDir | Out-Null

$projects = @(
    "src\HarmonyOS.Interop\HarmonyOS.Interop.csproj",
    "src\HarmonyOS.Maui.Permissions.Abstractions\HarmonyOS.Maui.Permissions.Abstractions.csproj",
    "src\HarmonyOS.Bindings\HarmonyOS.Bindings.csproj",
    "src\HarmonyOS.Essentials\HarmonyOS.Essentials.csproj",
    "src\HarmonyOS.Maui\HarmonyOS.Maui.csproj",
    "src\HarmonyOS.Maui.Web\HarmonyOS.Maui.Web.csproj",
    "src\HarmonyOS.Templates\HarmonyOS.Templates.csproj"
)

$packageVersion = "1.0.$(Get-Random -Minimum 100000 -Maximum 999999)-local"

$buildPropsPath = Join-Path $repoRoot "Directory.Build.props"
$packagesPropsPath = Join-Path $repoRoot "Directory.Packages.props"
$templateJsonPaths = @(
    "src\HarmonyOS.Templates\content\harmony-maui\.template.config\template.json",
    "src\HarmonyOS.Templates\content\harmony-maui-web\.template.config\template.json"
)

$buildProps = [xml](Get-Content -LiteralPath $buildPropsPath -Raw)
$packagesProps = [xml](Get-Content -LiteralPath $packagesPropsPath -Raw)
$expectedPackageVersion = $buildProps.Project.PropertyGroup.PackageVersion
$expectedPublishAotClangVersion = @($packagesProps.Project.ItemGroup.PackageVersion |
    Where-Object { $_.Include -eq "PublishAotClang" })[0].Version

foreach ($templateJsonPath in $templateJsonPaths) {
    $templateJson = Get-Content -LiteralPath (Join-Path $repoRoot $templateJsonPath) -Raw | ConvertFrom-Json
    if ($templateJson.symbols.packageVersion.defaultValue -ne $expectedPackageVersion) {
        throw "Template HarmonyOS.Maui version '$($templateJson.symbols.packageVersion.defaultValue)' does not match Directory.Build.props version '$expectedPackageVersion'."
    }
    if ($templateJson.symbols.publishAotClangVersion.defaultValue -ne $expectedPublishAotClangVersion) {
        throw "Template PublishAotClang version '$($templateJson.symbols.publishAotClangVersion.defaultValue)' does not match Directory.Packages.props version '$expectedPublishAotClangVersion'."
    }
}

foreach ($project in $projects) {
    dotnet pack (Join-Path $repoRoot $project) -c Release -o $FeedDir -p:PackageVersion=$packageVersion
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to pack $project"
    }
}

dotnet new uninstall HarmonyOS.Templates | Out-Null
dotnet new install (Join-Path $FeedDir "HarmonyOS.Templates.$packageVersion.nupkg")
if ($LASTEXITCODE -ne 0) {
    throw "Failed to install HarmonyOS.Templates."
}

$appDir = Join-Path $WorkDir "TemplateApp"
$webAppDir = Join-Path $WorkDir "TemplateWebApp"
Set-Location $WorkDir
dotnet new harmony-maui -n TemplateApp --packageVersion $packageVersion
if ($LASTEXITCODE -ne 0) {
    throw "Failed to instantiate harmony-maui template."
}

dotnet new harmony-maui-web -n TemplateWebApp --packageVersion $packageVersion
if ($LASTEXITCODE -ne 0) {
    throw "Failed to instantiate harmony-maui-web template."
}

@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local-feed" value="$FeedDir" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -LiteralPath (Join-Path $WorkDir "nuget.config")

Set-Location $repoRoot
$appProject = Join-Path $appDir "TemplateApp.csproj"
$webAppProject = Join-Path $webAppDir "TemplateWebApp.csproj"
dotnet build $appProject -t:HarmonyStageHost -c Release
if ($LASTEXITCODE -ne 0) {
    throw "harmony-maui template app build failed."
}

dotnet build $webAppProject -t:HarmonyStageWebHost -c Release
if ($LASTEXITCODE -ne 0) {
    throw "harmony-maui-web template app build failed."
}

$stagedHost = Join-Path $appDir "obj\harmony\host\entry\src\main\module.json5"
if (-not (Test-Path -LiteralPath $stagedHost)) {
    throw "Staged host not found: $stagedHost"
}

$normalProjectText = Get-Content -LiteralPath $appProject -Raw
if ($normalProjectText -match "HarmonyOS\.Maui\.Web|HarmonyWebExports|ohos\.permission\.INTERNET") {
    throw "harmony-maui template contains WebView support."
}
if (Test-Path -LiteralPath (Join-Path $appDir "Platforms\HarmonyOS\HarmonyWebExports.cs")) {
    throw "harmony-maui template contains HarmonyWebExports.cs."
}

$webProjectText = Get-Content -LiteralPath $webAppProject -Raw
if ($webProjectText -notmatch "HarmonyOS\.Maui\.Web") {
    throw "harmony-maui-web template does not reference HarmonyOS.Maui.Web."
}
if ($webProjectText -notmatch "ohos\.permission\.INTERNET") {
    throw "harmony-maui-web template does not declare ohos.permission.INTERNET."
}
$webExportsPath = Join-Path $webAppDir "Platforms\HarmonyOS\HarmonyWebExports.cs"
if (-not (Test-Path -LiteralPath $webExportsPath)) {
    throw "harmony-maui-web template does not contain HarmonyWebExports.cs."
}
$webExportsText = Get-Content -LiteralPath $webExportsPath -Raw
foreach ($export in @("HarmonyWebInit", "HarmonyWebInterceptRequest", "HarmonyWebReceiveMessage")) {
    if ($webExportsText -notmatch [regex]::Escape($export)) {
        throw "harmony-maui-web HarmonyWebExports.cs is missing the $export export."
    }
}

$webStagedIndex = Join-Path $webAppDir "obj\harmony\host\entry\src\main\ets\pages\Index.ets"
$webStagedModule = Join-Path $webAppDir "obj\harmony\host\entry\src\main\module.json5"
foreach ($file in @($webStagedIndex, $webStagedModule)) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Expected staged Web host file not found: $file"
    }
}
$webStagedIndexText = Get-Content -LiteralPath $webStagedIndex -Raw
if ($webStagedIndexText -notmatch "passWebView|interceptWebViewRequest|receiveWebViewMessage") {
    throw "Staged Web host does not contain the WebView bridge."
}
$webStagedModuleText = Get-Content -LiteralPath $webStagedModule -Raw
if ($webStagedModuleText -notmatch "ohos\.permission\.INTERNET") {
    throw "Staged Web host does not contain ohos.permission.INTERNET."
}

if ($Full) {
    dotnet build $appProject -t:HarmonyBuildLibApp -c Release
    if ($LASTEXITCODE -ne 0) {
        throw "Template app NativeAOT library build failed."
    }

    dotnet build $appProject -t:HarmonyBuildHap -c Release
    if ($LASTEXITCODE -ne 0) {
        throw "Template app HAP build failed."
    }
}

Write-Host "Template consumer verification OK."
