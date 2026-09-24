"""libclang 解析：ArkUI NDK 头文件 → 中间模型（model.Model）。

只从 config 列出的头文件收割声明（#ifndef 传递 include 进来的轮子一概不采），
生成器不猜测语义：任何命名/布局/单位的纠偏都是 semantics.yaml 的职责。
"""
from __future__ import annotations

from pathlib import Path

from clang import cindex

from .model import CType, EnumMember, EnumModel, FieldModel, FunctionModel, Model, StructModel

_K = cindex.CursorKind
_T = cindex.TypeKind


def _with_arch_include(roots: list[Path]) -> list[Path]:
    """sysroot 的位类型头（bits/alltypes.h）在 <include>/<arch>-linux-ohos 子目录，
    缺了它 int32_t 等会解析失败并连锁把函数指针成员解析成 K&R 残骸。"""
    out: list[Path] = []
    for r in roots:
        out.append(r)
        for sub in sorted(r.glob("*-linux-ohos")):
            if sub.is_dir() and sub not in out:
                out.append(sub)
    return out


# ── 类型规范化 ────────────────────────────────────────────────────────────

def norm_type(t: cindex.Type) -> CType:
    """把 libclang Type 折叠为 CType。typedef/Elaborated 外壳的处理原则：
    保留 typedef 别名拼写（对句柄别名如 ArkUI_NodeHandle 至关重要），
    分类依据看 canonical 类型。"""
    can = t.get_canonical()

    if can.kind == _T.POINTER:
        pointee = t.get_pointee()
        # 函数指针：pointer → function prototype
        # （FUNCTIONNOPROTO = "void (*)()" K&R 无参写法，按零参函数指针处理；
        #  其 argument_types() 不允许调用，只能按零参对待）
        pcan = pointee.get_canonical()
        if pcan.kind == _T.FUNCTIONNOPROTO:
            return CType(kind="fnptr", ret=norm_type(pointee.get_result()), args=[])
        if pcan.kind == _T.FUNCTIONPROTO:
            return CType(
                kind="fnptr",
                ret=norm_type(pointee.get_result()),
                args=[norm_type(a) for a in pointee.argument_types()],
            )
        # typedef 指针别名（ArkUI_NodeHandle 等）：原样保留别名，交由语义配置裁决
        if t.kind in (_T.TYPEDEF, _T.ELABORATED) and t.spelling != can.spelling:
            return CType(kind="alias", name=t.spelling, pointee=norm_type(pointee))
        return CType(kind="pointer", pointee=norm_type(pointee), name=t.spelling)

    if can.kind == _T.CONSTANTARRAY:
        return CType(
            kind="array",
            pointee=norm_type(t.element_type),
            count=t.element_count,
        )

    if can.kind == _T.ENUM:
        name = can.spelling.strip()
        if name.startswith("const "):
            name = name[len("const "):]
        return CType(kind="enum", name=name)

    if can.kind in (_T.RECORD,):
        decl = can.get_declaration()
        name = (t.spelling or can.spelling).strip()
        if name.startswith("const "):
            name = name[len("const "):]
        if not decl.is_definition():
            # 头文件里只有前置声明：不透明类型
            return CType(kind="opaque", name=name)
        return CType(kind="struct", name=name)

    # 其余一律按基元处理（spelling 保留 typedef 写法如 int32_t）
    name = (t.spelling or can.spelling).strip()
    if name.startswith("const "):
        name = name[len("const "):]
    return CType(kind="builtin", name=name)


# ── 声明收割 ──────────────────────────────────────────────────────────────

def _typedef_name(tu_cursor: cindex.Cursor, target_extent, header: Path) -> str:
    """定位包裹给定源范围的 TYPEDEF_DECL（typedef struct {..} Name / 匿名枚举同理）。"""
    for node in tu_cursor.get_children():
        if node.kind != _K.TYPEDEF_DECL:
            continue
        f = node.location.file
        if f is None or Path(f.name) != header:
            continue
        ut = node.underlying_typedef_type
        decl = ut.get_declaration()
        if decl.extent.start.offset == target_extent.start.offset:
            return node.spelling
    return ""


