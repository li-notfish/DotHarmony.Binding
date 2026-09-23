#!/bin/bash
# M3tail 小项 uitest 验证：Essentials 新服务（传感器/定位/媒体选择）+ Controls 新功能
# （CarouselView 位置回传/跳转、Grid Span>1 Auto 轨道、Drag&Drop）
# 用法：MSYS_NO_PATHCONV=1 bash scripts/verify-m3tail-uitest.sh [essentials|hello] [target]
# 结果在界面 Label（不在日志）；每点一个按钮读一次。
set -e
MODE="${1:-essentials}"
TARGET="${2:-127.0.0.1:5555}"
HDC=""
for base in "$OHOS_SDK_BASE" "D:/Harmony/OpenHarmony/Sdk" "D:/Program Files/Huawei/DevEco Studio/sdk/default/openharmony"; do
    [ -n "$base" ] || continue
    for rel in "26.0.0/toolchains/hdc.exe" "toolchains/hdc.exe"; do
        [ -f "$base/$rel" ] && HDC="$base/$rel" && break 2
    done
done
[ -n "$HDC" ] || { echo "hdc not found"; exit 1; }
t() { "$HDC" -t "$TARGET" "$@"; }

mkdir -p tmp
DUMP=tmp/m3tail.json
snap() { # dumpLayout → 文本节点（先 rm 设备端文件防静默失败取旧档）
    t shell rm -f /data/local/tmp/m3tail.json >/dev/null 2>&1 || true
    t shell uitest dumpLayout -p /data/local/tmp/m3tail.json >/dev/null 2>&1 || true
    t file recv /data/local/tmp/m3tail.json "$DUMP" >/dev/null 2>&1 || true
    python -c "
import json,sys
d=json.load(open('$DUMP',encoding='utf-8'))
def walk(n):
    a=n.get('attributes',{})
    if a.get('text'): print(a.get('bounds'),repr(a['text']))
    for c in n.get('children',[]): walk(c)
walk(d)"
}
click() { # 按文本找中心点点击
    t shell rm -f /data/local/tmp/m3tail.json >/dev/null 2>&1 || true
    t shell uitest dumpLayout -p /data/local/tmp/m3tail.json >/dev/null 2>&1 || true
    t file recv /data/local/tmp/m3tail.json "$DUMP" >/dev/null 2>&1 || true
    COORD=$(python -c "
import json,re,sys
want=sys.argv[1]
d=json.load(open('$DUMP',encoding='utf-8'))
def walk(n):
    a=n.get('attributes',{})
    if a.get('text','')==want:
        b=[int(x) for x in re.findall(r'-?\d+',a['bounds'])]
        print((b[0]+b[2])//2,(b[1]+b[3])//2); return True
    for c in n.get('children',[]):
        if walk(c): return True
walk(d)" "$1")
    [ -n "$COORD" ] || { echo "FAIL: text not found: $1"; return 1; }
    t shell uitest uiInput click $COORD >/dev/null 2>&1
}
# 按文本找两元素中心点（drag 用）
coords2() {
    t shell rm -f /data/local/tmp/m3tail.json >/dev/null 2>&1 || true
    t shell uitest dumpLayout -p /data/local/tmp/m3tail.json >/dev/null 2>&1 || true
    t file recv /data/local/tmp/m3tail.json "$DUMP" >/dev/null 2>&1 || true
    python -c "
import json,re,sys
d=json.load(open('$DUMP',encoding='utf-8'))
found=[]
def walk(n):
    a=n.get('attributes',{})
    txt=a.get('text','')
    if txt in (sys.argv[1],sys.argv[2]):
        b=[int(x) for x in re.findall(r'-?\d+',a['bounds'])]
        found.append(((b[0]+b[2])//2,(b[1]+b[3])//2,txt))
    for c in n.get('children',[]): walk(c)
walk(d)
for f in found: print(f[0],f[1],f[2])" "$1" "$2"
}

# 解锁（点按钮前屏可能已锁）
t shell power-shell wakeup >/dev/null 2>&1 || true
t shell uitest uiInput swipe 660 2800 660 800 200 >/dev/null 2>&1 || true

if [ "$MODE" = "essentials" ]; then
    BUNDLE="com.arktsbinding.essentialsapp"
    t shell aa force-stop "$BUNDLE" >/dev/null 2>&1 || true
    sleep 1
    t shell aa start -a EntryAbility -b "$BUNDLE" >/dev/null 2>&1
    sleep 8
    echo "=== 1. 滚动到底（新按钮区）"
    t shell uitest uiInput swipe 360 2000 360 500 300 >/dev/null 2>&1 || true
    t shell uitest uiInput swipe 360 2000 360 500 300 >/dev/null 2>&1 || true
    sleep 2
    echo "--- Accelerometer 2s window"
    click "Accelerometer 2s window" || true
    sleep 3.5
    snap | grep -E "accel|compass" || echo "FAIL: accel label not found"
    echo "--- Compass single reading"
    click "Compass single reading" || true
    sleep 3
    snap | grep -E "heading|compass" || echo "FAIL: compass label not found"
    echo "--- Geolocation current"
    click "Geolocation current" || true
    sleep 8
    snap | grep -E "geo:" || echo "FAIL: geolocation label not found"
    echo "--- Pick photo（弹选择器后按 Back 取消）"
    click "Pick photo" || true
    sleep 3
    t shell uitest uiInput keyEvent Back >/dev/null 2>&1 || true
    sleep 2
    snap | grep -E "picked|canceled|FAILED" || echo "note: picker result label not captured"
else
    BUNDLE="com.arktsbinding.helloapp"
    t shell aa force-stop "$BUNDLE" >/dev/null 2>&1 || true
    sleep 1
    t shell aa start -a EntryAbility -b "$BUNDLE" >/dev/null 2>&1
    sleep 8
    echo "=== 1. 进 Controls 页"
    click "Controls demo (10 widgets)" || true
    sleep 2
    echo "=== 2. Carousel 跳转到第 3 页（pos label 应变 2）"
    # Carousel/Jump 现位于页面顶部（Entry/Editor 之后），单次小幅上滑即可全部露出
    t shell uitest uiInput swipe 360 1800 360 1200 300 >/dev/null 2>&1 || true
    sleep 1
    click "跳到第 3 页" || true
    sleep 2
    snap | grep -E "carousel pos" || echo "FAIL: carousel pos label not found"
    echo "=== 3. Span>1 Auto 轨道（span3 标签应完整显示）"
    snap | grep "span3-wide" || echo "FAIL: span3 label not found"
    echo "=== 4. Drag&Drop（长按拖动源 → 目标）"
    C=$(coords2 "长按拖我 (drag me)" "放到这里 (drop here)" || true)
    SX=$(echo "$C" | awk 'NR==1{print $1}'); SY=$(echo "$C" | awk 'NR==1{print $2}')
    TX=$(echo "$C" | awk 'NR==2{print $1}'); TY=$(echo "$C" | awk 'NR==2{print $2}')
    if [ -n "$SX" ] && [ -n "$TX" ]; then
        # uiInput drag（长按起拖语义；swipe 是高速滑动不触发 NODE_ON_DRAG_START，实测）
        t shell uitest uiInput drag "$SX" "$SY" "$TX" "$TY" 400 >/dev/null 2>&1 || true
        sleep 2
        snap | grep -E "drop: " || echo "FAIL: drop label unchanged"
    else
        echo "FAIL: drag source/target not found: [$C]"
    fi
    echo "=== 5. Shape/自绘（滚动到 Shapes 网格 + GraphicsView，截图人工核验）"
    t shell uitest uiInput swipe 360 1800 360 900 400 >/dev/null 2>&1 || true
    t shell uitest uiInput swipe 360 1800 360 1200 400 >/dev/null 2>&1 || true
    sleep 2
    t shell snapshot_display -f /data/local/tmp/m3tail_draw.jpeg >/dev/null 2>&1 || true
    t file recv /data/local/tmp/m3tail_draw.jpeg tmp/m3tail_draw.jpeg >/dev/null 2>&1 || true
    [ -s tmp/m3tail_draw.jpeg ] && echo "screenshot saved: tmp/m3tail_draw.jpeg" || echo "note: screenshot not captured"
fi
echo "=== done"
