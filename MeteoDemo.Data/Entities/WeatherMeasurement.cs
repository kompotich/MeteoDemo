namespace MeteoDemo.Data.Entities;

/// <summary>Одно значение показателя погоды в конкретном разделе и временной точке прогноза.</summary>
public sealed class WeatherMeasurement
{
    /// <summary>Первичный ключ измерения.</summary>
    public long Id { get; set; }

    /// <summary>Внешний ключ связанного снимка погоды.</summary>
    public long WeatherSnapshotId { get; set; }

    /// <summary>Навигационное свойство к снимку, которому принадлежит измерение.</summary>
    public WeatherSnapshot WeatherSnapshot { get; set; } = null!;

    /// <summary>Раздел ответа API: current, hourly или daily.</summary>
    public string Section { get; set; } = "";

    /// <summary>Имя переменной Open-Meteo, например temperature_2m.</summary>
    public string VariableName { get; set; } = "";

    /// <summary>Время измерения или дата прогноза в часовом поясе ответа.</summary>
    public string Timestamp { get; set; } = "";

    /// <summary>Числовое значение показателя; null, если API не вернуло значение.</summary>
    public double? NumericValue { get; set; }

    /// <summary>Текстовое значение показателя, например время восхода или заката.</summary>
    public string? TextValue { get; set; }

    /// <summary>Единица измерения, сообщённая API для этой переменной.</summary>
    public string? Unit { get; set; }
}
