namespace MeteoDemo.Core.Models;

/// <summary>Единицы измерения полей из соответствующего блока ответа Open-Meteo.</summary>
public sealed class WeatherUnits
{
    /// <summary>Единицы измерения, индексированные именами показателей API.</summary>
    public Dictionary<string, string> Values { get; init; } = new();
}