def parse_header(header: Path, include_roots: list[Path], src_name: str = "") -> Model:
    """解析单个头文件，只收割位于该文件内的声明。

    src_name：模型中记录的源标识（如 "native_node.h" 或 "node_attributes/text.h"），
    供配置按子目录归类（node_attributes/*.h 枚举全收的历史行为）。
    """
    args = ["-x", "c", "-std=c11"]
    for inc in _with_arch_include(include_roots):
        args.append("-I" + str(inc))
    index = cindex.Index.create()
    tu = index.parse(
        str(header),
        args=args,
        options=(
            cindex.TranslationUnit.PARSE_SKIP_FUNCTION_BODIES
            | cindex.TranslationUnit.PARSE_INCOMPLETE
        ),
    )
    model = Model()
    anon_counter = 0

    def belongs(cur: cindex.Cursor) -> bool:
        f = cur.location.file
        return f is not None and Path(f.name) == header

    for node in tu.cursor.get_children():
        if not belongs(node):
            continue

        if node.kind == _K.ENUM_DECL and node.is_definition():
            sp = node.spelling or ""
            name = sp if (sp and "(unnamed" not in sp) else _typedef_name(tu.cursor, node.extent, header)
            if not name:
                # 无 typedef 包装的匿名枚举（ui_input_event.h 中的 UI_TOUCH_EVENT_ACTION 等）：
                # 以 "首成员名" 作稳定键，语义配置经 anon:<header>:<firstMember> 引用
                first = ""
                for c in node.get_children():
                    if c.kind == _K.ENUM_CONSTANT_DECL:
                        first = c.spelling
                        break
                name = f"anon:{header.name}:{first}" if first else f"anon:{header.name}:#{node.extent.start.line}"
            members = []
            for c in node.get_children():
                if c.kind == _K.ENUM_CONSTANT_DECL:
                    members.append(EnumMember(c.spelling, c.enum_value, c.raw_comment or ""))
            model.enums.setdefault(name, EnumModel(name=name, members=members, header=src_name or header.name))

        elif node.kind == _K.TYPEDEF_DECL:
            ut = node.underlying_typedef_type
            decl = ut.get_declaration()
            can = ut.get_canonical()
            if can.kind == _T.RECORD and decl.is_definition() and belongs(decl):
                fields = [
                    FieldModel(f.spelling, norm_type(f.type))
                    for f in decl.get_children()
                    if f.kind == _K.FIELD_DECL
                ]
                is_union = decl.kind == _K.UNION_DECL
                key = node.spelling
                model.structs.setdefault(
                    key, StructModel(name=key, fields=fields, is_union=is_union, header=src_name or header.name)
                )
            elif can.kind == _T.RECORD and not decl.is_definition():
                # typedef struct X X; —— 不透明类型
                model.opaque.setdefault(node.spelling, src_name or header.name)
            elif can.kind == _T.POINTER and can.get_pointee().get_canonical().kind == _T.RECORD:
                # typedef struct X* XHandle; —— 句柄别名
                model.aliases.setdefault(node.spelling, norm_type(ut))

        elif node.kind == _K.FUNCTION_DECL:
            args = [
                FieldModel(p.spelling, norm_type(p.type))
                for p in node.get_children()
                if p.kind == _K.PARM_DECL
            ]
            model.functions.setdefault(
                node.spelling,
                FunctionModel(
                    name=node.spelling,
                    ret=norm_type(node.result_type),
                    args=args,
                    header=src_name or header.name,
                ),
            )

    return model


def parse_headers(headers: list[Path], include_roots: list[Path]) -> Model:
    merged = Model()
    for h in headers:
        merged.merge(parse_header(h, include_roots))
    return merged
