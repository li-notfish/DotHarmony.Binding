#nullable enable
namespace HarmonyOS.Bindings.NativeNode;

/// <summary>
/// ArkUI_NumberValue 构造辅助
/// </summary>
public static class ArkUIValue
{
    public static ArkUI_NumberValue F(float value) => new() { f32 = value };
    public static ArkUI_NumberValue I(int value) => new() { i32 = value };
    public static ArkUI_NumberValue U(uint value) => new() { u32 = value };
}
