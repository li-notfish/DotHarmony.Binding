using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class TabbedDemoPage : TabbedPage
{
    public TabbedDemoPage()
    {
        InitializeComponent();

        var pages = Children.ToArray();
        for (var i = 0; i < pages.Length; i++)
        {
            var page = pages[i];
            Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(5 * (i + 1)), () => SelectedItem = page);
        }
    }
}
