using Microsoft.Maui.Controls;

namespace ControlsSampleApp;

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
        => new(new MainPage());
}
