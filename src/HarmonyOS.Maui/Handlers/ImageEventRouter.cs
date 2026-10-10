#nullable enable
using Microsoft.Maui;

namespace HarmonyOS.Maui.Handlers;

internal static class ImageEventRouter
{
    public static void Started(IImageSourcePartEvents events, IImageSourcePart part)
    {
        events.LoadingStarted();
        part.UpdateIsLoading(true);
    }

    public static void Completed(IImageSourcePartEvents events, IImageSourcePart part, bool successful)
    {
        events.LoadingCompleted(successful);
        part.UpdateIsLoading(false);
    }

    public static void Failed(IImageSourcePartEvents events, IImageSourcePart part, Exception exception)
    {
        events.LoadingFailed(exception);
        part.UpdateIsLoading(false);
    }
}
