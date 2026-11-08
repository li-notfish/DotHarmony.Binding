param(
    # 输出目录（隔离 SDK、本地 feed、模板探针工程）。默认仓库 tmp\verify-workload。
    [string]$OutputDir = "",
    # 隔离 SDK 目录；默认 $OutputDir\dotnet。已存在则复用（幂等）。
    [string]$DotNetInstallDir = "",
    # 默认复制全局 SDK 为隔离副本（快、离线可用）；-DownloadSdk 时改用 dotnet-install 下载。
    [switch]$DownloadSdk,
    # 下载模式使用的 SDK 版本；默认取当前全局版本（保证 manifest band 匹配）。
    [string]$DotNetVersion = "",
    # workload manifest 安装 band（与 build-workload.ps1 保持一致）。
    [string]$SdkBand = "10.0.100",
    # 成功后保留输出目录（失败时总是保留，供失败现场检查）。
    [switch]$KeepOutput
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $OutputDir) {
    $OutputDir = Join-Path $repoRoot "tmp\verify-workload"
}
$resolvedOutputDir = [IO.Path]::GetFullPath($OutputDir).TrimEnd('\', '/')
$resolvedTmpRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot "tmp")).TrimEnd('\', '/')
if (-not $resolvedOutputDir.StartsWith(
        $resolvedTmpRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "OutputDir must be inside '$resolvedTmpRoot'."
}

New-Item -ItemType Directory -Path $resolvedOutputDir -Force | Out-Null
$feedDir = Join-Path $resolvedOutputDir "feed"
$stagingDir = Join-Path $resolvedOutputDir "workload"
if (Test-Path -LiteralPath $feedDir) { Remove-Item -LiteralPath $feedDir -Recurse -Force }
if (Test-Path -LiteralPath $stagingDir) { Remove-Item -LiteralPath $stagingDir -Recurse -Force }
# cli-home（模板引擎缓存）随环境一起重建，避免上次失败遗留的模板包导致安装冲突。
$cliHomeDir = Join-Path $resolvedOutputDir "cli-home"
if (Test-Path -LiteralPath $cliHomeDir) { Remove-Item -LiteralPath $cliHomeDir -Recurse -Force }
New-Item -ItemType Directory -Path $feedDir -Force | Out-Null
New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null

function Invoke-IsolatedDotnet {
    param([string]$DotNetInstallDir, [string[]]$DotnetArgs, [string]$WorkingDirectory)
    $env:DOTNET_ROOT = $DotNetInstallDir
    $env:DOTNET_MULTILEVEL_LOOKUP = "0"
    $env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
    $env:DOTNET_NOLOGO = "1"
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
    $env:DOTNET_GENERATE_ASPNET_CERTIFICATE = "0"
    $env:DOTNET_CLI_HOME = Join-Path $resolvedOutputDir "cli-home"
    $env:NUGET_PACKAGES = Join-Path $resolvedOutputDir "nuget-packages"
    # NuGet http 缓存也隔离，避免宿主缓存文件被占用/ACL 异常导致的间歇性还原失败。
    $env:NUGET_HTTP_CACHE_PATH = Join-Path $resolvedOutputDir "nuget-http-cache"
    # 隔离临时目录，避免宿主 %TEMP% 中的残留文件/杀软锁导致临时文件写入失败。
    $isolatedTemp = Join-Path $resolvedOutputDir "temp"
    New-Item -ItemType Directory -Path $isolatedTemp -Force | Out-Null
    $env:TMP = $isolatedTemp
    $env:TEMP = $isolatedTemp
    # 本地 manifest 无需联网校验更新；禁用后跳过 manifest update check。
    $env:DOTNET_CLI_WORKLOAD_UPDATE_CHECK_DISABLE = "1"
    Push-Location
    try {
        if ($WorkingDirectory) { Set-Location -LiteralPath $WorkingDirectory }
        & (Join-Path $DotNetInstallDir "dotnet.exe") @DotnetArgs
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet $($DotnetArgs -join ' ') failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

try {
    # 1/6：workload 布局 + 本地 feed（平台包 / App.Ref / runtime pack / 模板包）
    & (Join-Path $repoRoot "eng\Workloads\build-workload.ps1") `
        -OutputDir $stagingDir -SdkBand $SdkBand
    if ($LASTEXITCODE -ne 0) {
        throw "build-workload.ps1 failed."
    }
    # build-workload 的平台包/App.Ref/runtime pack nupkg 落在 workload\nupkgs，
    # 统一搬运到探针 restore 使用的本地 feed。
    Copy-Item -Path (Join-Path $stagingDir "nupkgs\*") -Destination $feedDir -Recurse -Force

    [xml]$buildProps = Get-Content -LiteralPath (Join-Path $repoRoot "Directory.Build.props") -Raw
    $packageVersion = $buildProps.SelectSingleNode("/Project/PropertyGroup/PackageVersion").InnerText.Trim()
    $manifestVersion = $packageVersion
    dotnet pack (Join-Path $repoRoot "src\HarmonyOS.Templates\HarmonyOS.Templates.csproj") `
        -c Release -o $feedDir --nologo "-p:PackageVersion=$packageVersion"
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to pack HarmonyOS.Templates."
    }

    # 2/6：隔离 SDK（不改 %ProgramFiles%\dotnet 下的用户全局 SDK）
    if (-not $DotNetInstallDir) {
        $DotNetInstallDir = Join-Path $resolvedOutputDir "dotnet"
    }
    $resolvedInstallDir = [IO.Path]::GetFullPath($DotNetInstallDir).TrimEnd('\', '/')
    if (-not (Test-Path (Join-Path $resolvedInstallDir "dotnet.exe"))) {
        if ($DownloadSdk) {
            if (-not $DotNetVersion) {
                $DotNetVersion = (& dotnet --version).Trim()
            }
            $installScript = Join-Path $resolvedOutputDir "dotnet-install.ps1"
            Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -Version $DotNetVersion -InstallDir $resolvedInstallDir -NoPath
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet-install failed."
            }
        }
        else {
            $dotnetSource = (Get-Command dotnet).Source
            $globalDotnetRoot = Split-Path -Parent $dotnetSource
            if (-not (Test-Path -LiteralPath (Join-Path $globalDotnetRoot "dotnet.exe"))) {
                throw "Resolved dotnet root '$globalDotnetRoot' does not contain dotnet.exe (source: '$dotnetSource')."
            }
            Write-Host "=== copying global SDK '$globalDotnetRoot' -> isolated '$resolvedInstallDir'"
            robocopy $globalDotnetRoot $resolvedInstallDir /E /NFL /NDL /NJH /NJS /NP /R:2 /W:2 | Out-Null
            if ($LASTEXITCODE -ge 8) {
                throw "robocopy failed with exit code $LASTEXITCODE."
            }
        }
    }

    # 3/6：安装 workload（manifest -> sdk-manifests\<band>\<id>\<version>；SDK pack -> packs\）
    # band 必须用隔离 SDK 自身的 feature band（如 10.0.403 -> 10.0.400）；
    # resolver 扫描该 band 目录，其余 band 的官方 manifest 仅通过 fallback 命中。
    $isolatedSdkVersion = (& (Join-Path $resolvedInstallDir "dotnet.exe") --version).Trim()
    if ($isolatedSdkVersion -notmatch '^(\d+)\.(\d+)\.(\d+)') {
        throw "Cannot parse isolated SDK version '$isolatedSdkVersion'."
    }
    $featureBandPatch = ([Math]::Floor([int]$Matches[3] / 100) * 100).ToString()
    $manifestBand = "$($Matches[1]).$($Matches[2]).$featureBandPatch"
    $stagingManifestDir = Join-Path $stagingDir "manifests\$SdkBand\microsoft.net.sdk.harmonyos\$manifestVersion"
    $stagingSdkPackDir = Join-Path $stagingDir "packs\Microsoft.NET.Sdk.HarmonyOS\$manifestVersion"
    foreach ($path in @($stagingManifestDir, $stagingSdkPackDir)) {
        if (-not (Test-Path -LiteralPath $path)) {
            throw "Workload staging missing: $path"
        }
    }
    $sdkManifestRoot = Join-Path $resolvedInstallDir "sdk-manifests\$manifestBand\microsoft.net.sdk.harmonyos\$manifestVersion"
    New-Item -ItemType Directory -Path $sdkManifestRoot -Force | Out-Null
    Copy-Item -Path (Join-Path $stagingManifestDir "*") -Destination $sdkManifestRoot -Recurse -Force
    # 复制 pack ID 层目录（而非版本子目录），保证 packs\<PackId>\<version>\Sdk\ 布局。
    Copy-Item -Path (Split-Path -Parent $stagingSdkPackDir) -Destination (Join-Path $resolvedInstallDir "packs") -Recurse -Force

    # 4/6：workload list 可见（软检查）。
    # 本机 CLI 的 workload 命令可能因环境问题崩溃（InstallerBase type-init / DTF 临时文件，
    # 全局 SDK 同样复现，与 workload 布局无关）；此时改用 MSBuild 属性探针做硬校验。
    $workloadListSkipped = $false
    try {
        $workloadList = Invoke-IsolatedDotnet -DotNetInstallDir $resolvedInstallDir -DotnetArgs @("workload", "list")
        $workloadListText = ($workloadList | Out-String)
        Write-Host $workloadListText
        if ($workloadListText -notmatch '(?m)^\s*harmonyos(\s|$)') {
            throw "Workload 'harmonyos' not visible in 'dotnet workload list' (band=$SdkBand). " `
                + "Isolated SDK: $resolvedInstallDir; manifest snapshot: $sdkManifestRoot"
        }
    }
    catch {
        $message = $_.Exception.Message
        if ($message -notmatch 'InstallerBase|dotnet workload list failed with exit code') {
            throw
        }
        $workloadListSkipped = $true
        Write-Host "WARNING: 'dotnet workload list' is broken on this machine (CLI-level issue): $message"
        Write-Host "         Continuing with MSBuild property probes as the functional workload check."
    }

    # 5/6：模板探针工程（仓库外目录，无内联平台片段）
    $probeDir = Join-Path $resolvedOutputDir "probe"
    if (Test-Path -LiteralPath $probeDir) { Remove-Item -LiteralPath $probeDir -Recurse -Force }
    $templatePkg = Get-ChildItem -LiteralPath $feedDir -Filter "HarmonyOS.Templates.$packageVersion.nupkg" -File |
        Select-Object -First 1
    if ($null -eq $templatePkg) {
        throw "Template package not found in feed: $feedDir"
    }
    Invoke-IsolatedDotnet -DotNetInstallDir $resolvedInstallDir -DotnetArgs @("new", "install", $templatePkg.FullName)
    Invoke-IsolatedDotnet -DotNetInstallDir $resolvedInstallDir `
        -DotnetArgs @("new", "harmony-maui", "-n", "WorkloadProbe", "-o", $probeDir)

    $probeCsproj = Get-ChildItem -LiteralPath $probeDir -Filter "*.csproj" -File | Select-Object -First 1
    if ($null -eq $probeCsproj) {
        throw "Probe project not created under $probeDir."
    }
    # 只匹配真实 XML 元素；注释文字中的属性名不算内联片段。
    $inlineSnippet = @(Select-String -LiteralPath $probeCsproj.FullName -Pattern "<TargetPlatformSupported|<SdkSupportedTargetPlatformVersion")
    if ($inlineSnippet.Count -gt 0) {
        throw "Probe project still contains the inline platform registration snippet."
    }

    # 4.5/6：MSBuild 属性探针（硬校验）：证明 workload manifest targets 与 SDK pack 真实生效。
    $installedValue = (Invoke-IsolatedDotnet -DotNetInstallDir $resolvedInstallDir `
        -DotnetArgs @("msbuild", $probeCsproj.FullName, "-getProperty:_HarmonyOSWorkloadInstalled")) | Out-String
    if ($installedValue.Trim() -ne "true") {
        throw "Workload SDK pack AutoImport not active (_HarmonyOSWorkloadInstalled='$($installedValue.Trim())')."
    }
    $activeValue = (Invoke-IsolatedDotnet -DotNetInstallDir $resolvedInstallDir `
        -DotnetArgs @("msbuild", $probeCsproj.FullName, "-getProperty:HarmonyOSWorkloadActive")) | Out-String
    if ($activeValue.Trim() -ne "true") {
        throw "Workload manifest targets not imported (HarmonyOSWorkloadActive='$($activeValue.Trim())')."
    }
    Write-Host "Workload MSBuild probes OK (AutoImport + manifest targets active)."

    $probeNuGetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="globalPackagesFolder" value="$([Security.SecurityElement]::Escape((Join-Path $resolvedOutputDir 'nuget-packages')))" />
  </config>
  <packageSources>
    <clear />
    <add key="local" value="$([Security.SecurityElement]::Escape($feedDir))" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
    $probeNuGetConfig | Set-Content -LiteralPath (Join-Path $probeDir "NuGet.Config") -Encoding utf8

    Invoke-IsolatedDotnet -DotNetInstallDir $resolvedInstallDir `
        -DotnetArgs @("restore", $probeCsproj.FullName,
            "--configfile", (Join-Path $probeDir "NuGet.Config"),
            "-p:ImportDirectoryBuildProps=false", "-p:ImportDirectoryBuildTargets=false", "--nologo")

    # 6/6：无设备 build（编译过 = workload 装好即可构建；HAP 打包需 HarmonyOS SDK，另行设备 gate）
    Invoke-IsolatedDotnet -DotNetInstallDir $resolvedInstallDir `
        -DotnetArgs @("build", $probeCsproj.FullName, "--no-restore",
            "-p:ImportDirectoryBuildProps=false", "-p:ImportDirectoryBuildTargets=false", "--nologo")

    Write-Host "Workload verification OK."
    if (-not $KeepOutput) {
        try {
            Remove-Item -LiteralPath $resolvedOutputDir -Recurse -Force
        }
        catch {
            Start-Sleep -Seconds 3
            try {
                Remove-Item -LiteralPath $resolvedOutputDir -Recurse -Force
            }
            catch {
                # 验证已通过；清理被宿主文件锁/杀软拦截只降级为警告。
                Write-Host "WARNING: cleanup failed ($($_.Exception.Message)); output kept at $resolvedOutputDir"
            }
        }
    }
}
catch {
    Write-Host $_.ToString()
    Write-Host ""
    Write-Host "=== FAILED. Preserved state for inspection:"
    Write-Host "    output dir      : $resolvedOutputDir"
    if ($resolvedInstallDir) {
        Write-Host "    isolated SDK    : $resolvedInstallDir"
    }
    if ($sdkManifestRoot) {
        Write-Host "    manifest snapshot: $sdkManifestRoot"
        Write-Host "    manifest json    : $(Join-Path $sdkManifestRoot 'WorkloadManifest.json')"
    }
    exit 1
}
