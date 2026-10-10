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
    "src\HarmonyOS.Templates\HarmonyOS.Templates.csproj"
)

$packageVersion = "1.0.$(Get-Random -Minimum 100000 -Maximum 999999)-local"

$buildPropsPath = Join-Path $repoRoot "Directory.Build.props"
$packagesPropsPath = Join-Path $repoRoot "Directory.Packages.props"
$templateJsonPath = Join-Path $repoRoot "src\HarmonyOS.Templates\content\harmony-maui\.template.config\template.json"

$buildProps = [xml](Get-Content -LiteralPath $buildPropsPath -Raw)
$packagesProps = [xml](Get-Content -LiteralPath $packagesPropsPath -Raw)
$templateJson = Get-Content -LiteralPath $templateJsonPath -Raw | ConvertFrom-Json
$expectedPackageVersion = $buildProps.Project.PropertyGroup.PackageVersion
$expectedPublishAotClangVersion = @($packagesProps.Project.ItemGroup.PackageVersion |
    Where-Object { $_.Include -eq "PublishAotClang" })[0].Version

if ($templateJson.symbols.packageVersion.defaultValue -ne $expectedPackageVersion) {
    throw "Template HarmonyOS.Maui version '$($templateJson.symbols.packageVersion.defaultValue)' does not match Directory.Build.props version '$expectedPackageVersion'."
}
if ($templateJson.symbols.publishAotClangVersion.defaultValue -ne $expectedPublishAotClangVersion) {
    throw "Template PublishAotClang version '$($templateJson.symbols.publishAotClangVersion.defaultValue)' does not match Directory.Packages.props version '$expectedPublishAotClangVersion'."
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
Set-Location $WorkDir
dotnet new harmony-maui -n TemplateApp --packageVersion $packageVersion
if ($LASTEXITCODE -ne 0) {
    throw "Failed to instantiate harmony-maui template."
}

@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local-feed" value="$FeedDir" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -LiteralPath (Join-Path $appDir "nuget.config")

Set-Location $repoRoot
dotnet build (Join-Path $appDir "TemplateApp.csproj") -t:HarmonyStageHost -c Release
if ($LASTEXITCODE -ne 0) {
    throw "Template app build failed."
}

$stagedHost = Join-Path $appDir "obj\harmony\host\entry\src\main\module.json5"
if (-not (Test-Path -LiteralPath $stagedHost)) {
    throw "Staged host not found: $stagedHost"
}

if ($Full) {
    dotnet build (Join-Path $appDir "TemplateApp.csproj") -t:HarmonyBuildLibApp -c Release
    if ($LASTEXITCODE -ne 0) {
        throw "Template app NativeAOT library build failed."
    }

    dotnet build (Join-Path $appDir "TemplateApp.csproj") -t:HarmonyBuildHap -c Release
    if ($LASTEXITCODE -ne 0) {
        throw "Template app HAP build failed."
    }
}

Write-Host "Template consumer verification OK."
