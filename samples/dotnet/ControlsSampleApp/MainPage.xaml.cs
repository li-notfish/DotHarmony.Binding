namespace ControlsSampleApp;

public partial class MainPage : TabbedPage
{
    private int _taps;

    public MainPage()
    {
        InitializeComponent();

        FeedList.ItemsSource = ActivityLog.Entries;
        ActivityLog.Entries.CollectionChanged += (_, _) =>
            EmptyFeedLabel.IsVisible = ActivityLog.Entries.Count == 0;

        CounterButton.Clicked += (_, _) =>
        {
            _taps++;
            CounterLabel.Text = $"Tapped {_taps} time{(_taps == 1 ? "" : "s")}";
            ActivityLog.Record($"Home: counter tapped ({_taps})");
        };
    }
}
