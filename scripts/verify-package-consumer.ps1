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
    $WorkDir = Join-Path $env:TEMP "harmony-package-consumer"
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
    "src\HarmonyOS.Maui\HarmonyOS.Maui.csproj"
)

$packageVersion = "1.0.$(Get-Random -Minimum 100000 -Maximum 999999)-local"

foreach ($project in $projects) {
    dotnet pack (Join-Path $repoRoot $project) -c Release -o $FeedDir -p:PackageVersion=$packageVersion
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to pack $project"
    }
}

$consumerProject = Join-Path $WorkDir "Consumer.csproj"
$sourceFile = Join-Path $WorkDir "Consumer.cs"
$xamlFile = Join-Path $WorkDir "MainPage.xaml"
$customMappingFile = Join-Path $WorkDir "harmony-permissions.custom.json"
$nugetConfig = Join-Path $WorkDir "nuget.config"

@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="HarmonyOS.Maui" Version="PACKAGE_VERSION" />
    <MauiXaml Include="MainPage.xaml" />
  </ItemGroup>
</Project>
'@ -replace 'PACKAGE_VERSION', $packageVersion | Set-Content -LiteralPath $consumerProject

@'
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;

public static class Consumer
{
    public static async Task RunAsync()
    {
        Vibration.Default.Vibrate();
        _ = Contacts.Default.GetAllAsync();
    }

    public static void OnLocationClicked(object sender, EventArgs e)
    {
        _ = Geolocation.Default.GetLocationAsync();
    }
}
'@ | Set-Content -LiteralPath $sourceFile

@'
<?xml version="1.0" encoding="utf-8"?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
  <Button Clicked="OnLocationClicked" />
</ContentPage>
'@ | Set-Content -LiteralPath $xamlFile

@'
{
  "version": 3,
  "mauiMethods": [
    {
      "containingType": "Microsoft.Maui.ApplicationModel.Communication.IContacts",
      "methodName": "GetAllAsync",
      "permission": "ohos.permission.READ_CONTACTS",
      "when": "inuse"
    },
    {
      "containingType": "Microsoft.Maui.ApplicationModel.Communication.Contacts",
      "methodName": "GetAllAsync",
      "permission": "ohos.permission.READ_CONTACTS",
      "when": "inuse"
    }
  ]
}
'@ | Set-Content -LiteralPath $customMappingFile

@'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="local-feed" value="FEED_DIR" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
'@ -replace 'FEED_DIR', $FeedDir | Set-Content -LiteralPath $nugetConfig

dotnet build $consumerProject -t:HarmonyPermissionCheck -c Release
if ($LASTEXITCODE -ne 0) {
    throw "Consumer build failed."
}

$permissionsJson = Join-Path $WorkDir "obj\harmony\permissions.json"
$moduleJson = Join-Path $WorkDir "obj\harmony\host\entry\src\main\module.json5"
$report = Join-Path $WorkDir "obj\harmony\permissions.report.md"

foreach ($file in @($permissionsJson, $moduleJson, $report)) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "Expected generated file not found: $file"
    }
}

$permissions = Get-Content -LiteralPath $permissionsJson -Raw | ConvertFrom-Json
$permissionNames = @($permissions.permissions | ForEach-Object { $_.name })
if (-not ($permissionNames -contains "ohos.permission.LOCATION")) {
    throw "Consumer did not infer ohos.permission.LOCATION."
}
if (-not ($permissionNames -contains "ohos.permission.VIBRATE")) {
    throw "Consumer did not infer ohos.permission.VIBRATE."
}
if (-not ($permissionNames -contains "ohos.permission.READ_CONTACTS")) {
    throw "Consumer did not infer ohos.permission.READ_CONTACTS from the custom mapping."
}

$location = @($permissions.permissions | Where-Object { $_.name -eq "ohos.permission.LOCATION" })[0]
if ($null -eq $location -or $location.sourcePath -notlike "*MainPage.xaml") {
    throw "Consumer did not trace LOCATION back to MainPage.xaml."
}

Write-Host "Package consumer verification OK."
