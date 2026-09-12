#!/bin/bash
# M1 收尾 uitest 验证：NavigationPage 标题栏/返回动画、Grid 单元对齐、ZIndex、Auto 轨道
# 用法：MSYS_NO_PATHCONV=1 bash scripts/verify-m1-uitest.sh [target]
set -e
TARGET="${1:-127.0.0.1:5555}"
HDC=""
for base in "$OHOS_SDK_BASE" "D:/Harmony/OpenHarmony/Sdk" "D:/Program Files/Huawei/DevEco Studio/sdk/default/openharmony"; do
    [ -n "$base" ] || continue
    for rel in "26.0.0/toolchains/hdc.exe" "toolchains/hdc.exe"; do
        [ -f "$base/$rel" ] && HDC="$base/$rel" && break 2
    done
done
[ -n "$HDC" ] || { echo "hdc not found"; exit 1; }
t() { "$HDC" -t "$TARGET" "$@"; }
snap() { # dumpLayout → 提取文本节点 bounds
    t shell uitest dumpLayout -p /data/local/tmp/m1.json >/dev/null 2>&1
    t file recv /data/local/tmp/m1.json tmp/m1.json >/dev/null 2>&1
    python -c "
import json,sys
d=json.load(open('tmp/m1.json',encoding='utf-8'))
def walk(n):
    a=n.get('attributes',{})
    if a.get('text'): print(a.get('bounds'),repr(a['text']))
    for c in n.get('children',[]): walk(c)
walk(d)"
}
click() { # 按文本找中心点点击
    t shell uitest dumpLayout -p /data/local/tmp/m1.json >/dev/null 2>&1
    t file recv /data/local/tmp/m1.json tmp/m1.json >/dev/null 2>&1
    COORD=$(python -c "
import json,re,sys
want=sys.argv[1]
d=json.load(open('tmp/m1.json',encoding='utf-8'))
def walk(n):
    a=n.get('attributes',{})
    if a.get('text','')==want:
        b=[int(x) for x in re.findall(r'-?\d+',a['bounds'])]
        print((b[0]+b[2])//2,(b[1]+b[3])//2); return True
    for c in n.get('children',[]):
        if walk(c): return True
walk(d)" "$1")
    [ -n "$COORD" ] || { echo "FAIL: text not found: $1"; exit 1; }
    t shell uitest uiInput click $COORD >/dev/null 2>&1
}
echo "=== 0. 首页工具栏（标题 Home，栈深1无返回键）"
sleep 3; snap | head -8
echo "=== 1. push Second → 标题 Second + 返回键出现"
click "Open second page"; sleep 2; snap | head -6
echo "=== 2. 工具栏 ← 返回（返回动画）"
click "←"; sleep 2; snap | grep -m1 "Hello XAML" && echo "back-to-home OK"
echo "=== 3. push Layout Demo"
click "Layout demo (Grid + Absolute)"; sleep 2
echo "=== 4. Grid 单元对齐（Start 靠左 / Center 居中 / End 靠右）"
snap | grep -E "Start|Center|End"
echo "=== 5. Auto 轨道：Grow text 两次"
click "Grow text"; sleep 1; click "Grow text"; sleep 2
snap | grep "growing" | tail -1
echo "=== 6. ZIndex 切换"
click "Toggle blue ZIndex"; sleep 1; snap | grep "ZIndex ="
echo "=== 7. 滚动到底后 Pop back 按钮返回"
t shell uitest uiInput swipe 660 2000 660 600 500 >/dev/null; sleep 1
t shell uitest uiInput swipe 660 2000 660 600 500 >/dev/null; sleep 2
click "Pop back"; sleep 2; snap | grep -m1 "Hello XAML" && echo "pop-back OK"
echo "=== 8. 进程存活"
t shell "ps -ef | grep harmonyhost | grep -v grep | head -2"
echo "=== DONE"
