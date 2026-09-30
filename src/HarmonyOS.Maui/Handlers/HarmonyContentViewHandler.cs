// ContentView / ContentPresenter 的 HarmonyOS Handler。
// 语义对齐 MAUI 官方（dotnet/maui src/Controls/src/Core/ContentView）：
// 模板化控件的呈现内容为 TemplateRoot（ControlTemplate 实例化产物），
// 其余 ContentView 直接呈现 Content；ContentPresenter 是模板内承载外层
// Content 的槽位（TemplateBinding 已由核心链好）。
#nullable enable
using System.Runtime.CompilerServices;
using HarmonyOS.Interop;
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;
using ArkColumn = HarmonyOS.ArkUI.Column;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI ContentView（含 TemplatedView 模板化子类）的 Handler。</summary>
public class HarmonyContentViewHandler : HarmonyViewHandler<IContentView, ArkColumn>
{
    /// <summary>视图热路径日志开关（SizeChange / 内容挂载；与 LogLayout/LogScroll 同一纪律）</summary>
    internal static readonly bool LogView = false;

    public static PropertyMapper<IContentView, HarmonyContentViewHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(IContentView.Content)] = MapContent,
        [nameof(IContentView.Background)] = MapBackground,
        [nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
        [nameof(VisualElement.WidthRequest)] = MapWidthRequest,
        [nameof(VisualElement.HeightRequest)] = MapHeightRequest,
    };

    public HarmonyContentViewHandler() : base(Mapper) { }

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        return column;
    }

    protected override void ConnectHandler(ArkColumn platformView)
    {
        base.ConnectHandler(platformView);
        ApplySizing();
        platformView.SizeChange += OnSizeChange;
    }

    /// <summary>尺寸策略：显式 Request &gt; MAUI 布局父容器内自然尺寸；模板宿主/页面根 → 填满。
    /// Connect 与 Request 变更共用（Request 清除后回退同一路径，不残留固定尺寸）。</summary>
    private void ApplySizing()
    {
        if (VirtualView is VisualElement ve)
        {
            if (ve.WidthRequest > 0) PlatformView.SetWidth((float)ve.WidthRequest);
            else if (HarmonyLayoutHandler.IsHorizontalStack(ve.Parent))
                // 水平栈主轴 = 水平：子项宽度由内容决定，100% 宽会把每个子项撑成整屏
                // （WeatherTwentyOne 小时预报卡片实测踩过）
                PlatformView.SetWidthAuto();
            else PlatformView.SetWidthPercent(1.0f);
            if (ve.HeightRequest > 0) PlatformView.SetHeight((float)ve.HeightRequest);
            else if (ve.Parent is not Microsoft.Maui.Controls.Layout
                     || ve.Parent is Microsoft.Maui.Controls.TemplatedView
                     || ve.Parent is Microsoft.Maui.Controls.ContentPresenter
                     || HarmonyLayoutHandler.IsHorizontalStack(ve.Parent))
                // 垂直方向同在水平栈交叉轴：横向滚动内容（Hub/Swiper 类分节）需要交叉轴充满
                PlatformView.SetHeightPercent(1.0f);
            else
                // Request 清除且位于布局容器交叉轴：复位为自然尺寸
                PlatformView.SetHeightAuto();
        }
        else
        {
            PlatformView.SetWidthPercent(1.0f);
            PlatformView.SetHeightPercent(1.0f);
        }
    }

    protected override void DisconnectHandler(ArkColumn platformView)
    {
        platformView.SizeChange -= OnSizeChange;
        base.DisconnectHandler(platformView);
    }

    // 鸿蒙侧没有 MAUI 布局引擎：模板控件（如 HubView.UpdateLayout 读 Width）依赖
    // MAUI Frame 存在——据节点尺寸回填 Arrange，维持"原生测量先行、托管级联"的语义。
    // 单位契约：NODE_ON_SIZE_CHANGE 事件值在本通道为 vp（与 MAUI Arrange 坐标系一致，
    // HarmonyManagedLayoutHandler 全链同款假设；若 NDK 版本改语义需 px→vp 换算）
    private void OnSizeChange(ArkUINodeEvent e)
    {
        if (VirtualView is IView v && e.SizeChangeWidth > 0 && e.SizeChangeHeight > 0)
        {
            if (LogView) HiLog.Info("HarmonyHost", $"[CV] SizeChange {VirtualView?.GetType().Name} w={e.SizeChangeWidth} h={e.SizeChangeHeight}");
            v.Arrange(new Rect(0, 0, e.SizeChangeWidth, e.SizeChangeHeight));
        }
    }

    public static void MapWidthRequest(HarmonyContentViewHandler h, IContentView v)
        => h.ApplySizing();

    public static void MapHeightRequest(HarmonyContentViewHandler h, IContentView v)
        => h.ApplySizing();

    public static void MapBackground(HarmonyContentViewHandler h, IContentView v)
        => BrushHelper.ApplyBackground(h.PlatformView, v.Background!);

    public static void MapBackgroundColor(HarmonyContentViewHandler h, IContentView v)
    {
        if (v is VisualElement ve && ve.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapContent(HarmonyContentViewHandler handler, IContentView view)
    {
        ContentHost.Update(handler, ResolvePresentedContent(view));
        // 与 ContentPresenter 同一纪律：空 Content 的模板宿主不得参与命中测试
        handler.PlatformView.SetHitTestBehavior(ResolvePresentedContent(view) is null
            ? ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_NONE
            : ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_DEFAULT);
    }

    /// <summary>ContentView 呈现内容：模板化时为 TemplateRoot（PresentedContent），否则为 Content。</summary>
    internal static IView? ResolvePresentedContent(IContentView view)
        => view.PresentedContent as IView ?? view.Content as IView;
}

/// <summary>布局模板内 ContentPresenter（承载外层 ContentView.Content 的槽位）的 Handler。</summary>
public class HarmonyContentPresenterHandler : HarmonyViewHandler<ContentPresenter, ArkColumn>
{
    public static PropertyMapper<ContentPresenter, HarmonyContentPresenterHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(ContentPresenter.Content)] = MapContent,
        [nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
        // ContentPresenter 继承 Layout.Padding（模板内衬垫）：映射到 NODE_PADDING
        [nameof(ContentPresenter.Padding)] = MapPadding,
    };

    public HarmonyContentPresenterHandler() : base(Mapper) { }

    protected override ArkColumn CreatePlatformView()
    {
        var column = new ArkColumn();
        column.SetWidthPercent(1.0f);
        column.SetHeightPercent(1.0f);
        return column;
    }

    public static void MapBackgroundColor(HarmonyContentPresenterHandler h, ContentPresenter v)
    {
        if (v.BackgroundColor is { } c)
            h.PlatformView.SetBackgroundColor(c.ToUint());
    }

    public static void MapPadding(HarmonyContentPresenterHandler h, ContentPresenter v)
    {
        var p = v.Padding;
        if (p.Top > 0 || p.Right > 0 || p.Bottom > 0 || p.Left > 0)
        {
            h.PlatformView.SetPaddingEdges(
                (float)p.Top, (float)p.Right, (float)p.Bottom, (float)p.Left);
        }
    }

    public static void MapContent(HarmonyContentPresenterHandler handler, ContentPresenter view)
    {
        ContentHost.Update(handler, view.Content);
        // 空 ContentPresenter 是纯布局占位（如 ControlTemplate 的 Header 槽会被管理 Grid
        // 拉伸到整棵子树高度、且 z 序居上）：参与命中测试会成为覆盖全部内容的"输入黑洞"
        // ——Metro Hub 磁贴点击/滑动全失效的根因。空槽穿透，非空恢复正常。
        handler.PlatformView.SetHitTestBehavior(view.Content is null
            ? ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_NONE
            : ArkUI_HitTestMode.ARKUI_HIT_TEST_MODE_DEFAULT);
    }

    protected override void ConnectHandler(ArkColumn platformView)
    {
        base.ConnectHandler(platformView);
        platformView.SizeChange += OnSizeChange;
    }

    protected override void DisconnectHandler(ArkColumn platformView)
    {
        platformView.SizeChange -= OnSizeChange;
        base.DisconnectHandler(platformView);
    }

    private void OnSizeChange(ArkUINodeEvent e)
    {
        // 事件尺寸为 vp（与 Arrange 坐标系一致，单位契约见 HarmonyContentViewHandler.OnSizeChange）
        if (VirtualView is IView v && e.SizeChangeWidth > 0 && e.SizeChangeHeight > 0)
            v.Arrange(new Rect(0, 0, e.SizeChangeWidth, e.SizeChangeHeight));
    }
}

