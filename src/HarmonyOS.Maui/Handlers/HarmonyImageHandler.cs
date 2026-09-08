using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using HarmonyOS.Bindings.NativeNode;
using ArkImage = HarmonyOS.ArkUI.Image;
using MImage = Microsoft.Maui.IImage;

namespace HarmonyOS.Maui.Handlers;

public class HarmonyImageHandler : ViewHandler<MImage, ArkImage>
{
    public static PropertyMapper<MImage, HarmonyImageHandler> Mapper = new(ViewMapper)
    {
        [nameof(MImage.Source)] = MapSource,
        [nameof(MImage.Aspect)] = MapAspect,
    };

    public HarmonyImageHandler() : base(Mapper) { }

    protected override ArkImage CreatePlatformView() => new();

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

    public static void MapSource(HarmonyImageHandler handler, MImage view)
    {
        var src = ResolveImageSource(view.Source);
        if (src is not null)
            handler.PlatformView.Src = src;
    }

    public static void MapAspect(HarmonyImageHandler handler, MImage view)
    {
        handler.PlatformView.ObjectFit = view.Aspect switch
        {
            Aspect.AspectFit => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_CONTAIN,
            Aspect.AspectFill => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_COVER,
            Aspect.Fill => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_FILL,
            _ => ArkUI_ObjectFit.ARKUI_OBJECT_FIT_CONTAIN,
        };
    }

    private static string? ResolveImageSource(IImageSource? source) => source switch
    {
        UriImageSource uri => uri.Uri?.ToString(),
        FileImageSource file => $"file://{file.File}",
        _ => null,
    };

    private void OnImageError(ArkUINodeEvent e)
    {
    }
}
