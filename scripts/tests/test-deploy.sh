#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
SCRIPTS_DIR="$(dirname "$SCRIPT_DIR")"
WORK="$(mktemp -d)"
HOST_DIR="$WORK/host"
mkdir -p "$HOST_DIR/AppScope"
printf '{ "bundleName": "com.example.deploytest" }\n' > "$HOST_DIR/AppScope/app.json5"

invoke_deploy() {
    local mode="$1" timeout="$2"
    shift 2
    set +e
    OUTPUT="$(env FAKE_HDC_MODE="$mode" HARMONY_HDC="$WORK/hdc" HOST_DIR="$HOST_DIR" \
        HARMONY_STARTUP_TIMEOUT_SECONDS="$timeout" \
        bash "$SCRIPTS_DIR/deploy-hap.sh" "$@" 2>&1)"
    STATUS=$?
    set -e
}

cleanup() { rm -rf "$WORK"; }
trap cleanup EXIT

cp "$SCRIPT_DIR/fake-hdc.sh" "$WORK/hdc"
chmod +x "$WORK/hdc"

invoke_deploy no-device 1 --preflight-only
[ "$STATUS" -ne 0 ] && grep -q "没有已连接的设备" <<< "$OUTPUT"
echo "PASS no device"

invoke_deploy missing-target 1 --preflight-only 10.0.0.9:5555
[ "$STATUS" -ne 0 ] && grep -q "指定目标 10.0.0.9:5555" <<< "$OUTPUT"
echo "PASS missing target"

mkdir -p "$HOST_DIR/entry/build/default/outputs/default"
printf 'fake\n' > "$HOST_DIR/entry/build/default/outputs/default/entry-default-unsigned.hap"

invoke_deploy success 1
[ "$STATUS" -eq 0 ] && grep -q "HarmonyHost startup verified" <<< "$OUTPUT"
echo "PASS startup success"

invoke_deploy timeout 0
[ "$STATUS" -ne 0 ] && grep -q "启动超时" <<< "$OUTPUT" && grep -q "最近 hilog" <<< "$OUTPUT"
echo "PASS startup timeout"
