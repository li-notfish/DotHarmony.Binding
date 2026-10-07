using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class InputsPage : ContentPage
{
    public InputsPage()
    {
        InitializeComponent();
        DemoPicker.ItemsSource = new[] { "Alpha", "Beta", "Gamma" };
        DemoPicker.SelectedIndex = 0;
    }
}
