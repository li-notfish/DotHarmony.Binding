#!/usr/bin/env bash
set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CHECK_DEVICE=0
if [ "${1:-}" = "--check-device" ]; then
    CHECK_DEVICE=1
    shift
fi
if [ "$#" -gt 0 ]; then
    echo "usage: $0 [--check-device]" >&2
    exit 2
fi

FAILED=0
ok() { printf '[OK]   %s: %s\n' "$1" "$2"; }
fail() { printf '[FAIL] %s: %s\n' "$1" "$2" >&2; FAILED=$((FAILED + 1)); }

DOTNET_CMD=""
if command -v dotnet >/dev/null 2>&1; then DOTNET_CMD=dotnet
elif [ -x "/mnt/c/Program Files/dotnet/dotnet.exe" ]; then DOTNET_CMD="/mnt/c/Program Files/dotnet/dotnet.exe"
fi
if [ -n "$DOTNET_CMD" ]; then
    DOTNET_VERSION="$("$DOTNET_CMD" --version 2>/dev/null | tail -n 1)"
    case "$DOTNET_VERSION" in
        10.*|[1-9][0-9].*) ok ".NET SDK" "$DOTNET_VERSION" ;;
        *) fail ".NET SDK" "需要 .NET 10 SDK，当前为 ${DOTNET_VERSION:-unknown}" ;;
    esac
else
    fail ".NET SDK" "dotnet not found"
fi

DEVECO_FOUND=""
for candidate in "${DEVECO_HOME:-}" \
    "D:/Program Files/Huawei/DevEco Studio" \
    "C:/Program Files/Huawei/DevEco Studio" \
    "/mnt/d/Program Files/Huawei/DevEco Studio"; do
    [ -n "$candidate" ] || continue
    if [ -f "$candidate/tools/hvigor/bin/hvigorw.js" ]; then
        DEVECO_FOUND="$candidate"
        break
    fi
done
if [ -n "$DEVECO_FOUND" ]; then
    ok "DevEco Studio / hvigor" "$DEVECO_FOUND"
else
    fail "DevEco Studio / hvigor" "hvigorw.js not found; set DEVECO_HOME"
fi

NODE_CMD=""
if command -v node >/dev/null 2>&1; then NODE_CMD=node
elif [ -x "/mnt/c/Program Files/nodejs/node.exe" ]; then NODE_CMD="/mnt/c/Program Files/nodejs/node.exe"
elif [ -n "$DEVECO_FOUND" ] && [ -x "$DEVECO_FOUND/tools/node/node.exe" ]; then
    NODE_CMD="$DEVECO_FOUND/tools/node/node.exe"
fi
if [ -n "$NODE_CMD" ]; then
    ok "Node.js" "$("$NODE_CMD" --version 2>/dev/null)"
else
    fail "Node.js" "node not found"
fi

PYTHON=""
if command -v python3 >/dev/null 2>&1; then PYTHON=python3
elif command -v python >/dev/null 2>&1; then PYTHON=python
fi
if [ -n "$PYTHON" ]; then
    ok "Python" "$("$PYTHON" --version 2>&1)"
else
    fail "Python" "python3/python not found"
fi

HDC=""
SDK_ROOT=""
if SDK_ROOT="$(bash "$SCRIPT_DIR/lib/sdk.sh" sdk-root 2>/dev/null)" &&
   HDC="$(bash "$SCRIPT_DIR/lib/sdk.sh" find-hdc 2>/dev/null)"; then
    PACKAGE_VERSION="$(sed -n 's/.*"platformVersion"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' \
        "$SDK_ROOT/ets/oh-uni-package.json" 2>/dev/null | head -n 1)"
    PACKAGE_VERSION="${PACKAGE_VERSION:-$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' \
        "$SDK_ROOT/oh-uni-package.json" 2>/dev/null | head -n 1)}"
    ok "SDK / hdc" "SDK ${PACKAGE_VERSION:-unknown}, hdc=$HDC"
else
    fail "SDK / hdc" "SDK/hdc discovery failed; set OHOS_SDK_BASE or OHSDK_HOME"
fi

if [ "$CHECK_DEVICE" -eq 1 ]; then
    if [ -z "$HDC" ]; then
        fail "设备连接" "前序 hdc 检查失败"
    else
        TARGETS="$("$HDC" list targets 2>/dev/null | tr -d '\r' | sed '/^[[:space:]]*$/d;/^\[Empty\]$/d')"
        if [ -n "$TARGETS" ]; then
            ok "设备连接" "$(tr '\n' ',' <<< "$TARGETS" | sed 's/,$//')"
        else
            fail "设备连接" "hdc list targets 为空；请启动设备或执行 hdc tconn <host:port>"
        fi
    fi
else
    echo "[SKIP] 设备连接（使用 --check-device 启用）"
fi

if [ "$FAILED" -gt 0 ]; then
    echo "Harmony environment check failed: $FAILED issue(s)." >&2
    exit 1
fi
echo "Harmony environment check passed."
