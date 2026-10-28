namespace MeteoDemo.Core.Models;

/// <summary>Массивы почасовых метеорологических показателей.</summary>
public sealed class HourlyWeatherData
{
    /// <summary>Почасовые временные метки в часовом поясе ответа.</summary>
    public string[] Time { get; init; } = [];

    /// <summary>Длительность шага данных в секундах.</summary>
    public int? IntervalSeconds { get; init; }

    /// <summary>Массивы числовых значений, индексированные именами переменных Open-Meteo.</summary>
    public Dictionary<string, double?[]> Values { get; init; } = new();
}
