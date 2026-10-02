"""中间模型：libclang 提取结果的规范化表示。

"C 长什么样" 的事实只进本模型；"C# 该怎么写" 的决定全部由
semantics.yaml 的规则 + emit_csharp 的类型映射完成。
"""
from __future__ import annotations

from dataclasses import dataclass, field


# ── 类型树 ──────────────────────────────────────────────────────────────

@dataclass
class CType:
    """规范化 C 类型引用。

    kind:
      builtin   — name ∈ C 基元拼写（int32_t / float / bool / void / size_t …）
      pointer   — pointee 指向
      fnptr     — 函数指针（args/ret）
      enum      — name 为 C 枚举名
      struct    — name 为 struct 名（有定义的结构体值）
      opaque    — name 为不透明记录名（头文件仅有前置声明）
      alias     — typedef 指针别名（如 ArkUI_NodeHandle），name 为别名拼写
      array     — pointee 为元素类型，count 为元素数（定长内联数组）
    """
    kind: str
    name: str = ""
    pointee: "CType | None" = None
    args: "list[CType] | None" = None
    ret: "CType | None" = None
    count: int | None = None


# ── 声明模型 ─────────────────────────────────────────────────────────────

@dataclass
class EnumMember:
    name: str
    value: int
    doc: str = ""          # 头文件注释原文（提取 @since 等）


@dataclass
class EnumModel:
    name: str              # typedef 名；匿名枚举归一为其 typedef 名
    members: list[EnumMember] = field(default_factory=list)
    header: str = ""


@dataclass
class FieldModel:
    name: str
    ctype: CType


@dataclass
class StructModel:
    name: str
    fields: list[FieldModel] = field(default_factory=list)
    is_union: bool = False
    header: str = ""

    @property
    def is_function_table(self) -> bool:
        return any(f.ctype.kind == "fnptr" for f in self.fields)


@dataclass
class FunctionModel:
    name: str
    ret: CType = None        # type: ignore[assignment]
    args: list[FieldModel] = field(default_factory=list)
    header: str = ""


@dataclass
class Model:
    enums: dict[str, EnumModel] = field(default_factory=dict)
    structs: dict[str, StructModel] = field(default_factory=dict)
    functions: dict[str, FunctionModel] = field(default_factory=dict)
    opaque: dict[str, str] = field(default_factory=dict)    # name -> header
    aliases: dict[str, CType] = field(default_factory=dict)  # typedef 指针别名 -> 指向

    def merge(self, other: "Model") -> None:
        for coll, other_coll in (
            (self.enums, other.enums),
            (self.structs, other.structs),
            (self.functions, other.functions),
            (self.opaque, other.opaque),
            (self.aliases, other.aliases),
        ):
            for k, v in other_coll.items():
                coll.setdefault(k, v)
