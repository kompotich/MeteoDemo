namespace MeteoDemo.Core.Configuration;

public sealed class OpenMeteoOptions
{
    public required string BaseUrl { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public required string[] Current { get; init; }
    public required string[] Hourly { get; init; }
    public required string[] Daily { get; init; }
    public int ForecastDays { get; init; } = 3;
    public int PastDays { get; init; }
    public string Timezone { get; init; } = "auto";
    public string TemperatureUnit { get; init; } = "celsius";
    public string WindSpeedUnit { get; init; } = "kmh";
    public string PrecipitationUnit { get; init; } = "mm";
    public int Tilt { get; init; } = 45;
    public int Azimuth { get; init; }
}
