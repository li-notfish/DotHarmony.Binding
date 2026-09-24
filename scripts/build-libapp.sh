#!/bin/bash
# 在远程 Linux 上执行：NativeAOT 发布 demo 应用 → libapp.so（arm64 + x86_64 双架构）
# DEMO_APP 环境变量选择 demo 工程（默认 HelloApp=控件 demo；可选 ApiDemo=@ohos.* API 绑定 demo）
# 前置：$HOME/.dotnet（SDK 10）
#   arm64: $HOME/aarch64-linux-musl-cross（musl.cc gcc）
#   x64:   $HOME/zig（zig cc，musl.cc x64 工具链下载完成后可切换）
set -e
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$HOME/.dotnet:$PATH

# aarch64 wrapper（幂等）：过滤 clang 风格 --target
WRAP64=$HOME/aarch64-linux-musl-cross/bin/naot-driver
if [ ! -x "$WRAP64" ]; then
  cat > "$WRAP64" <<'W'
#!/bin/bash
args=()
for a in "$@"; do
  case "$a" in
    --target=*) ;;
    *) args+=("$a") ;;
  esac
done
exec "$HOME/aarch64-linux-musl-cross/bin/aarch64-linux-musl-gcc" "${args[@]}"
W
  chmod +x "$WRAP64"
fi

# x64 zig wrapper（幂等）：过滤 zig 链接驱动不支持的 GNU-ld 参数
WRAPX=$HOME/zig/naot-driver-zig
if [ ! -x "$WRAPX" ]; then
  cat > "$WRAPX" <<'W'
#!/bin/bash
args=()
for a in "$@"; do
  case "$a" in
    -Wl,--discard-all|-Wl,--gc-sections) ;;
    *) args+=("$a") ;;
  esac
done
exec "$HOME/zig/zig" cc "${args[@]}"
W
  chmod +x "$WRAPX"
fi

# zig objcopy GNU 兼容 wrapper（幂等）：支持 in-place，过滤不支持的参数
OBJCOPY_GNU=$HOME/zig/objcopy-gnu
if [ ! -x "$OBJCOPY_GNU" ]; then
  cat > "$OBJCOPY_GNU" <<'W'
#!/bin/bash
args=()
pos=()
for a in "$@"; do
  case "$a" in
    --strip-unneeded|--add-gnu-debuglink=*) ;;
    -*) args+=("$a") ;;
    *) pos+=("$a") ;;
  esac
done
in="${pos[0]}"
out="${pos[1]:-}"
if [ -z "$out" ]; then
  tmp=$(mktemp /tmp/objcopy.XXXXXX)
  "$HOME/zig/zig" objcopy "${args[@]}" "$in" "$tmp" && cat "$tmp" > "$in" && rm -f "$tmp"
else
  exec "$HOME/zig/zig" objcopy "${args[@]}" "$in" "$out"
fi
W
  chmod +x "$OBJCOPY_GNU"
fi

# 应用工程路径（相对打包根）：仓库内默认 samples/dotnet/$DEMO_APP；
# 包消费模式由 remote-build 传入 APP_PATH（应用工程在其源码树内的相对路径）
DEMO_APP="${DEMO_APP:-HelloApp}"
APP_PATH="${APP_PATH:-samples/dotnet/$DEMO_APP}"
# 打包根：仓库内为脚本上级；消费方模式由 remote-build 显式指到解包目录
SRC_ROOT="${SRC_ROOT:-$(cd "$(dirname "$0")/.." && pwd)}"
echo "=== demo app: $DEMO_APP (path: $APP_PATH, root: $SRC_ROOT) ==="

# 消费方模式停用 workload 解析后，被引用类库的多 TFM（net10.0-android 等）无法求值——
# remote-build 会同时传入 PIN_TFM=net10.0 把整棵引用树钉到纯托管目标
PIN_ARGS=()
if [ -n "$PIN_TFM" ]; then
  PIN_ARGS+=(-p:TargetFrameworks=$PIN_TFM -p:TargetFramework=$PIN_TFM)
fi
# 与 TFM 同理：消费方若用 $(MauiVersion) 占位（workload 提供），解析器关闭后为空——
# 由 remote-build 显式钉到包要求版本
if [ -n "$PIN_MAUI" ]; then
  PIN_ARGS+=(-p:MauiVersion=$PIN_MAUI)
fi

echo "=== publishing linux-musl-arm64 (musl.cc gcc) ==="
dotnet publish -c Release -r linux-musl-arm64 \
  "$SRC_ROOT/$APP_PATH" \
  "${PIN_ARGS[@]}" \
  -p:CppCompilerAndLinker=$WRAP64 \
  -p:ObjCopyName=$HOME/aarch64-linux-musl-cross/bin/aarch64-linux-musl-objcopy \
  2>&1 | tail -2
file "$SRC_ROOT/$APP_PATH/bin/Release/net10.0/linux-musl-arm64/publish/app.so"

echo "=== publishing linux-musl-x64 (zig cc + zig objcopy) ==="
dotnet publish -c Release -r linux-musl-x64 \
  "$SRC_ROOT/$APP_PATH" \
  "${PIN_ARGS[@]}" \
  -p:CppCompilerAndLinker=$WRAPX \
  -p:ObjCopyName=$HOME/zig/objcopy-gnu \
  2>&1 | tail -2
file "$SRC_ROOT/$APP_PATH/bin/Release/net10.0/linux-musl-x64/publish/app.so"
