#!/bin/bash
# 一键部署：重装 HAP → 启动 → 抓取 HarmonyHost 日志。
# SDK/hdc 定位由 scripts/lib/sdk.sh 统一处理。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

PREFLIGHT_ONLY=0
if [ "${1:-}" = "--preflight-only" ]; then
    PREFLIGHT_ONLY=1
    shift
fi
if [ "$#" -gt 1 ]; then
    echo "错误: deploy-hap.sh 最多接受一个目标参数" >&2
    exit 2
fi

# 宿主目录：HOST_DIR 覆盖；未设置时只在“恰好一个已暂存宿主”时自动采用，
# 绝不回退共享模板（AGENTS.md：samples/HarmonyHost 不是可部署宿主）。
HOST_DIR="${HOST_DIR:-}"
if [ -z "$HOST_DIR" ]; then
    STAGED_COUNT=0
    FIRST_STAGED=""
    for candidate in "$PROJECT_ROOT"/samples/dotnet/*/obj/harmony/host; do
        [ -f "$candidate/AppScope/app.json5" ] || continue
        STAGED_COUNT=$((STAGED_COUNT + 1))
        FIRST_STAGED="$candidate"
    done
    if [ "$STAGED_COUNT" -eq 1 ]; then
        HOST_DIR="$FIRST_STAGED"
        echo "HOST_DIR 未设置；使用唯一暂存宿主: $HOST_DIR"
    elif [ "$STAGED_COUNT" -gt 1 ]; then
        echo "错误: 未设置 HOST_DIR，且检测到多个暂存宿主。请显式指定一个：" >&2
        for candidate in "$PROJECT_ROOT"/samples/dotnet/*/obj/harmony/host; do
            [ -f "$candidate/AppScope/app.json5" ] && printf '  %s\n' "$candidate" >&2
        done
        exit 1
    else
        echo "错误: 未找到暂存宿主。请先运行 dotnet build <工程> -t:HarmonyRun 或 -t:HarmonyStageHost。" >&2
        exit 1
    fi
fi
if [ ! -f "$HOST_DIR/AppScope/app.json5" ]; then
    echo "错误: 宿主目录无效（缺少 AppScope/app.json5）: $HOST_DIR" >&2
    exit 1
fi

