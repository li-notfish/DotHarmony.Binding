namespace WeatherTwentyOne.Models;

public class HourlyForecast
{
    public string Time { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
}

public class DailyForecast
{
    public string DayName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string High { get; set; } = string.Empty;
    public string Low { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public int ChanceOfRain { get; set; }
}
