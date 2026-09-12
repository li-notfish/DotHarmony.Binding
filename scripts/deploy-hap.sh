#!/bin/bash
# 一键部署：重装 HAP → 启动 → 抓取 HarmonyHost 日志。
# SDK/hdc 定位顺序：OHOS_SDK_BASE > OHSDK_HOME > D:\Harmony\OpenHarmony\Sdk > DevEco sdk。
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# bundle 名：HOST_DIR 为暂存宿主时由 stage-host 写入 app.json5，这里同步读取；默认共享模板
if [ -n "$HOST_DIR" ] && [ -f "$HOST_DIR/AppScope/app.json5" ]; then
    BUNDLE=$(sed -n 's/.*"bundleName"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$HOST_DIR/AppScope/app.json5" | head -1)
fi
BUNDLE="${BUNDLE:-com.arktsbinding.harmonyhost}"
ABILITY=EntryAbility
MODULE=entry

# ---- 定位 hdc ----
HDC=""
for base in "$OHOS_SDK_BASE" "$OHSDK_HOME" "D:/Harmony/OpenHarmony/Sdk" "C:/Program Files/Huawei/DevEco Studio/sdk"; do
    [ -n "$base" ] || continue
    for rel in "26.0.0/toolchains/hdc.exe" "toolchains/hdc.exe"; do
        if [ -f "$base/$rel" ]; then HDC="$base/$rel"; break 2; fi
    done
done
if [ -z "$HDC" ]; then
    echo "错误: 找不到 hdc.exe。请设置 OHOS_SDK_BASE 指向 OpenHarmony SDK 根目录（含 26.0.0/toolchains）" >&2
    exit 1
fi
echo "hdc: $HDC"

# ---- 目标设备：$1 或 $HDC_TARGET（多设备在线时指定，如 127.0.0.1:5555 / 192.168.1.6:8710）----
HDC_TARGET="${1:-${HDC_TARGET:-}}"
hdc_t() {
    if [ -n "$HDC_TARGET" ]; then "$HDC" -t "$HDC_TARGET" "$@"; else "$HDC" "$@"; fi
}

# ---- 定位 HAP ----
# HAP 路径：HOST_DIR 覆盖（targets 生成的按应用暂存宿主），默认共享模板
HAP="${HOST_DIR:-${PROJECT_ROOT}/samples/HarmonyHost}/entry/build/default/outputs/default/${MODULE}-default-unsigned.hap"
if [ ! -f "$HAP" ]; then
    echo "错误: 找不到 HAP：$HAP（请先运行 scripts/build-hap.cmd）" >&2
    exit 1
fi
# hdc.exe 不认 MSYS 正斜杠绝对路径（会拼到自身 CWD 前面），Git Bash 下转成反斜杠 Windows 路径
if command -v cygpath >/dev/null 2>&1; then HAP_WIN=$(cygpath -w "$HAP"); else HAP_WIN="$HAP"; fi

echo "=== 1. 检查设备 ==="
TARGETS=$("$HDC" list targets | tr -d '[:space:]')
if [ -z "$TARGETS" ] || [ "$TARGETS" = "[Empty]" ]; then
    echo "错误: 没有已连接的设备/模拟器（hdc list targets 为空）" >&2
    exit 1
fi
echo "targets: $TARGETS"
if [ -n "$HDC_TARGET" ] && ! grep -q "^${HDC_TARGET}$" <("$HDC" list targets | tr -d '\r'); then
    echo "错误: 指定目标 $HDC_TARGET 不在 hdc list targets 中（先 hdc tconn）" >&2
    exit 1
fi

echo "=== 2. 清空 hilog ==="
hdc_t shell hilog -r >/dev/null 2>&1 || true

echo "=== 3. 安装 HAP ==="
# 先停掉旧实例：install -r 与运行中实例存在时序竞争（新实例可能启动即被销毁）
hdc_t shell "aa force-stop $BUNDLE" >/dev/null 2>&1 || true
if ! hdc_t install -r "$HAP_WIN" | grep -q "install bundle successfully"; then
    echo "错误: 安装失败" >&2
    exit 1
fi

echo "=== 4. 启动应用 ==="
hdc_t shell aa start -a "$ABILITY" -b "$BUNDLE" -m "$MODULE"

echo "=== 5. 等待后抓取日志 ==="
sleep 6
hdc_t shell "hilog -x | grep -aE 'A00000/HarmonyHost|dlopen|libapp|dotnet|DOTNET'" | tail -40
