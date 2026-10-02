using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WeatherTwentyOne.Models;

namespace WeatherTwentyOne.ViewModels;

public class WeatherViewModel : INotifyPropertyChanged
{
    private string _location = "St. Louis, Missouri";
    private string _currentTemperature = "52F";
    private string _currentCondition = "Clear";
    private string _currentIcon = "weather_partly_cloudy_day.svg";

    public ObservableCollection<HourlyForecast> Hours { get; } = new();
    public ObservableCollection<DailyForecast> Days { get; } = new();

    public string Location
    {
        get => _location;
        set => Set(ref _location, value);
    }

    public string CurrentTemperature
    {
        get => _currentTemperature;
        set => Set(ref _currentTemperature, value);
    }

    public string CurrentCondition
    {
        get => _currentCondition;
        set => Set(ref _currentCondition, value);
    }

    public string CurrentIcon
    {
        get => _currentIcon;
        set => Set(ref _currentIcon, value);
    }

    public WeatherViewModel()
    {
        LoadStLouis();
    }

    public void LoadBoston()
    {
        Location = "Boston, Massachusetts";
        CurrentTemperature = "48F";
        CurrentCondition = "Cloudy";
        CurrentIcon = "weather_cloudy.svg";
        LoadHours("48", "46", "45", "44", "46", "49");
        LoadDays("48", "50", "52", "49", "47", "45", "46");
    }

    public void LoadRedmond()
    {
        Location = "Redmond, Washington";
        CurrentTemperature = "58F";
        CurrentCondition = "Partly Cloudy";
        CurrentIcon = "weather_partly_cloudy_day.svg";
        LoadHours("58", "57", "56", "55", "54", "53");
        LoadDays("58", "60", "62", "61", "59", "57", "56");
    }

    public void LoadStLouis()
    {
        Location = "St. Louis, Missouri";
        CurrentTemperature = "52F";
        CurrentCondition = "Clear";
        CurrentIcon = "weather_partly_cloudy_day.svg";
        LoadHours("52", "51", "50", "49", "50", "52");
        LoadDays("52", "61", "62", "57", "49", "49", "47");
    }

    public void Refresh()
    {
        if (Location.StartsWith("St. Louis", StringComparison.OrdinalIgnoreCase))
            LoadBoston();
        else if (Location.StartsWith("Boston", StringComparison.OrdinalIgnoreCase))
            LoadRedmond();
        else
            LoadStLouis();
    }

    private void LoadHours(params string[] values)
    {
        Hours.Clear();
        for (var i = 0; i < values.Length; i++)
        {
            Hours.Add(new HourlyForecast
            {
                Time = DateTime.Now.AddHours(i + 1).ToString("htt"),
                Icon = GetIcon(values[i]),
                Temperature = values[i] + "",
                Condition = values[i] == "52" ? "Clear" : "Cloudy",
            });
        }
    }

    private void LoadDays(params string[] highs)
    {
        Days.Clear();
        var icons = new[]
        {
            "weather_partly_cloudy_day.svg",
            "weather_partly_cloudy_night.svg",
            "weather_cloudy.svg",
            "weather_rain.svg",
            "weather_thunderstorm.svg",
            "weather_moon.svg",
            "weather_partly_cloudy_day.svg",
        };

        for (var i = 0; i < highs.Length; i++)
        {
            Days.Add(new DailyForecast
            {
                DayName = DateTime.Today.AddDays(i + 1).ToString("ddd"),
                Icon = icons[i],
                High = highs[i] + "F",
                Low = (int.Parse(highs[i]) - 9) + "F",
                Condition = i % 3 == 0 ? "Clear" : "Partly Cloudy",
                ChanceOfRain = i * 13 % 80,
            });
        }
    }

    private static string GetIcon(string value) => int.Parse(value) switch
    {
        >= 60 => "weather_partly_cloudy_day.svg",
        >= 50 => "weather_cloudy.svg",
        _ => "weather_moon.svg",
    };

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
