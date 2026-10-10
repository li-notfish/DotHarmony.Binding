#nullable enable
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui.Controls;

namespace HarmonyOS.Maui.Handlers;

internal static class ButtonTouchRouter
{
    public static void Route(Button button, ArkPointerTouchAction action)
    {
        if (action == ArkPointerTouchAction.Pressed)
            button.SendPressed();
        else if (action is ArkPointerTouchAction.Released or ArkPointerTouchAction.Canceled)
            button.SendReleased();
    }
}
