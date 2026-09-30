// 所有 HarmonyOS View Handler 的统一基类：在 Connect/Disconnect 生命周期挂载
// 手势识别管线（HarmonyGestureManager：View.GestureRecognizers → ArkUI 原生手势）。
// Page 类 Handler（ContentPage/NavigationPage）无手势语义，保持直接继承 ViewHandler。
#nullable enable
using System.Collections.Generic;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace HarmonyOS.Maui.Handlers;

public abstract class HarmonyViewHandler<TVirtualView, TPlatformView> : ViewHandler<TVirtualView, TPlatformView>
    where TVirtualView : class, Microsoft.Maui.IView
    where TPlatformView : ArkUINodeBase
{
    private HarmonyGestureManager? _gestures;

    protected HarmonyViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper) { }

    protected override void ConnectHandler(TPlatformView platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is Microsoft.Maui.Controls.View controlsView)
            _gestures = new HarmonyGestureManager(controlsView, platformView);

        // 初始视觉态一次落齐（ViewHandler.SetVirtualView 密封，无法后置；mapper 刷新
        // 不含视觉键，此处时序即"全部初始态就绪"）。装饰性通道逐项降级：单项失败仅
        // 告警，不炸页面构建（BuildUICore 对异常整体 -2 = 白屏）
        if (VirtualView is IView v)
        {
            ApplyVisualSafe(() => ApplyVisibility(v, platformView), v, "Visibility");
            ApplyVisualSafe(() => ApplyEnabled(v, platformView), v, "IsEnabled");
            ApplyVisualSafe(() => ApplyOpacity(v, platformView), v, "Opacity");
            ApplyVisualSafe(() => ApplyTransform(v, platformView), v, "Transform");
            ApplyVisualSafe(() => ApplyPivot(v, platformView), v, "Anchor");
            ApplyVisualSafe(() => ApplyHitTestMode(v, platformView), v, "InputTransparent");
        }
    }

    private static void ApplyVisualSafe(Action apply, IView view, string channel)
    {
        try
        {
            apply();
        }
        catch (Exception ex)
        {
            HiLog.Warn("HarmonyHost",
                $"[Visual] {channel} apply failed on {view.GetType().Name}: {ex.Message}");
        }
    }

    protected override void DisconnectHandler(TPlatformView platformView)
    {
        _gestures?.Dispose();
        _gestures = null;
        base.DisconnectHandler(platformView);
    }

    public override void Invoke(string command, object? args)
    {
        // MAUI 的 ZIndex 变更经 VisualElement.ZIndexPropertyChanged → Handler.Invoke("ZIndex")
        // 命令通道下发（非属性映射）。栈式布局另有 "UpdateZIndex" 批量命令，互不冲突
        if (command == nameof(IView.ZIndex))
        {
            // 变更后按声明顺序重写全体兄弟的 z（唯一化编码），恢复 MAUI 平局语义；
            // 无布局父容器（单内容宿主等）只写自身
            if (!ZIndexOrder.TryRewriteSiblings(VirtualView))
                PlatformView.SetZIndex(ZIndexOrder.EffectiveZ(VirtualView));
            return;
        }
        base.Invoke(command, args);
    }

    // ───────────────────────── 通用视觉协议 ─────────────────────────
    // MAUI 基础视觉属性（Visibility/IsEnabled/Opacity/变换/锚点/InputTransparent）统一在此
    // 映射：拦截走 UpdateValue 之前，先于各 handler 的 new(HarmonyViewMapper.Base) 链生效——具体
    // handler 的 mapper 不必也不应重复映射这些键。初始态在 SetVirtualView（mapper 刷新
    // 之后）一次性落齐，替代旧的 Connect 一次性 InputTransparent 写入（后者无法响应变更）。

    private static readonly HashSet<string> VisualProperties = new(StringComparer.Ordinal)
    {
        nameof(IView.Visibility),
        nameof(VisualElement.IsVisible),
        nameof(IView.IsEnabled),
        nameof(IView.Opacity),
        nameof(IView.TranslationX),
        nameof(IView.TranslationY),
        nameof(IView.Scale),
        nameof(IView.ScaleX),
        nameof(IView.ScaleY),
        nameof(IView.Rotation),
        nameof(IView.RotationX),
        nameof(IView.RotationY),
        nameof(VisualElement.AnchorX),
        nameof(VisualElement.AnchorY),
        nameof(VisualElement.InputTransparent),
    };

    public override void UpdateValue(string property)
    {
        if (VisualProperties.Contains(property))
        {
            if (VirtualView is not IView v || PlatformView is not ArkUINodeBase node)
                return;
            try
            {
                switch (property)
                {
                    case nameof(IView.Visibility):
                    case nameof(VisualElement.IsVisible):
                        ApplyVisibility(v, node);
                        break;
                    case nameof(IView.IsEnabled):
                        ApplyEnabled(v, node);
                        break;
                    case nameof(IView.Opacity):
                        ApplyOpacity(v, node);
                        break;
                    case nameof(IView.TranslationX):
                    case nameof(IView.TranslationY):
                    case nameof(IView.Scale):
                    case nameof(IView.ScaleX):
                    case nameof(IView.ScaleY):
                    case nameof(IView.Rotation):
                    case nameof(IView.RotationX):
                    case nameof(IView.RotationY):
                        ApplyTransform(v, node);
                        break;
                    case nameof(VisualElement.AnchorX):
                    case nameof(VisualElement.AnchorY):
                        ApplyPivot(v, node);
                        break;
                    case nameof(VisualElement.InputTransparent):
                        ApplyInputTransparent(v);
                        break;
                }
            }
            catch (Exception ex)
            {
                HiLog.Warn("HarmonyHost",
                    $"[Visual] {property} update failed on {v.GetType().Name}: {ex.Message}");
            }
            return;
        }
        base.UpdateValue(property);
    }

    internal static void ApplyVisibility(IView view, ArkUINodeBase node)
        => node.SetVisibility(view.Visibility switch
        {
            Visibility.Visible => ArkUI_Visibility.ARKUI_VISIBILITY_VISIBLE,
            Visibility.Hidden => ArkUI_Visibility.ARKUI_VISIBILITY_HIDDEN,
            _ => ArkUI_Visibility.ARKUI_VISIBILITY_NONE,
        });

    internal static void ApplyEnabled(IView view, ArkUINodeBase node)
        => node.Enabled = view.IsEnabled;

    internal static void ApplyOpacity(IView view, ArkUINodeBase node)
        => node.SetOpacity(Math.Clamp((float)view.Opacity, 0f, 1f));

    /// <summary>变换组合语义：MAUI Scale 为全局倍率（最终 X = Scale × ScaleX）；三轴旋转一次下发。</summary>
    internal static void ApplyTransform(IView view, ArkUINodeBase node)
    {
        node.SetTranslate((float)view.TranslationX, (float)view.TranslationY);
        node.SetScale((float)(view.Scale * view.ScaleX), (float)(view.Scale * view.ScaleY));
        node.SetRotation((float)view.RotationX, (float)view.RotationY, (float)view.Rotation);
    }

    /// <summary>锚点 → 变换中心（NODE_TRANSFORM_CENTER，组件相对坐标）。IView 层无 AnchorX/Y，
    /// 缺省取 MAUI 默认锚点 0.5/0.5（与 ArkUI 默认一致）。</summary>
    internal static void ApplyPivot(IView view, ArkUINodeBase node)
    {
        double anchorX = 0.5, anchorY = 0.5;
        if (view is VisualElement ve)
        {
            anchorX = ve.AnchorX;
            anchorY = ve.AnchorY;
        }
        node.SetPivot((float)anchorX, (float)anchorY);
    }

    private void ApplyInputTransparent(IView view)
    {
        ApplyHitTestMode(view, PlatformView);
        // 容器级联：本视图透明性变更后，子树内所有已挂 handler 的视图按同一规则重刷
        if (view is Element root)
        {
            foreach (var descendant in EnumerateVisualDescendants(root))
                if (descendant.Handler?.PlatformView is ArkUINodeBase childNode)
                    ApplyHitTestMode(descendant, childNode);
        }
    }

    internal static void ApplyHitTestMode(IView view, ArkUINodeBase node)
        => node.SetHitTestBehavior(VisualProtocol.ResolveHitTestMode(view));

    private static IEnumerable<IView> EnumerateVisualDescendants(Element root)
    {
        foreach (var child in ((IElementController)root).LogicalChildren)
        {
            if (child is null)
                continue;
            if (child is IView childView)
                yield return childView;
            foreach (var grandChild in EnumerateVisualDescendants(child))
                yield return grandChild;
        }
    }

    /// <summary>
    /// 处置一个托管子内容（Content/Scroll 内容位场景）：断连 handler（注销手势与节点事件）
    /// 并释放平台节点子树。直接 RemoveAllChildren 而不释放会泄漏 NodeEventBus 订阅与原生节点。
    /// </summary>
    internal static void DisposeContent(IElementHandler? contentHandler, ArkUINodeBase container)
    {
        if (contentHandler is null)
            return;
        if (contentHandler.VirtualView is IElement element)
            element.Handler = null; // 触发 DisconnectHandler（HarmonyGestureManager / 事件订阅清理）
        if (contentHandler.PlatformView is ArkUINodeBase node)
        {
            container.RemoveChild(node);
            node.Dispose();
        }
    }
}

