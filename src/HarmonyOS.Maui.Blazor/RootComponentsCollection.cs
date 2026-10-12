#nullable enable
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components.Web;

namespace HarmonyOS.Maui.Blazor;

public sealed class HarmonyRootComponentsCollection : ObservableCollection<HarmonyRootComponent>, IJSComponentConfiguration
{
    public HarmonyRootComponentsCollection(JSComponentConfigurationStore jsComponents)
    {
        ArgumentNullException.ThrowIfNull(jsComponents);
        JSComponents = jsComponents;
    }

    public JSComponentConfigurationStore JSComponents { get; }
}
