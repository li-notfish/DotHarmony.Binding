#!/usr/bin/env python3
"""从 HarmonyOS NDK 头文件提取 ArkUI 枚举，生成 C# 绑定。

用法:
    python src/nativeBinding/extract_arkui_types.py [--sdk <path>] [--out <path>]

SDK 探测顺序: --sdk 参数 → OHOS_SDK_BASE → OHSDK_HOME → 本机默认路径。
仅提取枚举；结构体与函数表镜像手工维护（需要精确布局控制）。
"""

import argparse
import io
import re
import sys
import os
from pathlib import Path

SDK_HOME = os.getenv("OHOS_SDK_BASE") or os.getenv("OHSDK_HOME")
if SDK_HOME is None:
    # 回退默认值
    SDK_HOME = r"D:/Harmony/OpenHarmony/Sdk"


def _default_sdk() -> str:
    """按优先级探测 ArkUI 头文件目录：SDK_HOME/版本号 → SDK_HOME/openharmony → SDK_HOME 根 → DevEco 内置"""
    candidates = []
    if SDK_HOME:
        candidates += [
            os.path.join(SDK_HOME, "26.0.0", "native", "sysroot", "usr", "include", "arkui"),
            os.path.join(SDK_HOME, "openharmony", "native", "sysroot", "usr", "include", "arkui"),
            os.path.join(SDK_HOME, "native", "sysroot", "usr", "include", "arkui"),
        ]
    candidates.append("C:/Program Files/Huawei/DevEco Studio/sdk/default/openharmony/native/sysroot/usr/include/arkui")
    for c in candidates:
        if os.path.isfile(os.path.join(c, "native_node.h")):
            return c
    raise SystemExit(
        "error: ArkUI headers not found. Set --sdk / OHOS_SDK_BASE / OHSDK_HOME "
        f"(tried: {', '.join(candidates)})")


DEFAULT_SDK = _default_sdk()

DEFAULT_OUT = "HarmonyOS.Bindings/NativeNode/ArkUINodeTypes.g.cs"

# node 层核心枚举（native_node.h / native_node_napi.h / native_animate.h / native_type_visual.h）
CORE_ENUMS = [
    "ArkUI_NodeType",
    "ArkUI_NodeAttributeType",
    "ArkUI_NodeEventType",
    "ArkUI_NodeDirtyFlag",
    "ArkUI_NodeAdapterEventType",
    "ArkUI_NodeContentEventType",
    "ArkUI_LengthMetricUnit",
    # 动画层（ArkUIAnimateApi.cs 手工函数表消费）
    "ArkUI_AnimationCurve",
    "ArkUI_FinishCallbackType",
]

HEADER_FILES = ["native_node.h", "native_type.h", "native_node_napi.h", "common_type.h",
                "native_animate.h", "native_type_visual.h"]
ATTRIBUTES_SUBDIR = "node_attributes"

ENUM_RE = re.compile(r"typedef\s+enum\s*(?::\s*\w+)?\s*\{(.*?)\}\s*(\w+)\s*;", re.S)
MEMBER_RE = re.compile(r"(\w+)\s*(?:=\s*([^,\n]+))?\s*,")
DEFINE_RE = re.compile(r"#define\s+(\w+)\s+([^\n]+)")


def strip_comments(src: str) -> str:
    src = re.sub(r"/\*.*?\*/", "", src, flags=re.S)
    src = re.sub(r"//[^\n]*", "", src)
    return src


def eval_expr(expr: str, defines: dict, depth: int = 0) -> int:
    expr = expr.strip()
    if not expr:
        raise ValueError("empty enum expression")
    if depth > 8:
        raise ValueError(f"define expansion too deep: {expr!r}")
    # 展开已知的 #define 常量
    for name in set(re.findall(r"\b[A-Z_][A-Z0-9_]*\b", expr)):
        if name in defines:
            expr = re.sub(r"\b%s\b" % re.escape(name), "(%s)" % defines[name], expr)
    if not re.fullmatch(r"[-+~*/%()<>|\s\dxXa-fA-F_0-9]+", expr):
        raise ValueError(f"unsupported enum expression: {expr!r}")
    return int(eval(expr, {"__builtins__": {}}, {}))


