# 本地编排：打包源码 → 上传远程 → 构建 libapp.so → 取回放入 HarmonyHost/libs
$ErrorActionPreference = "Stop"

# 切换到脚本所在目录的上级（项目根目录）
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
Set-Location $projectRoot

# 在项目根目录下创建 tmp 目录
$tmpDir = Join-Path $projectRoot "tmp"
New-Item -ItemType Directory -Force -Path $tmpDir | Out-Null

# 从环境变量读取配置，若未设置则使用默认值
$REMOTE = if ($env:REMOTE) { $env:REMOTE } else { "wsl_auzrelinux" }
$BUILD  = if ($env:BUILD)  { $env:BUILD  } else { "/tmp/arktsbinding" }

Write-Host "=== 1. 打包源码 ==="
$tempArchive = Join-Path $tmpDir "arkts-src.tgz"

$tarExcludes = @(
    "--exclude=node_modules",
    "--exclude=.git",
    "--exclude=dist",
    "--exclude=coverage",
    "--exclude=bin",
    "--exclude=obj",
    "--exclude=output",
    "--exclude=UnityHarmony",
    "--exclude=tmp"
)
$filesToTar = @("src", "HarmonyOS.Bindings", "samples", "scripts", "ArkTsBinding.slnx")
& tar czf $tempArchive $tarExcludes $filesToTar
if ($LASTEXITCODE -ne 0) { throw "打包失败" }

Write-Host "=== 2. 上传并解压 ==="
& ssh $REMOTE "rm -rf $BUILD && mkdir -p $BUILD"
if ($LASTEXITCODE -ne 0) { throw "远程创建目录失败" }

& scp -q $tempArchive "$($REMOTE):$BUILD/src.tgz"
if ($LASTEXITCODE -ne 0) { throw "上传源码包失败" }

& ssh $REMOTE "cd $BUILD && tar xzf src.tgz"
if ($LASTEXITCODE -ne 0) { throw "远程解压失败" }

Write-Host "=== 3. 远程构建 ==="
& ssh $REMOTE "bash $BUILD/scripts/build-libapp.sh"
if ($LASTEXITCODE -ne 0) { throw "远程构建失败" }

Write-Host "=== 4. 取回 libapp.so（双架构） ==="
# 创建本地目标目录
New-Item -ItemType Directory -Force -Path "samples/HarmonyHost/entry/libs/arm64-v8a" | Out-Null
New-Item -ItemType Directory -Force -Path "samples/HarmonyHost/entry/libs/x86_64"   | Out-Null

& scp -q "$($REMOTE):$BUILD/samples/dotnet/HelloApp/bin/Release/net10.0/linux-musl-arm64/publish/app.so" `
       "samples/HarmonyHost/entry/libs/arm64-v8a/libapp.so"
if ($LASTEXITCODE -ne 0) { throw "取回 arm64 架构文件失败" }

& scp -q "$($REMOTE):$BUILD/samples/dotnet/HelloApp/bin/Release/net10.0/linux-musl-x64/publish/app.so" `
       "samples/HarmonyHost/entry/libs/x86_64/libapp.so"
if ($LASTEXITCODE -ne 0) { throw "取回 x86_64 架构文件失败" }

# 显示取回的文件信息
Get-ChildItem -Recurse "samples/HarmonyHost/entry/libs/arm64-v8a/libapp.so", "samples/HarmonyHost/entry/libs/x86_64/libapp.so" | Format-Table -AutoSize

Write-Host "=== 完成 ==="