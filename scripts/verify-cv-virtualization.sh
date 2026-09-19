#!/bin/bash
# CollectionView 虚拟化（NodeAdapter）uitest 验证：
# 初始物化仅可见范围（非 200 全量）→ 内部滚动后条目索引前进（按需物化）→ 进程存活
# 用法：MSYS_NO_PATHCONV=1 bash scripts/verify-cv-virtualization.sh [target]
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
snap() { # dumpLayout → 提取文本节点（text + bounds）
    t shell uitest dumpLayout -p /data/local/tmp/cv.json >/dev/null 2>&1
    t file recv /data/local/tmp/cv.json tmp/cv.json >/dev/null 2>&1
    python -c "
import json,sys
d=json.load(open('tmp/cv.json',encoding='utf-8'))
def walk(n):
    a=n.get('attributes',{})
    if a.get('text'): print(a.get('bounds'),repr(a['text']))
    for c in n.get('children',[]): walk(c)
walk(d)"
}
cvitems() { # 只统计 条目 N 文本，输出条数 + 最小/最大索引
    snap | python -c "
import sys,re
idx=[]
for line in sys.stdin:
    m=re.search(r'条目 (\d+)',line)
    if m: idx.append(int(m.group(1)))
print('count=%d min=%d max=%d'%(len(idx),min(idx) if idx else -1,max(idx) if idx else -1))"
}
click() { # 按文本找中心点点击
    t shell uitest dumpLayout -p /data/local/tmp/cv.json >/dev/null 2>&1
    t file recv /data/local/tmp/cv.json tmp/cv.json >/dev/null 2>&1
    COORD=$(python -c "
import json,re,sys
want=sys.argv[1]
d=json.load(open('tmp/cv.json',encoding='utf-8'))
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
echo "=== 1. push Controls demo"
sleep 3
click "Controls demo (10 widgets)"; sleep 2
echo "=== 2. 外层滚动到 CollectionView（Picker '选一个' 标题可见即到位）"
for i in 1 2 3 4 5 6 7 8; do
    t shell uitest uiInput swipe 660 2000 660 500 400 >/dev/null 2>&1; sleep 1
    if snap | grep -q "选一个"; then echo "scrolled to picker area (pass $i)"; break; fi
done
echo "=== 3. 初始物化统计（期望：远小于 200，仅可见范围）"
cvitems
echo "=== 4. 内部滚动（在 CollectionView 区域上滑）"
t shell uitest uiInput swipe 660 1500 660 700 400 >/dev/null 2>&1; sleep 2
cvitems
t shell uitest uiInput swipe 660 1500 660 700 400 >/dev/null 2>&1; sleep 2
echo "=== 5. 再滚一次后统计（期望：max 索引继续前进）"
cvitems
echo "=== 6. 进程存活"
t shell "ps -ef | grep harmonyhost | grep -v grep | head -2"
echo "=== DONE"