/// <summary>
/// 通用视觉协议纯规则层（单测直接覆盖）：MAUI InputTransparent 容器级联 ——
/// 自身或任一祖先 InputTransparent 即整链透明。透明容器 → TRANSPARENT（不阻塞子树与
/// 内部非托管子节点的命中路径，全屏装饰层场景的核心语义）；透明叶子 → NONE（整节点
/// 穿透）；不透明 → DEFAULT。
/// </summary>
internal static class VisualProtocol
{
    public static ArkUI_HitTestMode ResolveHitTestMode(IView view)
    {
        bool transparent = view.InputTransparent;
        if (!transparent && view is Element element)
        {
            for (var parent = element.Parent; parent is not null; parent = parent.Parent)
            {
                if (parent is IView parentView && parentView.InputTransparent)
                {
                    transparent = true;
                    break;
                }
            }
        }

        if (!transparent)
            return ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_DEFAULT;

        bool hasChildren = view is Element e && ((IElementController)e).LogicalChildren.Count > 0;
        return hasChildren
            ? ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_TRANSPARENT
            : ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_NONE;
    }
}

/// <summary>
/// NODE_Z_INDEX 仲裁规则（原生探针实测验证，scripts/verify-zindex-probe.ps1）：
/// 1) 同值写入不触发重排；2) 值变化的节点落入新分组时排在该组现有成员之上。
/// 因此 MAUI「同 z 平局 = 后声明者在上」必须物理消灭平局：
/// 编码为 z * 兄弟数 + 声明序下标——保持 ZIndex 全序，同值组内按声明序唯一递增。
/// 必须与所有写 z 的路径（Invoke / UpdateZIndex / Arrange / AttachChild）共用同一编码。
/// </summary>
internal static class ZIndexOrder
{
    /// <summary>float 整数精确域（2^24）；编码结果必须保持在该范围内。</summary>
    private const int FloatExactMax = 1 << 24;

