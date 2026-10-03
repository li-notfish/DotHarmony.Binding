"""A2 验收测试：代码目标发射（结构体/函数表/PInvoke）+ 逐签名 diff 工具。
自包含：fixture 头文件 + 内联 target/语义配置，不依赖真实 SDK。"""
from pathlib import Path
from types import SimpleNamespace

from arkui_bindgen.diff import diff_texts, inventory
from arkui_bindgen.emit_target import emit_code_target
from arkui_bindgen.parse import parse_headers

FIX = Path(__file__).parent / "fixtures"

SEM_RAW = {
    "bool": {"unmanaged_fnptr": "byte", "pinvoke": "marshal_u1"},
    "alias_overrides": {"Mini_ContextHandle": "IntPtr"},
    "handwritten_aliases": [],
    "defaults": {"opaque_value": "IntPtr", "opaque_pointer": "IntPtr"},
}

TGT = {
    "file": "MiniApi.g.cs",
    "types": ["Mini_Callback", "Mini_Complete"],
    "fn_tables": ["Mini_Api_1"],
    "functions": ["OH_Mini_Create", "OH_Mini_SetFlag", "OH_Mini_SetCurve"],
    "container": {"class": "MiniApi", "lib_const": "MiniLib", "visibility": "internal"},
    "per_type": {
        "Mini_Callback": {"field_case": "pascal"},
        "Mini_Complete": {"field_case": "pascal"},
    },
    "per_fn": {"OH_Mini_SetFlag": {"doc": "/// <summary>flag doc</summary>"}},
}


def _emit():
    model = parse_headers([FIX / "mini_animate.h"], [FIX])
    return emit_code_target("mini", TGT, model, SimpleNamespace(_raw=SEM_RAW))


def _emit_guarded(tgt_extra: dict, sem_extra: dict | None = None):
    model = parse_headers([FIX / "mini_animate.h"], [FIX])
    tgt = {**TGT, **tgt_extra}
    raw = {**SEM_RAW, **(sem_extra or {})}
    return emit_code_target("mini", tgt, model, SimpleNamespace(_raw=raw))


def test_tfm_guard_default_off():
    # 未声明 tfm_guard：产物无包裹（现状口径，check/diff 零漂移）
    text = _emit()
    assert "#if " not in text
    assert "#endif" not in text


def test_tfm_guard_wraps_body_with_default_constant():
    text = _emit_guarded({"tfm_guard": True})
    i_comment = text.index("// 来源:")
    i_if = text.index("#if HARMONYOS")
    i_nullable = text.index("#nullable enable")
    assert i_comment < i_if < i_nullable
    assert text.rstrip().endswith("#endif // HARMONYOS")
    # 包裹不改变签名清单：与未包裹产物逐签名一致
    assert diff_texts(text, _emit()) == []


def test_tfm_guard_constant_name_from_semantics():
    text = _emit_guarded({"tfm_guard": True}, {"tfm_guard": "HARMONYOS_NEXT"})
    assert "#if HARMONYOS_NEXT" in text
    assert text.rstrip().endswith("#endif // HARMONYOS_NEXT")


def test_struct_fields_and_case():
    text = _emit()
    assert "internal unsafe struct Mini_Callback" in text
    assert "    public void* UserData;" in text
    assert "    public delegate* unmanaged<void*, void> Callback;" in text


def test_struct_member_order_preserved():
    text = _emit()
    i_type = text.index("public Mini_CallbackType Type;")
    i_cb = text.index("public delegate* unmanaged<void*, void> Callback;", i_type)
    i_ud = text.index("public void* UserData;", i_cb)
    assert i_type < i_cb < i_ud


def test_fn_table_signature_mapping():
    text = _emit()
    # 别名→IntPtr；不透明指针→IntPtr；已发射结构体指针保留名字；int32_t→int
    assert ("public delegate* unmanaged<IntPtr, IntPtr, Mini_Callback*, Mini_Complete*, int> doIt;"
            in text)
    assert "public delegate* unmanaged<IntPtr, void> dispose;" in text


def test_pinvoke_block():
    text = _emit()
    assert "internal static unsafe partial class MiniApi" in text
    assert "    internal static partial IntPtr OH_Mini_Create();" in text
    # bool 在 PInvoke 走 MarshalAs(U1)
    assert ("internal static partial void OH_Mini_SetFlag(IntPtr opt, "
            "[MarshalAs(UnmanagedType.U1)] bool end);") in text
    # 枚举参数按其 C# 名发射
    assert ("internal static partial void OH_Mini_SetCurve(IntPtr opt, Mini_CallbackType value);"
            in text)
    # per_fn doc 在 [LibraryImport] 之前
    i_doc = text.index("/// <summary>flag doc</summary>")
    i_lib = text.index("[LibraryImport(MiniLib)]", i_doc)
    assert i_doc < i_lib


def test_diff_zero_and_nonzero():
    text = _emit()
    assert diff_texts(text, text) == []
    # 成员错位 = ABI 漂移，必须报出
    broken = text.replace(
        "public void* UserData;\n    public delegate* unmanaged<void*, void> Callback;",
        "public delegate* unmanaged<void*, void> Callback;\n    public void* UserData;")
    probs = diff_texts(broken, text)
    assert probs and any("Mini_Callback" in p for p in probs)


def test_diff_scope_missing_reported():
    text = _emit()
    no_table = text.replace("struct Mini_Api_1", "struct Mini_Api_1_RENAMED")
    probs = diff_texts(no_table, text)
    assert any("Mini_Api_1" in p for p in probs)


def test_inventory_ignores_non_signature_members():
    # 懒获取属性/私有字段不参与签名比对（手写文件保留逻辑代码时不误报）
    legacy = (
        "internal static unsafe partial class MiniApi\n{\n"
        "    private static Mini_Api_1* _api;\n"
        "    internal static Mini_Api_1* Api\n"
        "    {\n        get { return _api; }\n    }\n"
        "    [LibraryImport(MiniLib)]\n"
        "    internal static partial IntPtr OH_Mini_Create();\n"
        "}\n"
    )
    inv = inventory(legacy)
    cls = next(s for s in inv if s.name == "MiniApi")
    assert cls.members == ["IntPtr OH_Mini_Create()"]
