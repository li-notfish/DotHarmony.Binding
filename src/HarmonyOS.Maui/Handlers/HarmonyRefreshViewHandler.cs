using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkRefresh = HarmonyOS.ArkUI.Refresh;
using ArkUINode = HarmonyOS.Bindings.NativeNode.ArkUINodeBase;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyRefreshViewHandler : HarmonyViewHandler<IRefreshView, ArkRefresh>, IRefreshViewHandler
{
    public static PropertyMapper<IRefreshView, IRefreshViewHandler> Mapper = new(ViewMapper)
    {
        [nameof(IRefreshView.IsRefreshing)] = MapIsRefreshing,
        [nameof(IRefreshView.Content)] = MapContent,
        [nameof(IRefreshView.RefreshColor)] = MapRefreshColor,
    };

    public HarmonyRefreshViewHandler() : base(Mapper) { }

    private IElementHandler? _contentHandler;

    protected override ArkRefresh CreatePlatformView() => new();

    protected override void ConnectHandler(ArkRefresh platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Refreshing += OnRefreshing;
    }

    protected override void DisconnectHandler(ArkRefresh platformView)
    {
        platformView.Refreshing -= OnRefreshing;
        base.DisconnectHandler(platformView);
    }

    public static void MapIsRefreshing(IRefreshViewHandler handler, IRefreshView view)
    {
        if (handler is HarmonyRefreshViewHandler h)
            h.PlatformView.IsRefreshing = view.IsRefreshing;
    }

    public static void MapContent(IRefreshViewHandler handler, IRefreshView view)
    {
        if (handler is HarmonyRefreshViewHandler h)
        {
            // 防重入：Content 变更时先摘除旧子树（mapper 初始同步与属性变更都会进这里）
            if (h._contentHandler is not null)
            {
                h.PlatformView.RemoveAllChildren();
                h._contentHandler = null;
            }

            if (view.Content is IView content)
            {
                var childHandler = HarmonyHandlerFactory.Create(content);
                childHandler.SetVirtualView(content);
                h._contentHandler = childHandler;
                if (childHandler.PlatformView is ArkUINode node)
                {
                    // 内容宽高填满 Refresh 视口（Refresh 自身高度由父容器/内容决定）
                    node.SetWidthPercent(1.0f);
                    node.SetHeightPercent(1.0f);
                    h.PlatformView.AddChild(node);
                }
            }
        }
    }

    public static void MapRefreshColor(IRefreshViewHandler handler, IRefreshView view)
    {
        // ArkUI C API Refresh 无进度球颜色属性，视觉跟随系统主题（gap：NODE 无对应枚举）
    }

    private void OnRefreshing(ArkUINodeEvent e)
    {
        // 下拉触发：置 IsRefreshing（Controls 层据此执行 Command / 触发刷新流程）
        VirtualView.IsRefreshing = true;
    }
}
