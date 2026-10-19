#!/bin/bash
# CollectionView 虚拟化（NodeAdapter）uitest 验证：
# 初始物化仅可见范围（非 200 全量）→ 内部滚动后条目索引前进（按需物化）→ 进程存活
# 用法：MSYS_NO_PATHCONV=1 bash scripts/verify-cv-virtualization.sh [target]
set -euo pipefail
TARGET="${1:-127.0.0.1:5555}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
HDC="$(bash "$SCRIPT_DIR/lib/sdk.sh" find-hdc)" || exit 1
t() { "$HDC" -t "$TARGET" "$@"; }
FAILED=0
fail() { echo "FAIL: $*"; FAILED=$((FAILED + 1)); }
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
echo "=== 2. 外层滚动到 CollectionView（条目文本出现即到位）"
# 屏幕中线 x=660 会命中 Picker/DatePicker（变成滚轮选择），外层滚动走右边缘；
# DatePicker/TimePicker 展开后很高，'选一个' 可见时 CV 仍在屏外，故以"条目"文本为准
REACHED_STRIP=0
for i in $(seq 1 12); do
    t shell uitest uiInput swipe 1250 2000 1250 500 400 >/dev/null 2>&1; sleep 1
    if snap | grep -q "条目"; then REACHED_STRIP=1; echo "scrolled to CV strip (pass $i)"; break; fi
done
[ "$REACHED_STRIP" -eq 1 ] || fail "CollectionView 条目在 12 次滚动后仍不可见"
echo "=== 3. 初始物化统计（期望：远小于 200，仅可见范围）"
STATS="$(cvitems)"
echo "$STATS"
INIT_COUNT="$(sed -n 's/.*count=\([0-9-]*\).*/\1/p' <<< "$STATS")"
INIT_MAX="$(sed -n 's/.*max=\([0-9-]*\).*/\1/p' <<< "$STATS")"
[ "${INIT_COUNT:-0}" -gt 0 ] || fail "初始未物化任何条目"
[ "${INIT_COUNT:-0}" -lt 200 ] || fail "初始物化了全部 200 条（未虚拟化）"
echo "initial materialized=$INIT_COUNT maxIndex=$INIT_MAX"
# 由 dump 计算 CV 条目带纵范围，内部滚动落在条带内（不写死坐标，布局变了也能跑）
cvswipe() {
    YC=$(snap | python -c "
import sys, re
bys = []
for line in sys.stdin:
    m = re.search(r'\[(\d+),(\d+)\]\[(\d+),(\d+)\].*条目 \d+', line)
    if m: bys += [int(m.group(2)), int(m.group(4))]
print((min(bys)+max(bys))//2 if bys else 0)")
    [ "$YC" != "0" ] || { echo "FAIL: CV strip not found in dump"; exit 1; }
    y2=$((YC-250)); [ $y2 -lt 700 ] && y2=700
    t shell uitest uiInput swipe 660 $YC 660 $y2 300 >/dev/null 2>&1; sleep 2
}
echo "=== 4. 内部滚动（在 CollectionView 区域上滑）"
cvswipe
AFTER1="$(cvitems)"
echo "$AFTER1"
MAX1="$(sed -n 's/.*max=\([0-9-]*\).*/\1/p' <<< "$AFTER1")"
[ "${MAX1:-0}" -gt "${INIT_MAX:-0}" ] || fail "内部滚动后最大条目索引未前进（before=$INIT_MAX after=$MAX1）"
cvswipe
AFTER2="$(cvitems)"
echo "$AFTER2"
MAX2="$(sed -n 's/.*max=\([0-9-]*\).*/\1/p' <<< "$AFTER2")"
[ "${MAX2:-0}" -ge "${MAX1:-0}" ] || fail "第二次滚动后条目索引回退（$MAX1 -> $MAX2）"
echo "=== 5. 再滚一次后统计（期望：max 索引继续前进）"
cvitems
echo "=== 6. 进程存活"
PS_OUT="$(t shell "ps -ef | grep harmonyhost | grep -v grep | head -2" 2>/dev/null || true)"
[ -n "$PS_OUT" ] || fail "harmonyhost 进程不存在"
echo "$PS_OUT"
if [ "$FAILED" -ne 0 ]; then
    echo "=== FAILED: $FAILED assertion(s)"
    exit 1
fi
echo "=== DONE"
