using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class InputsPage : ContentPage
{
    private readonly string[] _colors = { "Ocean blue", "Forest green", "Sunset orange" };
    private string _pickedColor = "none";

    public InputsPage()
    {
        InitializeComponent();

        ColorPicker.ItemsSource = _colors;

        NameEntry.TextChanged += (_, e) =>
            NameEcho.Text = string.IsNullOrWhiteSpace(e.NewTextValue)
                ? "Hello, stranger"
                : $"Hello, {e.NewTextValue}";

        DemoSearch.SearchButtonPressed += (_, _) =>
        {
            SearchStatus.Text = $"Searched for “{DemoSearch.Text}”";
            ActivityLog.Record($"Inputs: search “{DemoSearch.Text}”");
        };

        VolumeSlider.ValueChanged += (_, e) =>
            VolumeValue.Text = ((int)e.NewValue).ToString();

        CountStepper.ValueChanged += (_, e) =>
        {
            CountValue.Text = ((int)e.NewValue).ToString();
            if (e.NewValue == 10)
                ActivityLog.Record("Inputs: count capped at 10");
        };

        NotificationsSwitch.Toggled += (_, _) => UpdateToggles("Notifications");
        TermsCheckBox.CheckedChanged += (_, e) =>
        {
            UpdateToggles("Terms");
            if (e.Value)
                ActivityLog.Record("Inputs: demo terms accepted");
        };
        SizeSmall.CheckedChanged += (_, e) => { if (e.Value) UpdateToggles("Size"); };
        SizeMedium.CheckedChanged += (_, e) => { if (e.Value) UpdateToggles("Size"); };
        SizeLarge.CheckedChanged += (_, e) => { if (e.Value) UpdateToggles("Size"); };

        ColorPicker.SelectedIndexChanged += (_, _) =>
        {
            if (ColorPicker.SelectedIndex < 0)
                return;
            _pickedColor = _colors[ColorPicker.SelectedIndex];
            ChoiceStatus.Text = $"Theme color: {_pickedColor}";
            ActivityLog.Record($"Inputs: picked {_pickedColor}");
        };
    }

    private void UpdateToggles(string changed)
    {
        var size = SizeSmall.IsChecked ? "S" : SizeLarge.IsChecked ? "L" : "M";
        ToggleStatus.Text =
            $"Notifications {(NotificationsSwitch.IsToggled ? "on" : "off")} · " +
            $"terms {(TermsCheckBox.IsChecked ? "accepted" : "not accepted")} · size {size}";
        ActivityLog.Record($"Inputs: {changed} changed ({size}, " +
            $"{(NotificationsSwitch.IsToggled ? "on" : "off")}, {(TermsCheckBox.IsChecked ? "yes" : "no")})");
    }
}
