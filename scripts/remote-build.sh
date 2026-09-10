#!/bin/bash
# 本地编排：打包源码 → 上传远程 → 构建 libapp.so → 取回放入 HarmonyHost/libs
# LOCAL=true：在本地 WSL 直接构建，跳过 SSH/SCP
set -e
cd "$(dirname "$0")/.."

# LOCAL 模式——在本地 WSL 直接构建，跳过 SSH/SCP
if [ "$LOCAL" = "true" ]; then
  echo "=== 本地 WSL 构建 ==="
  bash "$(dirname "$0")/build-libapp.sh"

  echo "=== 复制 libapp.so（双架构） ==="
  mkdir -p samples/HarmonyHost/entry/libs/arm64-v8a samples/HarmonyHost/entry/libs/x86_64
  cp samples/dotnet/HelloApp/bin/Release/net10.0/linux-musl-arm64/publish/app.so \
     samples/HarmonyHost/entry/libs/arm64-v8a/libapp.so
  cp samples/dotnet/HelloApp/bin/Release/net10.0/linux-musl-x64/publish/app.so \
     samples/HarmonyHost/entry/libs/x86_64/libapp.so
  ls -la samples/HarmonyHost/entry/libs/arm64-v8a/libapp.so samples/HarmonyHost/entry/libs/x86_64/libapp.so
  echo "=== 完成 ==="
  exit 0
fi

if [ -n "$REMOTE" ]; then
  REMOTE_ALIAS=$REMOTE
else
  # 未显式指定 REMOTE 时自动定位构建机：resolve 脚本会把最新 IP 写入 ~/.ssh/config，
  # 连接始终走别名，用户名/密钥由 ssh config 提供
  pwsh -NoProfile -File "$(dirname "$0")/resolve-remote.ps1" >/dev/null
  REMOTE_ALIAS=wsl_auzrelinux
fi
BUILD=${BUILD:-/tmp/arktsbinding}

echo "=== 1. 打包源码 ==="
tar czf /tmp/arkts-src.tgz \
  --exclude=node_modules --exclude=.git --exclude=dist --exclude=coverage \
  --exclude=bin --exclude=obj --exclude=output --exclude=UnityHarmony \
  src HarmonyOS.Bindings samples scripts ArkTsBinding.slnx

echo "=== 2. 上传并解压 (构建机: $REMOTE_ALIAS) ==="
ssh $REMOTE_ALIAS "rm -rf $BUILD && mkdir -p $BUILD"
scp -q /tmp/arkts-src.tgz $REMOTE_ALIAS:$BUILD/src.tgz
ssh $REMOTE_ALIAS "cd $BUILD && tar xzf src.tgz"

echo "=== 3. 远程构建 ==="
ssh $REMOTE_ALIAS "bash $BUILD/scripts/build-libapp.sh"

echo "=== 4. 取回 libapp.so（双架构） ==="
mkdir -p samples/HarmonyHost/entry/libs/arm64-v8a samples/HarmonyHost/entry/libs/x86_64
scp -q $REMOTE_ALIAS:$BUILD/samples/dotnet/HelloApp/bin/Release/net10.0/linux-musl-arm64/publish/app.so \
   samples/HarmonyHost/entry/libs/arm64-v8a/libapp.so
scp -q $REMOTE_ALIAS:$BUILD/samples/dotnet/HelloApp/bin/Release/net10.0/linux-musl-x64/publish/app.so \
   samples/HarmonyHost/entry/libs/x86_64/libapp.so
ls -la samples/HarmonyHost/entry/libs/arm64-v8a/libapp.so samples/HarmonyHost/entry/libs/x86_64/libapp.so
echo "=== 完成 ==="
