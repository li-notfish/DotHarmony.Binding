using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using HarmonyOS.Interop;
using ArkImage = HarmonyOS.ArkUI.Image;
using MImage = Microsoft.Maui.IImage;

namespace HarmonyOS.Maui.Handlers;

/// <summary>MAUI Image 的 HarmonyOS Handler（ArkUI Image 节点）。</summary>
public class HarmonyImageHandler : HarmonyViewHandler<MImage, ArkImage>
{
    public static PropertyMapper<MImage, HarmonyImageHandler> Mapper = new(HarmonyViewMapper.Base)
    {
        [nameof(MImage.Source)] = MapSource,
        [nameof(MImage.Aspect)] = MapAspect,
        [nameof(VisualElement.WidthRequest)] = MapWidthRequest,
        [nameof(VisualElement.HeightRequest)] = MapHeightRequest,
    };

    public HarmonyImageHandler() : base(Mapper) { }

    protected override ArkImage CreatePlatformView() => new();

    public static void MapWidthRequest(HarmonyImageHandler h, MImage v)
    {
        // Request 清除（-1）须复位 Auto——否则绑定往返后旧固定值残留在节点上。
        // 显式宽度必须落到节点：Auto 轨道/水平栈里 ArkUI Image 按解码原图尺寸自撑，
        // 34x34 的图标会按 PNG 原尺寸溢出轨道（WeatherTwentyOne 七日行图标实测踩过）
        if (v is VisualElement ve && ve.WidthRequest >= 0)
            h.PlatformView.SetWidth((float)ve.WidthRequest);
        else
            h.PlatformView.SetWidthAuto();
    }

    public static void MapHeightRequest(HarmonyImageHandler h, MImage v)
    {
        if (v is VisualElement ve && ve.HeightRequest >= 0)
            h.PlatformView.SetHeight((float)ve.HeightRequest);
        else
            h.PlatformView.SetHeightAuto();
    }

    protected override void ConnectHandler(ArkImage platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Error += OnImageError;
    }

    protected override void DisconnectHandler(ArkImage platformView)
    {
        platformView.Error -= OnImageError;
        base.DisconnectHandler(platformView);
    }

    public static void MapSource(HarmonyImageHandler h, MImage v)
    {
        // 流式来源：异步落盘后回写 Src（不阻塞 UI 线程；回写经 MainThreadDispatcher 落到 JS 线程）
        if (v.Source is StreamImageSource stream)
        {
            _ = LoadStreamSourceAsync(h, stream);
            return;
        }

        // 裸相对名（MAUI 资源约定）：包内 rawfile 异步解析优先，miss 回退 file:// 相对路径
        if (v.Source is FileImageSource { File: { } file } &&
            !file.Contains("://") && !System.IO.Path.IsPathRooted(file))
        {
            _ = LoadRawfileSourceAsync(h, v, file);
            return;
        }

        var src = ImageSourceResolver.Resolve(v.Source);
        if (src is not null)
            h.PlatformView.Src = src;
        else
            HiLog.Warn("Image", $"Unsupported image source: {v.Source?.GetType().Name ?? "null"}");
    }

    private static async System.Threading.Tasks.Task LoadStreamSourceAsync(HarmonyImageHandler h, StreamImageSource source)
    {
        var path = await ImageSourceResolver.ResolveStreamAsync(source);
        if (path is null)
            return;
        MainThreadDispatcher.Post(() =>
        {
            // 回写时视图可能已换图/断连：Source 仍指向同一流才更新，节点已释放则跳过
            if (ReferenceEquals(h.VirtualView?.Source, source))
            {
                try { h.PlatformView.Src = path; }
                catch (InvalidOperationException) { /* 节点已销毁 */ }
            }
        });
    }

    private static async System.Threading.Tasks.Task LoadRawfileSourceAsync(
        HarmonyImageHandler h, MImage view, string logicalName)
    {
        // 捕获触发时的 Source，回写时比对对象本身（对齐 LoadStreamSourceAsync 的守卫口径）
        var origin = view.Source;
        var uri = await ImageSourceResolver.ResolveRawfileAsync(logicalName)
                  ?? $"file://{logicalName}"; // rawfile 未暂存：退回相对路径语义（旧行为）
        MainThreadDispatcher.Post(() =>
        {
            if (ReferenceEquals(h.VirtualView?.Source, origin))
            {
                try { h.PlatformView.Src = uri; }
                catch (InvalidOperationException) { /* 节点已销毁 */ }
            }
        });
    }

    public static void MapAspect(HarmonyImageHandler h, MImage v)
    {
        h.PlatformView.ObjectFit = v.Aspect switch
        {
            Aspect.AspectFit => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_CONTAIN,
            Aspect.AspectFill => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_COVER,
            Aspect.Fill => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_FILL,
            _ => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_CONTAIN,
        };
    }

    private void OnImageError(ArkUINodeEvent e)
    {
        HiLog.Warn("Image", $"Failed to load: {VirtualView.Source}");
    }

}
