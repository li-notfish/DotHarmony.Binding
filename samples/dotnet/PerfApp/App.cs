using Microsoft.Maui.Controls;

namespace PerfApp;

public partial class App : Application
{
    public App()
    {
#pragma warning disable CS0618 // The Harmony host intentionally calls CreateWindow(null).
        MainPage = new MainPage();
#pragma warning restore CS0618
    }
}
