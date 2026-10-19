#!/usr/bin/env bash
set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
. "$SCRIPT_DIR/lib/sdk.sh"
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
    if [ -f "$DEVECO_FOUND/tools/hvigor/hvigor/hvigor-config.json5" ]; then
        MODEL_VERSION="$(sed -n 's/.*"modelVersion"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' \
            "$DEVECO_FOUND/tools/hvigor/hvigor/hvigor-config.json5" 2>/dev/null | head -n 1)"
        [ -n "$MODEL_VERSION" ] && ok "hvigor model" "$MODEL_VERSION"
    fi
    OHPM=""
    for candidate in "$DEVECO_FOUND/tools/ohpm/bin/ohpm.bat" "$DEVECO_FOUND/tools/ohpm/bin/ohpm"; do
        [ -f "$candidate" ] && { OHPM="$candidate"; break; }
    done
    if [ -n "$OHPM" ]; then
        ok "ohpm" "$OHPM"
    elif command -v ohpm >/dev/null 2>&1; then
        ok "ohpm" "$(command -v ohpm)"
    else
        printf '[SKIP] ohpm: not found (staged host may still build if oh_modules is present)\n'
    fi
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
    NODE_VERSION="$("$NODE_CMD" --version 2>/dev/null | tr -d 'v')"
    NODE_MAJOR="${NODE_VERSION%%.*}"
    case "$NODE_MAJOR" in
        ''|*[!0-9]*) fail "Node.js" "无法解析版本: ${NODE_VERSION:-unknown}" ;;
        *) if [ "$NODE_MAJOR" -ge 18 ]; then ok "Node.js" "v$NODE_VERSION"; else fail "Node.js" "需要 >= 18，当前 v$NODE_VERSION"; fi ;;
    esac
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
if harmony_find_sdk_root; then
    SDK_ROOT="$HARMONY_SDK_ROOT"
    harmony_read_sdk_version "$SDK_ROOT"
    if harmony_read_sdk_api_version "$SDK_ROOT"; then API_VERSION="$HARMONY_SDK_API_VERSION"; else API_VERSION="unknown"; fi
    if harmony_find_hdc; then
        ok "SDK / hdc" "SDK ${HARMONY_SDK_VERSION:-unknown} / API $API_VERSION, hdc=$HDC"
    else
        fail "SDK / hdc" "SDK found at $SDK_ROOT but hdc missing; set HARMONY_HDC"
    fi
else
    fail "SDK / hdc" "SDK not found; set OHOS_SDK_BASE / OHSDK_HOME / DEVECO_SDK_HOME"
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
