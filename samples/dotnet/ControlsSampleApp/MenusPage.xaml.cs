using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public partial class MenusPage : ContentPage
{
    private readonly MenuFlyout _flyout = new();

    public MenusPage()
    {
        InitializeComponent();

        MenuNew.Clicked += (_, _) => Choose("File ▸ New");
        MenuRecentOne.Clicked += (_, _) => Choose("File ▸ Recent ▸ Item one");
        MenuRecentTwo.Clicked += (_, _) => Choose("File ▸ Recent ▸ Item two");
        MenuCopy.Clicked += (_, _) => Choose("Edit ▸ Copy");

        var rename = new MenuFlyoutItem { Text = "Rename" };
        rename.Clicked += (_, _) => ChooseFlyout("Rename");
        var deleteItem = new MenuFlyoutItem { Text = "Delete" };
        deleteItem.Clicked += (_, _) => ChooseFlyout("Delete");
        _flyout.Add(rename);
        _flyout.Add(deleteItem);

        FlyoutBase.SetContextFlyout(FlyoutButton, _flyout);
        FlyoutButton.Clicked += (_, _) =>
        {
            FlyoutStatus.Text = "Flyout attached — long-press the button";
            ActivityLog.Record("Menus: flyout attached to button");
        };
    }

    private void Choose(string what)
    {
        MenuStatus.Text = $"Chose: {what}";
        ActivityLog.Record($"Menus: {what}");
    }

    private void ChooseFlyout(string what)
    {
        FlyoutStatus.Text = $"Flyout chose: {what}";
        ActivityLog.Record($"Menus: flyout “{what}”");
    }
}
