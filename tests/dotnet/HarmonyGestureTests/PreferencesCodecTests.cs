// HarmonyPreferences 的编解码纯逻辑单测：类型标签编码 → 解码 roundtrip。
// 值编码为 "类型标签:载荷" 字符串（b:/i:/f:/d:/s:/t:/o:），语义见 HarmonyPreferences.cs 文件头。
// 关注点：精度（long/DateTime.ToBinary 超 double 2^53 不得走 float/double 路径）、
// 文化不变性（InvariantCulture）、字符串含冒号、外部写入的无标签数据容忍。
using System.Globalization;
using HarmonyOS.Essentials;
using Xunit;

namespace HarmonyGestureTests;

public class PreferencesCodecTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Roundtrip_Bool(bool value)
    {
        var raw = HarmonyPreferences.Encode(value);
        Assert.StartsWith("b:", raw);
        Assert.Equal(value, HarmonyPreferences.Decode(raw, !value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-42)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Roundtrip_Int(int value)
    {
        var raw = HarmonyPreferences.Encode(value);
        Assert.StartsWith("i:", raw);
        Assert.Equal(value, HarmonyPreferences.Decode(raw, 0));
    }

    [Fact]
    public void Roundtrip_Long_PastDoublePrecision()
    {
        // DateTime.ToBinary() 的 tick 数（≈6.4e17）超出 double 2^53 精度：
        // 若编码走了 float/double 路径，roundtrip 必然失真——本测试锁死该回归
        long value = DateTime.Now.ToBinary();
        Assert.True(Math.Abs(value) > 9007199254740992); // 2^53

        var raw = HarmonyPreferences.Encode(value);
        Assert.Equal(value, HarmonyPreferences.Decode(raw, 0L));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(-3.141592653589793)]
    public void Roundtrip_Double(double value)
    {
        var raw = HarmonyPreferences.Encode(value);
        Assert.StartsWith("d:", raw);
        Assert.Equal(value, HarmonyPreferences.Decode(raw, 0.0), 15);
    }

    [Fact]
    public void Roundtrip_Float()
    {
        const float value = 1.5f;
        var raw = HarmonyPreferences.Encode(value);
        Assert.StartsWith("f:", raw);
        Assert.Equal(value, HarmonyPreferences.Decode(raw, 0f));
    }

    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("with:colon:inside")]
    [InlineData("中文值")]
    public void Roundtrip_String(string value)
    {
        var raw = HarmonyPreferences.Encode(value);
        Assert.StartsWith("s:", raw);
        Assert.Equal(value, HarmonyPreferences.Decode(raw, "def"));
    }

    [Fact]
    public void Roundtrip_DateTime()
    {
        var value = new DateTime(2026, 9, 13, 12, 34, 56, DateTimeKind.Local).AddTicks(1234567);
        var raw = HarmonyPreferences.Encode(value);
        Assert.StartsWith("t:", raw);
        Assert.Equal(value, HarmonyPreferences.Decode(raw, DateTime.MinValue));
    }

    [Fact]
    public void Roundtrip_DateTimeOffset()
    {
        var value = new DateTimeOffset(2026, 9, 13, 8, 0, 0, TimeSpan.FromHours(8));
        var raw = HarmonyPreferences.Encode(value);
        Assert.StartsWith("o:", raw);
        Assert.Equal(value, HarmonyPreferences.Decode(raw, DateTimeOffset.MinValue));
    }

    [Fact]
    public void Decode_MissingTag_ReturnsDefault_ForNonString()
    {
        // 外部写入的无标签数据（如裸 "123"）：非 string 类型按 MAUI 语义返回缺省
        Assert.Equal(7, HarmonyPreferences.Decode<int>("123", 7));
        Assert.Equal(true, HarmonyPreferences.Decode<bool>("junk", true));
    }

    [Fact]
    public void Decode_UnknownTag_ReturnsDefault()
    {
        Assert.Equal(9, HarmonyPreferences.Decode<int>("x:whatever", 9));
    }

    [Fact]
    public void Encode_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => HarmonyPreferences.Encode<string?>(null));
    }

    [Fact]
    public void Encode_UnsupportedType_Throws()
    {
        Assert.Throws<NotSupportedException>(() => HarmonyPreferences.Encode(new object()));
    }

    [Fact]
    public void Encode_IsCultureInvariant()
    {
        // 小数点必须恒为 '.'（某些文化是 ','）——切文化后编码不得漂移
        var raw = HarmonyPreferences.Encode(1.25d);
        Assert.Equal("d:1.25", raw);
        Assert.Equal(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, ".");
    }
}
