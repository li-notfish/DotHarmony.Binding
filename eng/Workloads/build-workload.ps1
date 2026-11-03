param(
    # 输出目录（workload 安装布局 + nupkgs 本地 feed）。默认仓库 tmp/ 下的临时产物。
    [string]$OutputDir = "",
    # manifest 版本（= SDK pack 版本）。固定值，不随机（P1-7 要求 manifest 版本固定）。
    [string]$ManifestVersion = "",
    # HarmonyOS pack（App.Ref / App.Runtime）版本。
    [string]$PackVersion = "",
    # 可选 ILCompiler pack 声明版本（须与本机 SDK 的 MicrosoftNETCoreAppRuntimeVersion 对齐，
    # 默认自动从 SDK BundledVersions 读取）。
    [string]$ILCompilerPackVersion = "",
    # manifest 安装 band。.NET 10 SDK 实测从 10.0.100 band 读取 manifests。
    [string]$SdkBand = "10.0.100",
    [switch]$SkipAssemblyStaging
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))
if (-not $OutputDir) {
    $OutputDir = Join-Path $repoRoot "tmp\workload"
}
$resolvedOutputDir = [IO.Path]::GetFullPath($OutputDir).TrimEnd('\', '/')
$resolvedTmpRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot "tmp")).TrimEnd('\', '/')
if (-not $resolvedOutputDir.StartsWith(
        $resolvedTmpRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must be inside '$resolvedTmpRoot'."
}

if (-not $PackVersion) {
    [xml]$buildProps = Get-Content -LiteralPath (Join-Path $repoRoot "Directory.Build.props") -Raw
    $PackVersion = $buildProps.SelectSingleNode("/Project/PropertyGroup/PackageVersion").InnerText.Trim()
}
if (-not $ManifestVersion) {
    $ManifestVersion = $PackVersion
}

if (-not $ILCompilerPackVersion) {
    $bundledVersions = $null
    $sdkRoot = Join-Path $env:ProgramFiles "dotnet\sdk"
    if (Test-Path -LiteralPath $sdkRoot) {
        $candidates = @(Get-ChildItem -LiteralPath $sdkRoot -Directory | ForEach-Object {
            Join-Path $_.FullName "Microsoft.NETCoreSdk.BundledVersions.props"
        } | Where-Object { Test-Path -LiteralPath $_ })
        if ($candidates.Count -gt 0) {
            # 按版本号降序（字符串排序会把 9.x 排在 10.x 前面）。
            $bundledVersions = $candidates |
                Sort-Object { [version](Split-Path -Path (Split-Path -Path $_ -Parent) -Leaf) } -Descending |
                Select-Object -First 1
        }
    }
    if ($bundledVersions -and (Test-Path -LiteralPath $bundledVersions)) {
        $match = Select-String -LiteralPath $bundledVersions `
            -Pattern 'ILCompilerPackVersion="(\d+\.\d+\.\d+)"' |
            Where-Object { $_.LineNumber -lt 130 } | Select-Object -First 1
        if ($match) {
            $ILCompilerPackVersion = $match.Matches[0].Groups[1].Value
        }
    }
    if (-not $ILCompilerPackVersion) {
        throw "Could not determine ILCompilerPackVersion from the installed SDK; pass -ILCompilerPackVersion."
    }
}

Write-Host "=== building harmonyos workload: manifest=$ManifestVersion pack=$PackVersion ilc=$ILCompilerPackVersion band=$SdkBand"

$manifestSourceDir = Join-Path $repoRoot "src\workloads\harmonyos"
$sdkPackSourceDir = Join-Path $manifestSourceDir "Microsoft.NET.Sdk.HarmonyOS"

$stagingRoot = $resolvedOutputDir
$manifestDir = Join-Path $stagingRoot "manifests\$SdkBand\microsoft.net.sdk.harmonyos\$ManifestVersion"
$packsDir = Join-Path $stagingRoot "packs"
$feedDir = Join-Path $stagingRoot "nupkgs"

foreach ($dir in @($manifestDir, $packsDir, $feedDir)) {
    if (Test-Path -LiteralPath $dir) {
        Remove-Item -LiteralPath $dir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
}

function Copy-StampedFile {
    param([string]$Source, [string]$Destination)
    $content = Get-Content -LiteralPath $Source -Raw
    $content = $content.Replace("__WORKLOAD_MANIFEST_VERSION__", $ManifestVersion)
    $content = $content.Replace("__HARMONYOS_PACK_VERSION__", $PackVersion)
    $content = $content.Replace("__ILCOMPILER_PACK_VERSION__", $ILCompilerPackVersion)
    Set-Content -LiteralPath $Destination -Value $content -Encoding utf8NoBOM -NoNewline
}

# 1/4：manifest（P1-1 / P1-2）
Copy-StampedFile -Source (Join-Path $manifestSourceDir "WorkloadManifest.in.json") `
                 -Destination (Join-Path $manifestDir "WorkloadManifest.json")
Copy-StampedFile -Source (Join-Path $manifestSourceDir "WorkloadManifest.targets") `
                 -Destination (Join-Path $manifestDir "WorkloadManifest.targets")

# 2/4：SDK pack（P1-2）
$sdkPackDir = Join-Path $packsDir "Microsoft.NET.Sdk.HarmonyOS\$ManifestVersion"
New-Item -ItemType Directory -Path (Join-Path $sdkPackDir "Sdk") -Force | Out-Null
Copy-Item -Path (Join-Path $sdkPackSourceDir "Sdk\*") -Destination (Join-Path $sdkPackDir "Sdk") -Recurse -Force

# 3/4：Assembly staging -> App.Ref / App.Runtime packs（P1-4）
$runtimePackages = @(
    @{ Project = "src\HarmonyOS.Interop\HarmonyOS.Interop.csproj"; PackageId = "HarmonyOS.Interop" },
    @{ Project = "src\HarmonyOS.Bindings\HarmonyOS.Bindings.csproj"; PackageId = "HarmonyOS.Bindings" },
    @{ Project = "src\HarmonyOS.Essentials\HarmonyOS.Essentials.csproj"; PackageId = "HarmonyOS.Essentials" },
    @{ Project = "src\HarmonyOS.Maui\HarmonyOS.Maui.csproj"; PackageId = "HarmonyOS.Maui" },
    @{ Project = "src\HarmonyOS.Maui.Web\HarmonyOS.Maui.Web.csproj"; PackageId = "HarmonyOS.Maui.Web" },
    @{ Project = "src\HarmonyOS.Maui.Blazor\HarmonyOS.Maui.Blazor.csproj"; PackageId = "HarmonyOS.Maui.Blazor" },
    @{ Project = "src\HarmonyOS.Maui.Permissions.Abstractions\HarmonyOS.Maui.Permissions.Abstractions.csproj"; PackageId = "HarmonyOS.Maui.Permissions.Abstractions"; PlatformTfm = $false }
)
$platformTfm = "net10.0-harmonyos26.0"
$rids = @("linux-musl-arm64", "linux-musl-x64")

if (-not $SkipAssemblyStaging) {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $stagingDlls = @{}   # packageId -> list of (tfmDir, dllPath)
    $tempPackageDir = Join-Path $stagingRoot "temp-packages"
    if (Test-Path -LiteralPath $tempPackageDir) {
        Remove-Item -LiteralPath $tempPackageDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $tempPackageDir -Force | Out-Null

    foreach ($package in $runtimePackages) {
        $projectPath = Join-Path $repoRoot $package.Project
        dotnet pack $projectPath -c Release -o $tempPackageDir --nologo "-p:PackageVersion=$PackVersion"
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to pack $($package.Project)."
        }
        $nupkg = Get-ChildItem -LiteralPath $tempPackageDir -Filter "$($package.PackageId).$PackVersion.nupkg" -File |
            Select-Object -First 1
        if ($null -eq $nupkg) {
            throw "Package not found for $($package.PackageId)."
        }
        # 平台包 nupkg 同时进入本地 feed（verify-workload 的模板工程 restore 需要）。
        Copy-Item -LiteralPath $nupkg.FullName -Destination $feedDir -Force
        $archive = [IO.Compression.ZipFile]::OpenRead($nupkg.FullName)
        try {
            $libEntries = @($archive.Entries | Where-Object { $_.FullName -like "lib/*.dll" })
            $perPackage = @()
            foreach ($entry in $libEntries) {
                $tfmDir = ($entry.FullName -split "/")[1]
                $extractPath = Join-Path $tempPackageDir "$($package.PackageId)\$tfmDir\$([IO.Path]::GetFileName($entry.FullName))"
                New-Item -ItemType Directory -Path (Split-Path -Parent $extractPath) -Force | Out-Null
                [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $extractPath, $true)
                $perPackage += [pscustomobject]@{ TfmDir = $tfmDir; DllPath = $extractPath }
            }
            $stagingDlls[$package.PackageId] = $perPackage
        }
        finally {
            $archive.Dispose()
        }
    }

    # Microsoft.HarmonyOS.App.Ref（targeting pack）：ref + RuntimeIdentifierGraph（P1-4）
    $appRefDir = Join-Path $packsDir "Microsoft.HarmonyOS.App.Ref\$PackVersion"
    $appRefLibDir = Join-Path $appRefDir "ref\$platformTfm"
    New-Item -ItemType Directory -Path $appRefLibDir -Force | Out-Null
    foreach ($packageId in $stagingDlls.Keys) {
        foreach ($dll in $stagingDlls[$packageId]) {
            Copy-Item -LiteralPath $dll.DllPath -Destination $appRefLibDir -Force
        }
    }
    $ridGraph = [ordered]@{
        "`$schema" = "https://aka.ms/rid-graph"
        runtimes  = [ordered]@{
            "harmonyos-arm64" = [ordered]@{ "#import" = @("linux-musl-arm64") }
            "harmonyos-x64"   = [ordered]@{ "#import" = @("linux-musl-x64") }
        }
    }
    $ridGraph | ConvertTo-Json -Depth 5 |
        Set-Content -LiteralPath (Join-Path $appRefDir "RuntimeIdentifierGraph.json") -Encoding utf8NoBOM

    # Microsoft.HarmonyOS.App.Runtime.NativeAOT.<rid>（runtime pack）：实现程序集（P1-4）
    foreach ($rid in $rids) {
        $runtimePackDir = Join-Path $packsDir "Microsoft.HarmonyOS.App.Runtime.NativeAOT.$rid\$PackVersion"
        $runtimeLibDir = Join-Path $runtimePackDir "runtimes\$rid\lib\$platformTfm"
        New-Item -ItemType Directory -Path $runtimeLibDir -Force | Out-Null
        foreach ($packageId in $stagingDlls.Keys) {
            foreach ($dll in $stagingDlls[$packageId]) {
                Copy-Item -LiteralPath $dll.DllPath -Destination $runtimeLibDir -Force
            }
        }
    }

    # 4/4：pack 产物 nupkg（独立工程可 restore，P1-4 验收）
    function New-PackNupkg {
        param([string]$PackId, [string]$ContentRoot, [string]$NuspecDescription)
        $nuspecPath = Join-Path $ContentRoot "$PackId.nuspec"
        @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
  <metadata>
    <id>$PackId</id>
    <version>$PackVersion</version>
    <authors>ArkTsBinding</authors>
    <description>$NuspecDescription</description>
  </metadata>
</package>
"@ | Set-Content -LiteralPath $nuspecPath -Encoding utf8NoBOM
        $nupkgPath = Join-Path $feedDir "$PackId.$PackVersion.nupkg"
        if (Test-Path -LiteralPath $nupkgPath) {
            Remove-Item -LiteralPath $nupkgPath -Force
        }
        [IO.Compression.ZipFile]::CreateFromDirectory($ContentRoot, $nupkgPath)
    }

    New-PackNupkg -PackId "Microsoft.HarmonyOS.App.Ref" -ContentRoot $appRefDir `
        -NuspecDescription "HarmonyOS managed targeting pack (ref assemblies + RuntimeIdentifierGraph)."
    foreach ($rid in $rids) {
        $runtimePackDir = Join-Path $packsDir "Microsoft.HarmonyOS.App.Runtime.NativeAOT.$rid\$PackVersion"
        New-PackNupkg -PackId "Microsoft.HarmonyOS.App.Runtime.NativeAOT.$rid" -ContentRoot $runtimePackDir `
            -NuspecDescription "HarmonyOS NativeAOT runtime pack ($rid): HarmonyOS-owned implementation assemblies."
    }

    Remove-Item -LiteralPath $tempPackageDir -Recurse -Force
}

Write-Host "=== workload staging complete:"
Write-Host "    manifest  : $manifestDir"
Write-Host "    sdk pack  : $sdkPackDir"
if (-not $SkipAssemblyStaging) {
    Write-Host "    app ref   : $(Join-Path $packsDir "Microsoft.HarmonyOS.App.Ref\$PackVersion")"
    Write-Host "    runtimes  : $(Join-Path $packsDir "Microsoft.HarmonyOS.App.Runtime.NativeAOT.*\$PackVersion")"
}
Write-Host "    nupkg feed: $feedDir"
