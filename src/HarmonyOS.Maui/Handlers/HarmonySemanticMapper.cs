#nullable enable
using HarmonyOS.Bindings.NativeNode;
using Microsoft.Maui;

namespace HarmonyOS.Maui.Handlers;

internal static class HarmonySemanticMapper
{
    public static void Apply(IView view, IHarmonySemanticNode node)
    {
        var semantics = view.Semantics;
        var text = semantics?.Description ?? view.AutomationId;
        if (!string.IsNullOrEmpty(text))
            node.SetSemanticText(text);

        var description = semantics?.Hint;
        if (!string.IsNullOrEmpty(description))
            node.SetSemanticDescription(description);

        // ArkUI's NDK does not currently expose a writable accessibility ID or
        // heading-level attribute. AutomationId is folded into TEXT, and heading
        // level remains a documented platform limitation rather than a fake mapping.
    }
}
