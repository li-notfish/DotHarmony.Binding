using Microsoft.Maui.Controls;
using HarmonyOS.Interop;

namespace ControlsSampleApp;

public partial class LayoutsPage : ContentPage
{
    public LayoutsPage()
    {
        InitializeComponent();

        DemoRefresh.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(RefreshView.IsRefreshing) && DemoRefresh.IsRefreshing)
            {
                await Task.Delay(500);
                MainThreadDispatcher.Post(() =>
                {
                    DemoRefresh.IsRefreshing = false;
                    RefreshStatus.Text = $"Refreshed at {DateTime.Now:HH:mm:ss}";
                    ActivityLog.Record("Layouts: pull-to-refresh");
                });
            }
        };

        ArchiveItem.Invoked += (_, _) =>
        {
            SwipeStatus.Text = "Archived via swipe";
            ActivityLog.Record("Layouts: SwipeView archive invoked");
        };

        TapCell(GridCellA, "left");
        TapCell(GridCellB, "right");
    }

    private void TapCell(BoxView cell, string name)
        => cell.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                GridStatus.Text = $"Tapped {name} cell";
                ActivityLog.Record($"Layouts: grid {name} cell tapped");
            }),
        });
}
