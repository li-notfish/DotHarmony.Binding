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

# 打包清单与 remote-build.sh 共用（scripts/build-files.txt）
$tarArgs = Get-Content (Join-Path $scriptDir "build-files.txt") |
    Where-Object { $_ -notmatch '^\s*(#|$)' }

$DemoApp = if ($env:DEMO_APP) { $env:DEMO_APP } else { "HelloApp" }
Write-Host "=== demo app: $DemoApp ==="

# libapp.so 落地的宿主目录：HOST_DIR 覆盖（targets 生成的按应用暂存宿主），默认共享模板
$HostDir = if ($env:HOST_DIR) { $env:HOST_DIR } else { Join-Path $projectRoot "samples/HarmonyHost" }

# LOCAL 模式——仍走"打包 → 复制进 WSL 原生文件系统 → 构建 → 取回"。
# 不要直接在 /mnt/*（9p 挂载）上构建：海量小文件 I/O 会慢一个数量级以上。
if ($env:LOCAL -eq "true") {
    # wsl.exe 会把参数交给 Linux shell 解析，反斜杠会被当转义符吃掉——必须换成正斜杠
    $wslRoot = (& wsl wslpath -a ($projectRoot -replace '\\', '/')).Trim()
    if ($LASTEXITCODE -ne 0) { throw "wslpath 转换失败" }
    $hostWsl = (& wsl wslpath -a ($HostDir -replace '\\', '/')).Trim()
    if ($LASTEXITCODE -ne 0) { throw "wslpath 转换失败" }

    Write-Host "=== 1. 打包源码 ==="
    # 归档名用相对路径：GNU tar 会把含盘符冒号的路径当远程主机（"Cannot connect to D:"）
    & tar czf "tmp/arkts-src.tgz" @tarArgs
    if ($LASTEXITCODE -ne 0) { throw "打包失败" }

    Write-Host "=== 2. 复制进 WSL 原生文件系统 ($BUILD) ==="
    $wslArchive = "$wslRoot/tmp/arkts-src.tgz"
    & wsl bash -c "rm -rf '$BUILD' && mkdir -p '$BUILD' && tar xzf '$wslArchive' -C '$BUILD'"
    if ($LASTEXITCODE -ne 0) { throw "WSL 解压失败" }

    Write-Host "=== 3. WSL 原生文件系统构建 ==="
    & wsl bash -c "DEMO_APP='$DemoApp' bash '$BUILD/scripts/build-libapp.sh'"
    if ($LASTEXITCODE -ne 0) { throw "本地 WSL 构建失败" }

    Write-Host "=== 4. 取回 libapp.so（双架构）→ $HostDir ==="
    & wsl bash -c "mkdir -p '$hostWsl/entry/libs/arm64-v8a' '$hostWsl/entry/libs/x86_64' && cp '$BUILD/samples/dotnet/$DemoApp/bin/Release/net10.0/linux-musl-arm64/publish/app.so' '$hostWsl/entry/libs/arm64-v8a/libapp.so' && cp '$BUILD/samples/dotnet/$DemoApp/bin/Release/net10.0/linux-musl-x64/publish/app.so' '$hostWsl/entry/libs/x86_64/libapp.so'"
    if ($LASTEXITCODE -ne 0) { throw "复制 libapp.so 失败" }

    Get-ChildItem (Join-Path $HostDir "entry/libs/arm64-v8a/libapp.so"), (Join-Path $HostDir "entry/libs/x86_64/libapp.so") | Format-Table -AutoSize
    Write-Host "=== 完成 ==="
    exit 0
}

Write-Host "=== 1. 打包源码 ==="
# 归档名用相对路径：GNU tar 会把含盘符冒号的路径当远程主机（"Cannot connect to D:"）
& tar czf "tmp/arkts-src.tgz" @tarArgs
if ($LASTEXITCODE -ne 0) { throw "打包失败" }
$tempArchive = "tmp/arkts-src.tgz"

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

Write-Host "=== 4. 取回 libapp.so（双架构）→ $HostDir ==="
# 创建本地目标目录
New-Item -ItemType Directory -Force -Path (Join-Path $HostDir "entry/libs/arm64-v8a") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $HostDir "entry/libs/x86_64")   | Out-Null

& scp -q "$($REMOTE):$BUILD/samples/dotnet/$DemoApp/bin/Release/net10.0/linux-musl-arm64/publish/app.so" `
       (Join-Path $HostDir "entry/libs/arm64-v8a/libapp.so")
if ($LASTEXITCODE -ne 0) { throw "取回 arm64 架构文件失败" }

& scp -q "$($REMOTE):$BUILD/samples/dotnet/$DemoApp/bin/Release/net10.0/linux-musl-x64/publish/app.so" `
       (Join-Path $HostDir "entry/libs/x86_64/libapp.so")
if ($LASTEXITCODE -ne 0) { throw "取回 x86_64 架构文件失败" }

# 显示取回的文件信息
Get-ChildItem -Recurse (Join-Path $HostDir "entry/libs/arm64-v8a/libapp.so"), (Join-Path $HostDir "entry/libs/x86_64/libapp.so") | Format-Table -AutoSize

Write-Host "=== 完成 ==="