    // 父容器判定走 IList<IView>（Controls.Layout 直接实现该集合接口；
    // 不能靠 ILayoutController——反射实测 Grid 并未实现它，旧的旧实现因此从未命中）
    private static IList<IView>? ParentChildren(IView view)
        => view.Parent as IList<IView>;

    /// <summary>兄弟范围内唯一化后的 z（平局按声明序递增；无布局父容器时原样返回）。</summary>
    public static float EffectiveZ(IView view)
    {
        int z = view.ZIndex;
        if (ParentChildren(view) is not { } children)
            return Encode(z, 1, 0);
        int count = children.Count;
        for (int i = 0; i < count; i++)
            if (ReferenceEquals(children[i], view))
                return Encode(z, count, i);
        return Encode(z, 1, 0);
    }

    /// <summary>某兄弟 z 变更后按声明顺序重写全员（全部唯一值，原生按值排序即 MAUI 语义）。</summary>
    public static bool TryRewriteSiblings(IView view)
    {
        if (ParentChildren(view) is not { } children)
            return false;
        int count = children.Count;
        for (int i = 0; i < count; i++)
        {
            if (children[i].Handler?.PlatformView is ArkUINodeBase node)
                node.SetZIndex(Encode(children[i].ZIndex, count, i));
        }
        return true;
    }

    private static float Encode(int z, int count, int index)
    {
        // 按兄弟数动态收缩 z 域，保证 z*count+i 始终落在 float 的整数精确域内。
        int maxZ = Math.Max(1, FloatExactMax / Math.Max(count, 1));
        long encoded = (long)Math.Clamp(z, -maxZ, maxZ) * count + index;
        return Math.Clamp(encoded, -FloatExactMax, FloatExactMax);
    }
}
