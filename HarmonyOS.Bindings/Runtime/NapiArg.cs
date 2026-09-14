using System;
using System.Runtime.InteropServices;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// napi 调用参数的 union struct（完全零装箱改造，ROADMAP 2.10 剩余分配源收口）。
/// params ReadOnlySpan<object?> 封送在调用点对基元装箱（int→object 堆分配）；
/// 本结构以 标签+值域 承载基元（栈上传入、字段赋值零分配），引用类型存引用字段
/// （string/JsObject/record 本就是引用类型，不产生额外装箱）。
/// 枚举经 Enum 隐式转换走 Ref 路径（Convert.ToInt32(Enum) 单次装箱）——生成调用点
/// 对枚举参数强转 int 后走 Int 路径零装箱；手写代码保留 Enum 兜底。
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly struct NapiArg
{
    /// <summary>值域标签：Null 空值；Number double/float；Int 整数；Bool；Native 句柄直传；Ref 引用类型</summary>
    public enum Tag : byte { Null, Number, Int, Bool, Native, Ref }

    public readonly Tag Kind;
    /// <summary>Number 值域（double/float）</summary>
    public readonly double Number;
    /// <summary>Int/Bool/Native 值域（long、uint、bool 位、IntPtr.ToInt64）</summary>
    public readonly long Integer;
    /// <summary>Ref 值域（string/JsObject/INapiRecord/Delegate/Enum/object）</summary>
    public readonly object? RefValue;

    private NapiArg(Tag kind, double num = 0, long integer = 0, object? refValue = null)
    {
        Kind = kind;
        Number = num;
        Integer = integer;
        RefValue = refValue;
    }

    public static implicit operator NapiArg(double v) => new(Tag.Number, num: v);
    public static implicit operator NapiArg(float v) => new(Tag.Number, num: v);
    public static implicit operator NapiArg(int v) => new(Tag.Int, integer: v);
    public static implicit operator NapiArg(uint v) => new(Tag.Int, integer: v);
    public static implicit operator NapiArg(long v) => new(Tag.Int, integer: v);
    public static implicit operator NapiArg(ulong v) => new(Tag.Int, integer: unchecked((long)v));
    public static implicit operator NapiArg(short v) => new(Tag.Int, integer: v);
    public static implicit operator NapiArg(ushort v) => new(Tag.Int, integer: v);
    public static implicit operator NapiArg(sbyte v) => new(Tag.Int, integer: v);
    public static implicit operator NapiArg(byte v) => new(Tag.Int, integer: v);
    public static implicit operator NapiArg(bool v) => new(Tag.Bool, integer: v ? 1 : 0);
    public static implicit operator NapiArg(IntPtr v) => new(Tag.Native, integer: v.ToInt64());
    public static implicit operator NapiArg(string? s) => new(Tag.Ref, refValue: s);
    public static implicit operator NapiArg(JsObject? j) => new(Tag.Ref, refValue: j);
    public static implicit operator NapiArg(Delegate? d) => new(Tag.Ref, refValue: d);
    public static implicit operator NapiArg(Enum? e) => new(Tag.Ref, refValue: e);

    /// <summary>object 类型值的显式工厂（C# 禁止基类/接口自定义转换——object/INapiRecord
    /// 类型的调用点经此入 Ref 路径，运行时 From(object) 分派 INapiRecord/IDictionary）</summary>
    public static NapiArg Of(object? o) => new(Tag.Ref, refValue: o);
}
