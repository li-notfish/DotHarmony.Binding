# 模板包验收：pack 产品包 + HarmonyOS.Templates → dotnet new install → 实例化 →
# HarmonyStageHost（验证 buildTransitive targets/宿主模板随包分发）+ 托管编译。
# 版本号每次随机，避免 NuGet 全局缓存命中旧同版本包。
param(
    [string]$FeedDir = "",
    [string]$WorkDir = ""
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

Write-Host "Template consumer verification OK."
