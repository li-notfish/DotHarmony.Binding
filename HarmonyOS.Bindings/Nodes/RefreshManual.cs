// 手写补充节点类（勿并入 refresh.cs —— 该文件由 NativeCodeGenerator 生成，请勿手工编辑）
// refresh.d.ts Evo 为新式属性包风格（Refresh({ refreshing: false })），解析器暂未展开
// 构造 options 接口，生成的 Refresh 类缺 refreshing 状态 setter，此处子类补充。
#nullable enable
using HarmonyOS.Bindings.NativeNode;

namespace HarmonyOS.ArkUI;

/// <summary>Refresh 的手写补充：刷新态设置（NODE_REFRESH_REFRESHING，i32）</summary>
public unsafe class RefreshNode : Refresh
{
    /// <summary>是否处于刷新中状态</summary>
    public bool IsRefreshing
    {
        set => SetNumericAttribute(ArkUI_NodeAttributeType.NODE_REFRESH_REFRESHING, ArkUIValue.I(value ? 1 : 0));
    }
}
