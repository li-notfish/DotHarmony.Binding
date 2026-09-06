#!/bin/bash
# 在远程 Linux 上执行：NativeAOT 发布 HelloApp → libapp.so（arm64 + x86_64 双架构）
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

cd "$(dirname "$0")/../samples/dotnet/HelloApp"

echo "=== publishing linux-musl-arm64 (musl.cc gcc) ==="
dotnet publish -c Release -r linux-musl-arm64 \
  -p:CppCompilerAndLinker=$WRAP64 \
  -p:ObjCopyName=$HOME/aarch64-linux-musl-cross/bin/aarch64-linux-musl-objcopy \
  2>&1 | tail -2
file bin/Release/net10.0/linux-musl-arm64/publish/app.so

echo "=== publishing linux-musl-x64 (zig cc + zig objcopy) ==="
dotnet publish -c Release -r linux-musl-x64 \
  -p:CppCompilerAndLinker=$WRAPX \
  -p:ObjCopyName=$HOME/zig/objcopy-gnu \
  2>&1 | tail -2
file bin/Release/net10.0/linux-musl-x64/publish/app.so
