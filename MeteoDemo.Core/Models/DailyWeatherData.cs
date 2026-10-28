namespace MeteoDemo.Core.Models;

/// <summary>Суточные сводные метеорологические показатели.</summary>
public sealed class DailyWeatherData
{
    /// <summary>Даты, к которым относятся суточные значения.</summary>
    public string[] Time { get; init; } = [];

    /// <summary>Время восхода солнца для каждой даты в часовом поясе ответа.</summary>
    public string[]? Sunrise { get; init; }

    /// <summary>Время заката солнца для каждой даты в часовом поясе ответа.</summary>
    public string[]? Sunset { get; init; }

    /// <summary>Числовые суточные значения, индексированные именами переменных Open-Meteo.</summary>
    public Dictionary<string, double?[]> Values { get; init; } = new();
}
