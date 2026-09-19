using System;
using System.Globalization;

namespace HarmonyOS.Bindings.Runtime;

/// <summary>
/// JS bigint 的 C# 载体（long 语义）。为什么不是裸 long：JS 里 number 与 bigint 是两种类型，
/// @ohos API 声明为 bigint 的参数必须经 napi_create_bigint_int64 创建（裸 long 会被创建为 number）。
/// 本结构携带类型信息，运行时据此选择正确的 napi 通道；超出 int64/uint64 范围的值封送时抛异常。
/// </summary>
public readonly struct JsBigInt : IEquatable<JsBigInt>, IFormattable
{
    public long Value { get; }

    public JsBigInt(long value) => Value = value;

    public static implicit operator JsBigInt(long value) => new(value);
    public static implicit operator long(JsBigInt value) => value.Value;

    public bool Equals(JsBigInt other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is JsBigInt other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);
}
