param(
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $OutputDir) {
    $OutputDir = Join-Path $repoRoot "tmp\verify-tfm-matrix"
}

$resolvedRepoRoot = [IO.Path]::GetFullPath($repoRoot).TrimEnd('\', '/')
$resolvedOutputDir = [IO.Path]::GetFullPath($OutputDir).TrimEnd('\', '/')
$resolvedTmpRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot "tmp")).TrimEnd('\', '/')
if (-not $resolvedOutputDir.StartsWith(
        $resolvedTmpRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must be inside '$resolvedTmpRoot'."
}

if (Test-Path -LiteralPath $resolvedOutputDir) {
    Remove-Item -LiteralPath $resolvedOutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $resolvedOutputDir -Force | Out-Null

function Get-ProjectProperty {
    param(
        [string]$ProjectPath,
        [string]$PropertyName
    )

    [xml]$project = Get-Content -LiteralPath $ProjectPath -Raw
    $node = $project.SelectSingleNode("/Project/PropertyGroup/$PropertyName")
    if ($null -eq $node) {
        return ""
    }

    return $node.InnerText.Trim()
}

$buildPropsPath = Join-Path $repoRoot "Directory.Build.props"
[xml]$buildProps = Get-Content -LiteralPath $buildPropsPath -Raw
$genericTargetFramework = $buildProps.SelectSingleNode(
    "/Project/PropertyGroup/HarmonyOSTargetFramework").InnerText.Trim()
$platformTargetFramework = $buildProps.SelectSingleNode(
    "/Project/PropertyGroup/HarmonyOSPlatformTargetFramework").InnerText.Trim()
$platformVersion = $buildProps.SelectSingleNode(
    "/Project/PropertyGroup/HarmonyOSPlatformVersion").InnerText.Trim()
$platformTargetFramework = $platformTargetFramework.Replace(
    '$(HarmonyOSPlatformVersion)',
    $platformVersion)
$expectedPlatformTargetFramework = "net10.0-harmonyos$platformVersion"
if ($platformTargetFramework -ne $expectedPlatformTargetFramework) {
    throw "HarmonyOSPlatformTargetFramework must be '$expectedPlatformTargetFramework', but is '$platformTargetFramework'."
}
$expectedRuntimeFrameworks = @($genericTargetFramework, $platformTargetFramework)

$runtimePackages = @(
    [pscustomobject]@{
        Project = "src\HarmonyOS.Interop\HarmonyOS.Interop.csproj"
        PackageId = "HarmonyOS.Interop"
    },
    [pscustomobject]@{
        Project = "src\HarmonyOS.Bindings\HarmonyOS.Bindings.csproj"
        PackageId = "HarmonyOS.Bindings"
    },
    [pscustomobject]@{
        Project = "src\HarmonyOS.Essentials\HarmonyOS.Essentials.csproj"
        PackageId = "HarmonyOS.Essentials"
    },
    [pscustomobject]@{
        Project = "src\HarmonyOS.Maui\HarmonyOS.Maui.csproj"
        PackageId = "HarmonyOS.Maui"
    },
    [pscustomobject]@{
        Project = "src\HarmonyOS.Maui.Web\HarmonyOS.Maui.Web.csproj"
        PackageId = "HarmonyOS.Maui.Web"
    },
    [pscustomobject]@{
        Project = "src\HarmonyOS.Maui.Blazor\HarmonyOS.Maui.Blazor.csproj"
        PackageId = "HarmonyOS.Maui.Blazor"
    }
)

$platformNeutralProjects = @(
    [pscustomobject]@{
        Project = "src\HarmonyOS.Maui.Permissions.Abstractions\HarmonyOS.Maui.Permissions.Abstractions.csproj"
        ExpectedFramework = "netstandard2.0"
    },
    [pscustomobject]@{
        Project = "src\HarmonyOS.Maui.Permissions\HarmonyOS.Maui.Permissions.csproj"
        ExpectedFramework = "netstandard2.0"
    },
    [pscustomobject]@{
        Project = "src\HarmonyOS.Templates\HarmonyOS.Templates.csproj"
        ExpectedFramework = "net10.0"
    },
    [pscustomobject]@{
        Project = "tools\HandlerAudit\HandlerAudit.csproj"
        ExpectedFramework = "net10.0"
    }
)

foreach ($package in $runtimePackages) {
    $projectPath = Join-Path $repoRoot $package.Project
    $targetFrameworksText = Get-ProjectProperty `
            -ProjectPath $projectPath `
            -PropertyName "TargetFrameworks"
    $targetFrameworksText = $targetFrameworksText.Replace(
        '$(HarmonyOSTargetFramework)',
        $genericTargetFramework)
    $targetFrameworksText = $targetFrameworksText.Replace(
        '$(HarmonyOSPlatformTargetFramework)',
        $platformTargetFramework)
    $targetFrameworksText = $targetFrameworksText.Trim()
    if (-not $targetFrameworksText) {
        $targetFrameworksText = Get-ProjectProperty `
            -ProjectPath $projectPath `
            -PropertyName "TargetFramework"
    }
    $targetFrameworks = @($targetFrameworksText -split ';' | Where-Object { $_ })

    if ($targetFrameworks.Count -ne $expectedRuntimeFrameworks.Count -or
        (Compare-Object $targetFrameworks $expectedRuntimeFrameworks)) {
        throw "$($package.Project) must target $($expectedRuntimeFrameworks -join ';'), but targets $($targetFrameworks -join ';')."
    }
}

foreach ($project in $platformNeutralProjects) {
    $projectPath = Join-Path $repoRoot $project.Project
    $targetFramework = Get-ProjectProperty `
        -ProjectPath $projectPath `
        -PropertyName "TargetFramework"
    $targetFramework = $targetFramework.Replace(
        '$(HarmonyOSTargetFramework)',
        $genericTargetFramework)
    $targetFramework = $targetFramework.Replace(
        '$(HarmonyOSPlatformTargetFramework)',
        $platformTargetFramework)
    $targetFramework = $targetFramework.Trim()
    if ($targetFramework -ne $project.ExpectedFramework) {
        throw "$($project.Project) must stay on $($project.ExpectedFramework), but targets '$targetFramework'."
    }
}

$packageVersion = "1.2.$(Get-Random -Minimum 100000 -Maximum 999999)-local"
$packProjects = @($runtimePackages.Project) + @(
    "src\HarmonyOS.Maui.Permissions.Abstractions\HarmonyOS.Maui.Permissions.Abstractions.csproj"
)

foreach ($project in $packProjects) {
    $projectPath = Join-Path $repoRoot $project
    dotnet pack $projectPath `
        -c Release `
        -o $resolvedOutputDir `
        "-p:PackageVersion=$packageVersion"
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to pack $project"
    }
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($package in $runtimePackages) {
    $packagePath = Get-ChildItem -LiteralPath $resolvedOutputDir `
        -Filter "$($package.PackageId).*.nupkg" `
        -File | Select-Object -First 1
    if ($null -eq $packagePath) {
        throw "Package not found for $($package.PackageId)."
    }

    $archive = [IO.Compression.ZipFile]::OpenRead($packagePath.FullName)
    try {
        $entries = @($archive.Entries | ForEach-Object { $_.FullName })
    }
    finally {
        $archive.Dispose()
    }

    $desktopAsset = "lib/$genericTargetFramework/$($package.PackageId).dll"
    $platformAsset = "lib/$platformTargetFramework/$($package.PackageId).dll"
    foreach ($requiredAsset in @($desktopAsset, $platformAsset)) {
        if ($requiredAsset -notin $entries) {
            throw "$($package.PackageId) is missing '$requiredAsset'."
        }
    }
}

$consumerDir = Join-Path $resolvedOutputDir "consumer"
$packagesDir = Join-Path $consumerDir "packages"
New-Item -ItemType Directory -Path $consumerDir -Force | Out-Null
New-Item -ItemType Directory -Path $packagesDir -Force | Out-Null

@'
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $consumerDir "Directory.Build.props") -Encoding utf8

@'
<Project>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $consumerDir "Directory.Build.targets") -Encoding utf8

@'
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $consumerDir "Directory.Packages.props") -Encoding utf8

$escapedFeedPath = [Security.SecurityElement]::Escape($resolvedOutputDir)
$escapedPackagesPath = [Security.SecurityElement]::Escape($packagesDir)
@"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="globalPackagesFolder" value="$escapedPackagesPath" />
  </config>
  <packageSources>
    <clear />
    <add key="local" value="$escapedFeedPath" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Set-Content -LiteralPath (Join-Path $consumerDir "NuGet.Config") -Encoding utf8

$packageReferences = ($runtimePackages | ForEach-Object {
    "    <PackageReference Include=""$($_.PackageId)"" Version=""$packageVersion"" />"
}) -join "`n"
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$platformTargetFramework</TargetFramework>
    <TargetPlatformSupported>true</TargetPlatformSupported>
    <TargetPlatformVersion>$platformVersion</TargetPlatformVersion>
    <NoWarn>`$(NoWarn);CA1418</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <SdkSupportedTargetPlatformVersion Include="$platformVersion" />
  </ItemGroup>
  <ItemGroup>
$packageReferences
  </ItemGroup>
</Project>
"@ | Set-Content -LiteralPath (Join-Path $consumerDir "Consumer.csproj") -Encoding utf8

@'
namespace Consumer;

public sealed class Marker;
'@ | Set-Content -LiteralPath (Join-Path $consumerDir "Marker.cs") -Encoding utf8

$consumerProject = Join-Path $consumerDir "Consumer.csproj"
dotnet restore $consumerProject `
    --configfile (Join-Path $consumerDir "NuGet.Config") `
    --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Failed to restore the external package consumer."
}

$assetsPath = Join-Path $consumerDir "obj\project.assets.json"
$assets = Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json -Depth 100
$targetProperty = $assets.targets.PSObject.Properties |
    Where-Object { $_.Name -eq $platformTargetFramework } |
    Select-Object -First 1
if ($null -eq $targetProperty) {
    throw "The external consumer assets do not contain '$platformTargetFramework'."
}

$targetAssets = $targetProperty.Value
foreach ($package in $runtimePackages) {
    $packageProperty = $targetAssets.PSObject.Properties |
        Where-Object { $_.Name -like "$($package.PackageId)/*" } |
        Select-Object -First 1
    if ($null -eq $packageProperty) {
        throw "The external consumer assets do not contain $($package.PackageId)."
    }

    $entry = $packageProperty.Value
    $compileAssets = @($entry.compile.PSObject.Properties.Name)
    $runtimeAssets = @($entry.runtime.PSObject.Properties.Name)
    $expectedAsset = "lib/$platformTargetFramework/$($package.PackageId).dll"
    if ($expectedAsset -notin $compileAssets -or $expectedAsset -notin $runtimeAssets) {
        throw "$($package.PackageId) did not select '$expectedAsset' for compile and runtime assets."
    }

    $nonPlatformAssets = @($compileAssets + $runtimeAssets |
        Where-Object { -not $_.StartsWith("lib/$platformTargetFramework/", [StringComparison]::OrdinalIgnoreCase) })
    if ($nonPlatformAssets.Count -gt 0) {
        throw "$($package.PackageId) selected non-platform assets: $($nonPlatformAssets -join ', ')."
    }
}

dotnet build $consumerProject --no-restore --nologo
if ($LASTEXITCODE -ne 0) {
    throw "Failed to build the external package consumer."
}

Remove-Item -LiteralPath $resolvedOutputDir -Recurse -Force
Write-Host "TFM matrix verification OK."
