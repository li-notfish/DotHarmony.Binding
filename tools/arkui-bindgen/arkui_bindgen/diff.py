"""逐签名比对：生成物 vs 手写快照（A2 迁移验收，后续 SDK 升级差异定位同样受用）。

签名定义（注释/文档行忽略）：
  struct 成员   public <type> <name>;                          → "<type> <name>"
  PInvoke       <vis> static partial <ret> <name>(<args>);     → "<ret> <name>(<args>)"
  enum 成员     <NAME> = <expr>,                               → "<NAME> = <int/expr>"

比对为"按作用域保序比对"：函数表成员顺序属于 ABI，错位即差异；
作用域（struct/class/enum）的出现次序不参与比对（生成物与手写文件布局可不同）。
"""
from __future__ import annotations

import re
from dataclasses import dataclass, field

_SCOPE_RE = re.compile(
    r"^\s*(?:\[[^\]]+\]\s*)*"
    r"(?:(?:public|internal|private|protected)\s+)?"
    r"(?:static\s+)?(?:unsafe\s+)?(?:partial\s+)?(?:readonly\s+)?"
    r"(struct|class|enum)\s+(\w+)")
_FIELD_RE = re.compile(r"^\s*(?:\[[^\]]+\]\s*)*public\s+(.+?)\s+(@?\w+);$")
_PINVOKE_RE = re.compile(
    r"^\s*(?:internal|private|public)\s+static\s+partial\s+(.+?)\s+(\w+)\s*\((.*)\)\s*;$")
_ENUM_MEMBER_RE = re.compile(r"^\s*(\w+)\s*=\s*(.+?),$")
_MEMBER_START_RE = re.compile(r"^\s*(?:\[[^\]]+\]\s*)*(?:public|internal|private)\s+\S")


def _merge_multiline_members(lines: list[str]) -> list[str]:
    """把跨行成员声明并成逻辑单行（如 delegate* 参数表折断、PInvoke 返回类型换行）。

    规则：可见性开头的行若无 ';' 且无花括号且非作用域声明，则并吞后续行，
    直到遇见 ';'；遇花括号行即停（方法体不并吞，保持行级深度账正确）。
    """
    out: list[str] = []
    buf = ""
    for line in lines:
        if buf:
            if ";" in line or "{" in line or "}" in line:
                buf += " " + line.strip()
                out.append(buf)
                buf = ""
            else:
                buf += " " + line.strip()
            continue
        if (_MEMBER_START_RE.match(line) and not _SCOPE_RE.match(line)
                and ";" not in line and "{" not in line and "}" not in line):
            buf = line.rstrip()
        else:
            out.append(line)
    if buf:
        out.append(buf)
    return out


def _strip_comments(text: str) -> list[str]:
    """整行 // / /// 注释剔除 + 块注释擦除，剩余内容供签名扫描。"""
    lines: list[str] = []
    in_block = False
    for raw in text.splitlines():
        s = raw
        if in_block:
            end = s.find("*/")
            if end < 0:
                continue
            s = s[end + 2:]
            in_block = False
        while "/*" in s:
            start = s.find("/*")
            end = s.find("*/", start)
            if end < 0:
                s = s[:start]
                in_block = True
                break
            s = s[:start] + s[end + 2:]
        cut = s.find("//")
        if cut >= 0:
            s = s[:cut]
        lines.append(s)
    return lines


def _norm(sig: str) -> str:
    s = re.sub(r"\s+", " ", sig).strip()
    s = re.sub(r"\s*,\s*", ", ", s)
    s = re.sub(r"\s*\*", "*", s)
    s = re.sub(r"\(\s+", "(", s)
    s = re.sub(r"\s+\)", ")", s)
    return s


def _norm_enum_value(expr: str) -> str:
    try:
        return str(int(expr.strip(), 0))
    except ValueError:
        return expr.strip()


@dataclass
class Scope:
    kind: str
    name: str
    members: list[str] = field(default_factory=list)


def inventory(text: str) -> list[Scope]:
    """C# 源文本 → 作用域保序签名清单。非签名成员（属性/方法体等）天然不匹配正则，自动忽略。"""
    scopes: list[Scope] = []
    stack: list[tuple[Scope, int]] = []
    pending: Scope | None = None
    depth = 0
    for line in _merge_multiline_members(_strip_comments(text)):
        m = _SCOPE_RE.match(line)
        if m:
            pending = Scope(m.group(1), m.group(2))
        cur = stack[-1][0] if stack else None
        if cur is not None:
            if cur.kind == "struct":
                fm = _FIELD_RE.match(line)
                if fm:
                    cur.members.append(_norm(f"{fm.group(1)} {fm.group(2)}"))
            elif cur.kind == "class":
                pm = _PINVOKE_RE.match(line)
                if pm:
                    cur.members.append(_norm(f"{pm.group(1)} {pm.group(2)}({pm.group(3)})"))
            elif cur.kind == "enum":
                em = _ENUM_MEMBER_RE.match(line)
                if em:
                    cur.members.append(f"{em.group(1)} = {_norm_enum_value(em.group(2))}")
        for ch in line:
            if ch == "{":
                depth += 1
                if pending is not None:
                    stack.append((pending, depth))
                    scopes.append(pending)
                    pending = None
            elif ch == "}":
                depth -= 1
                while stack and stack[-1][1] > depth:
                    stack.pop()
    return scopes


def diff_texts(generated: str, legacy: str, ignore_scopes: set[str] | None = None) -> list[str]:
    """返回差异描述列表；空列表 = 逐签名零差异。

    ignore_scopes：手写文件中保留的托管包装/私有枚举等作用域，不参与比对
    （如 ArkUI_NodeHandle 句柄包装、ArkUIVariantKind 私有枚举）。
    """
    skip = ignore_scopes or set()
    gen = [s for s in inventory(generated) if s.name not in skip]
    leg = [s for s in inventory(legacy) if s.name not in skip]
    problems: list[str] = []

    gen_map = {s.name: s for s in gen}
    leg_map = {s.name: s for s in leg}

    for name in leg_map:
        if name not in gen_map:
            problems.append(f"scope 缺失（生成物无 {leg_map[name].kind} {name}）")
    for name in gen_map:
        if name not in leg_map:
            problems.append(f"scope 多余（手写快照无 {gen_map[name].kind} {name}）")

    for name in gen_map.keys() & leg_map.keys():
        g, l = gen_map[name].members, leg_map[name].members
        if gen_map[name].kind == "enum":
            # 枚举成员顺序无 ABI 语义（值才有），按集合比对
            if sorted(g) == sorted(l):
                continue
            for m in sorted(set(l) - set(g)):
                problems.append(f"{name}: 生成物缺少枚举成员 '{m}'")
            for m in sorted(set(g) - set(l)):
                problems.append(f"{name}: 生成物多出枚举成员 '{m}'")
            continue
        if g == l:
            continue
        for i in range(max(len(g), len(l))):
            gs = g[i] if i < len(g) else "<无>"
            ls = l[i] if i < len(l) else "<无>"
            if gs != ls:
                problems.append(f"{name}[{i}]: 生成='{gs}' 手写='{ls}'")
    return problems