/// <summary>单 Content 容器的公共挂树/断树（ContentView/ContentPresenter 共用）。</summary>
internal static class ContentHost
{
    private sealed class Holder { public IElementHandler? Handler; }
    private static readonly ConditionalWeakTable<IElementHandler, Holder> _holders = new();
#pragma warning disable CS0618 // SetChildInheritedBindingContext 由模板核心链好子级 BindingContext
    public static void Update<TVirtualView, TPlatform>(HarmonyViewHandler<TVirtualView, TPlatform> self, IView? content)
        where TVirtualView : class, Microsoft.Maui.IView
        where TPlatform : ArkUINodeBase
    {
        var holder = _holders.GetValue(self, _ => new Holder());
        if (holder.Handler is not null)
        {
            HarmonyViewHandler<IView, ArkUINode>.DisposeContent(holder.Handler, self.PlatformView);
        holder.Handler = null;
        }
        if (content is null) return;
        if (HarmonyContentViewHandler.LogView)
            HiLog.Info("HarmonyHost", $"[ContentHost] attach {content.GetType().Name} under {self.VirtualView?.GetType().Name}");
        var childHandler = HarmonyHandlerFactory.Create(content);
        childHandler.SetVirtualView(content);
        holder.Handler = childHandler;
        if (childHandler.PlatformView is ArkUINode node)
        {
            node.SetWidthPercent(1.0f);
            node.SetHeightPercent(1.0f);
            self.PlatformView.AddChild(node);
        }
    }
#pragma warning restore CS0618
}
