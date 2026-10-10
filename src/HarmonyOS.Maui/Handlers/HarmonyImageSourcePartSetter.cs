#nullable enable
using System;
using Microsoft.Maui;
using Microsoft.Maui.Platform;

namespace HarmonyOS.Maui.Handlers;

internal sealed class HarmonyImageSourcePartSetter : IImageSourcePartSetter
{
    private readonly IElementHandler _handler;
    private readonly Action<object?> _setImage;

    public HarmonyImageSourcePartSetter(IElementHandler handler, Action<object?> setImage)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _setImage = setImage ?? throw new ArgumentNullException(nameof(setImage));
    }

    public IElementHandler? Handler => _handler;

    public IImageSourcePart? ImageSourcePart =>
        _handler.VirtualView as IImageSourcePart ?? _handler.VirtualView as Microsoft.Maui.IImage;

    public void SetImageSource(object? platformImage) => _setImage(platformImage);
}