# bundle 名：由 stage-host 写入 app.json5，这里同步读取
BUNDLE=$(sed -n 's/.*"bundleName"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$HOST_DIR/AppScope/app.json5" | head -1)
BUNDLE="${BUNDLE:-com.arktsbinding.harmonyhost}"
ABILITY=EntryAbility
MODULE=entry

# ---- 定位 hdc ----
HDC="$(bash "$SCRIPT_DIR/lib/sdk.sh" find-hdc)" || exit 1
echo "hdc: $HDC"

# ---- 目标设备：构建 AOT/Hvigor 前完成预检 ----
TARGETS="$("$HDC" list targets | tr -d '\r' | sed '/^[[:space:]]*$/d;/^\[Empty\]$/d')"
if [ -z "$TARGETS" ]; then
    echo "错误: 没有已连接的设备/模拟器（hdc list targets 为空）" >&2
    exit 1
fi
HDC_TARGET="${1:-${HDC_TARGET:-}}"
if [ -n "$HDC_TARGET" ] && ! grep -Fxq "$HDC_TARGET" <<< "$TARGETS"; then
    echo "错误: 指定目标 $HDC_TARGET 不在 hdc list targets 中（先 hdc tconn）" >&2
    exit 1
fi
echo "targets: $(tr '\n' ',' <<< "$TARGETS" | sed 's/,$//')"
hdc_t() {
    if [ -n "$HDC_TARGET" ]; then "$HDC" -t "$HDC_TARGET" "$@"; else "$HDC" "$@"; fi
}
if [ "$PREFLIGHT_ONLY" -eq 1 ]; then
    echo "=== Harmony device preflight OK"
    exit 0
fi

# ---- 定位 HAP：签名产物必须不旧于 unsigned ----
OUTPUT_DIR="$HOST_DIR/entry/build/default/outputs/default"
SIGNED_HAP="$OUTPUT_DIR/${MODULE}-default-signed.hap"
UNSIGNED_HAP="$OUTPUT_DIR/${MODULE}-default-unsigned.hap"
HAP=""
if [ -f "$SIGNED_HAP" ]; then
    if [ -f "$UNSIGNED_HAP" ] && [ "$SIGNED_HAP" -ot "$UNSIGNED_HAP" ]; then
        echo "警告: signed HAP 旧于 unsigned（签名未随本次构建重跑）；改为安装 unsigned。" >&2
        HAP="$UNSIGNED_HAP"
    else
        HAP="$SIGNED_HAP"
    fi
elif [ -f "$UNSIGNED_HAP" ]; then
    HAP="$UNSIGNED_HAP"
else
    echo "错误: 找不到 HAP：$OUTPUT_DIR（请先运行 scripts/build-hap.cmd，或用 -t:HarmonyRun）" >&2
    exit 1
fi
echo "HAP: $HAP"
# hdc.exe 不认 MSYS 正斜杠绝对路径（会拼到自身 CWD 前面），Git Bash 下转成反斜杠 Windows 路径
if command -v cygpath >/dev/null 2>&1; then HAP_WIN=$(cygpath -w "$HAP"); else HAP_WIN="$HAP"; fi

echo "=== 2. 清空 hilog ==="
hdc_t shell hilog -r >/dev/null 2>&1 || true

echo "=== 3. 安装 HAP ==="
# 先停掉旧实例：install -r 与运行中实例存在时序竞争（新实例可能启动即被销毁）
hdc_t shell "aa force-stop $BUNDLE" >/dev/null 2>&1 || true
INSTALL_OUT="$(hdc_t install -r "$HAP_WIN" 2>&1 || true)"
if ! grep -q "install bundle successfully" <<< "$INSTALL_OUT"; then
    echo "错误: 安装失败：$INSTALL_OUT" >&2
    exit 1
fi

echo "=== 4. 启动应用 ==="
START_OUTPUT="$(hdc_t shell "aa start -a $ABILITY -b $BUNDLE -m $MODULE -W" 2>&1 || true)"

echo "=== 5. 等待启动就绪 ==="
TIMEOUT_SECONDS="${HARMONY_STARTUP_TIMEOUT_SECONDS:-20}"
if ! awk -v t="$TIMEOUT_SECONDS" 'BEGIN{exit !(t ~ /^[0-9]+(\.[0-9]+)?$/)}'; then
    echo "错误: HARMONY_STARTUP_TIMEOUT_SECONDS 必须是非负数：$TIMEOUT_SECONDS" >&2
    exit 2
fi
now_ms() {
    if [ -n "${EPOCHREALTIME:-}" ]; then
        local digits="${EPOCHREALTIME//[!0-9]/}"
        echo "${digits:0:13}"
    else
        python3 -c 'import time; print(int(time.time() * 1000))'
    fi
}
TIMEOUT_MS=$(awk -v t="$TIMEOUT_SECONDS" 'BEGIN{printf "%d", t * 1000}')
DEADLINE=$(( $(now_ms) + TIMEOUT_MS ))
READY=0
RECENT_LOG=""
while :; do
    PROCESS_OUTPUT="$(hdc_t shell "pidof $BUNDLE" 2>/dev/null || true)"
    RECENT_LOG="$(hdc_t shell "hilog -x" 2>/dev/null || true)"
    if grep -Eq '(^|[^0-9])[0-9]+' <<< "$PROCESS_OUTPUT" ||
       grep -q 'A00000/HarmonyHost' <<< "$RECENT_LOG"; then
        READY=1
        break
    fi
    [ "$(now_ms)" -ge "$DEADLINE" ] && break
    sleep 0.5
done

if [ "$READY" -ne 1 ]; then
    echo "错误: 启动超时（${TIMEOUT_SECONDS} 秒）：未发现 $BUNDLE 进程或 HarmonyHost 日志。" >&2
    echo "aa start 输出: $START_OUTPUT" >&2
    echo "最近 hilog:" >&2
    tail -40 <<< "$RECENT_LOG" >&2
    exit 1
fi

grep -aE 'A00000/HarmonyHost|dlopen|libapp|dotnet|DOTNET' <<< "$RECENT_LOG" | tail -40 || true
echo "=== HarmonyHost startup verified"
