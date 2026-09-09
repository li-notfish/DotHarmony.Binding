#!/bin/bash
# 一键部署：重装 HAP → 启动 → 抓取 HarmonyHost 日志
set -e

# 获取脚本所在目录的绝对路径，然后进入上一级（项目根目录）
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

HDC="$OHSDK_HOME/26.0.0/toolchains/hdc.exe"
HAP="${PROJECT_ROOT}/samples/HarmonyHost/entry/build/default/outputs/default/entry-default-unsigned.hap"

echo "=== 1. 清空 hilog ==="
"$HDC" shell hilog -r >/dev/null

echo "=== 2. 安装 HAP ==="
"$HDC" install -r "$HAP"

echo "=== 3. 启动应用 ==="
"$HDC" shell aa start -a EntryAbility -b com.arktsbinding.harmonyhost -m entry

echo "=== 4. 等待后抓取日志 ==="
sleep 6
"$HDC" shell "hilog -x | grep -aE 'A00000/HarmonyHost|dlopen|libapp|dotnet|DOTNET'" | tail -25
