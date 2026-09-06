#!/bin/bash
# 本地编排：打包源码 → 上传远程 → 构建 libapp.so → 取回放入 HarmonyHost/libs
set -e
cd "$(dirname "$0")/.."
REMOTE=${REMOTE:-wsl_auzrelinux}
BUILD=${BUILD:-/tmp/arktsbinding}

echo "=== 1. 打包源码 ==="
tar czf /tmp/arkts-src.tgz \
  --exclude=node_modules --exclude=.git --exclude=dist --exclude=coverage \
  --exclude=bin --exclude=obj --exclude=output --exclude=UnityHarmony \
  src HarmonyOS.Bindings samples scripts ArkTsBinding.slnx

echo "=== 2. 上传并解压 ==="
ssh $REMOTE "rm -rf $BUILD && mkdir -p $BUILD"
scp -q /tmp/arkts-src.tgz $REMOTE:$BUILD/src.tgz
ssh $REMOTE "cd $BUILD && tar xzf src.tgz"

echo "=== 3. 远程构建 ==="
ssh $REMOTE "bash $BUILD/scripts/build-libapp.sh"

echo "=== 4. 取回 libapp.so（双架构） ==="
mkdir -p samples/HarmonyHost/entry/libs/arm64-v8a samples/HarmonyHost/entry/libs/x86_64
scp -q $REMOTE:$BUILD/samples/dotnet/HelloApp/bin/Release/net10.0/linux-musl-arm64/publish/app.so \
   samples/HarmonyHost/entry/libs/arm64-v8a/libapp.so
scp -q $REMOTE:$BUILD/samples/dotnet/HelloApp/bin/Release/net10.0/linux-musl-x64/publish/app.so \
   samples/HarmonyHost/entry/libs/x86_64/libapp.so
ls -la samples/HarmonyHost/entry/libs/arm64-v8a/libapp.so samples/HarmonyHost/entry/libs/x86_64/libapp.so
echo "=== 完成 ==="
