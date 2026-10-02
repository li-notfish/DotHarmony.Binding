using Microsoft.Maui.Controls;

namespace WeatherTwentyOne;

public class App : Application
{
    public App()
    {
        MainPage = new WeatherPage();
    }
}
