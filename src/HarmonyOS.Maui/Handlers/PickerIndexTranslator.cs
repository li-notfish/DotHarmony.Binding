#nullable enable

namespace HarmonyOS.Maui.Handlers;

internal static class PickerIndexTranslator
{
    public static int ToNative(int selectedIndex)
        => selectedIndex < 0 ? 0 : selectedIndex + 1;

    public static int FromNative(int nativeIndex)
        => nativeIndex <= 0 ? -1 : nativeIndex - 1;
}