def parse_defines(src: str) -> dict:
    defines = {}
    for name, value in DEFINE_RE.findall(src):
        value = value.strip()
        if not re.fullmatch(r"[-+~()<>|\s\dxXa-fA-F_0-9]+", value):
            continue
        try:
            defines[name] = str(eval_expr(value, defines))
        except (ValueError, SyntaxError):
            continue
    return defines


def parse_enums(header_path: Path, shared_env: dict) -> dict:
    src = strip_comments(header_path.read_text(encoding="utf-8", errors="replace"))
    defines = dict(shared_env)
    defines.update(parse_defines(src))
    enums = {}
    for body, name in ENUM_RE.findall(src):
        members = []
        next_auto = 0
        for m in MEMBER_RE.finditer(body + ","):
            member, value = m.group(1), m.group(2)
            if member in ("__attribute__",):
                continue
            if value is not None:
                next_auto = eval_expr(value, defines)
            members.append((member, next_auto))
            defines[member] = str(next_auto)
            next_auto += 1
        enums[name] = members
    shared_env.clear()
    shared_env.update(defines)
    return enums


def pascal(name: str) -> str:
    return "".join(p.capitalize() or "_" for p in name.lower().split("_"))


def render_enum(name: str, members) -> str:
    lines = [f"public enum {name}", "{"]
    seen = set()
    for member, value in members:
        cs = member
        if cs in seen:
            continue
        seen.add(cs)
        lines.append(f"    {cs} = {value},")
    lines.append("}")
    return "\n".join(lines)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--sdk", default=DEFAULT_SDK)
    ap.add_argument("--out", default=DEFAULT_OUT)
    ap.add_argument("--dump-json", dest="dump_json", default=None,
                    help="同时输出 {枚举名: {成员: 值}} 的 JSON，供解析器生成器消费")
    args = ap.parse_args()

    sdk = Path(args.sdk)
    all_enums: dict = {}
    shared_env: dict = {}

    attr_dir = sdk / ATTRIBUTES_SUBDIR
    attr_files = sorted(attr_dir.glob("*.h")) if attr_dir.exists() else []

    print(f"Using SDK: {sdk}", file=sys.stderr)
    for h in HEADER_FILES + [str(p) for p in attr_files]:
        path = Path(h) if Path(h).is_absolute() else sdk / h
        print(f"Checking: {path} -> exists? {path.exists()}", file=sys.stderr)

    if not attr_files:
        print(f"warn: attribute headers not found under {attr_dir}", file=sys.stderr)

    for header in HEADER_FILES + [str(p) for p in attr_files]:
        path = Path(header) if Path(header).is_absolute() else sdk / header
        if not path.exists():
            print(f"warn: header not found: {path}", file=sys.stderr)
            continue
        for name, members in parse_enums(path, shared_env).items():
            all_enums.setdefault(name, members)

    # 记录 node_attributes 子目录产生的枚举名（全收）
    attr_enum_names: set = set()
    if attr_dir.exists():
        attr_env: dict = dict(shared_env)
        for p in attr_files:
            for name in parse_enums(p, attr_env):
                attr_enum_names.add(name)

    wanted = set(CORE_ENUMS) | attr_enum_names

    out_lines = [
        "// <auto-generated>由 src/nativeBinding/extract_arkui_types.py 从 HarmonyOS NDK 头文件生成，请勿手工编辑</auto-generated>",
        f"// 源头文件: {', '.join(HEADER_FILES + [ATTRIBUTES_SUBDIR + '/*.h'])}",
        "#nullable enable",
        "",
        "namespace HarmonyOS.Bindings.NativeNode;",
        "",
    ]
    count = 0
    for name in sorted(wanted):
        if name not in all_enums:
            continue
        out_lines.append(render_enum(name, all_enums[name]))
        out_lines.append("")
        count += 1

    out = Path(args.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    io.open(out, "w", encoding="utf-8", newline="\n").write("\n".join(out_lines))
    print(f"generated {count} enums -> {out}")

    if args.dump_json:
        import json
        dump = {name: {m: v for m, v in members} for name, members in all_enums.items()}
        json_path = Path(args.dump_json)
        json_path.parent.mkdir(parents=True, exist_ok=True)
        io.open(json_path, "w", encoding="utf-8", newline="\n").write(
            json.dumps(dump, indent=0, sort_keys=True))
        print(f"dumped enum metadata -> {json_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
