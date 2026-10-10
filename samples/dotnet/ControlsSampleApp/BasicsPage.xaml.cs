using Microsoft.Maui.Controls;
using HarmonyOS.Interop;
using Microsoft.Maui.Dispatching;

namespace ControlsSampleApp;

public partial class BasicsPage : ContentPage
{
    private int _buttonTaps;
    private int _imageTaps;

    public BasicsPage()
    {
        InitializeComponent();

        MainButton.Clicked += (_, _) =>
        {
            _buttonTaps++;
            ButtonsStatus.Text = $"Button pressed {_buttonTaps}x · Image pressed {_imageTaps}x";
            ActivityLog.Record($"Basics: Button pressed ({_buttonTaps})");
        };

        IconButton.Clicked += (_, _) =>
        {
            _imageTaps++;
            ButtonsStatus.Text = $"Button pressed {_buttonTaps}x · Image pressed {_imageTaps}x";
            ActivityLog.Record($"Basics: ImageButton pressed ({_imageTaps})");
        };

        ProgressButton.Clicked += async (_, _) =>
        {
            if (Spinner.IsRunning)
                return;

            Spinner.IsRunning = true;
            ProgressButton.IsEnabled = false;
            for (var i = 1; i <= 20; i++)
            {
                await Task.Delay(120);
                MainThreadDispatcher.Post(() =>
                {
                    DemoProgress.Progress = i / 20.0;
                    ProgressStatus.Text = $"Working… {i * 5}%";
                });
            }

            MainThreadDispatcher.Post(() =>
            {
                Spinner.IsRunning = false;
                DemoProgress.Progress = 0;
                ProgressButton.IsEnabled = true;
                ProgressStatus.Text = "Done";
                ActivityLog.Record("Basics: fake task completed");
            });
        };
    }
}
