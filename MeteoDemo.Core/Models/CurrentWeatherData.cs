namespace MeteoDemo.Core.Models;

/// <summary>Текущие метеорологические показатели.</summary>
public sealed class CurrentWeatherData
{
    /// <summary>Время наблюдения в часовом поясе ответа.</summary>
    public string Time { get; init; } = "";

    /// <summary>Длительность шага данных в секундах.</summary>
    public int? IntervalSeconds { get; init; }

    /// <summary>Числовые значения показателей, индексированные именами переменных Open-Meteo.</summary>
    public Dictionary<string, double?> Values { get; init; } = new();
}
