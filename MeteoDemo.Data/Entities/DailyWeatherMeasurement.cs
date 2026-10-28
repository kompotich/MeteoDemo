namespace MeteoDemo.Data.Entities;

/// <summary>Одно значение показателя из суточной сводки прогноза.</summary>
public sealed class DailyWeatherMeasurement
{
    /// <summary>Первичный ключ измерения.</summary>
    public long Id { get; set; }

    /// <summary>Внешний ключ снимка суточного прогноза.</summary>
    public long DailyWeatherSnapshotId { get; set; }

    /// <summary>Навигационное свойство к снимку суточного прогноза.</summary>
    public DailyWeatherSnapshot Snapshot { get; set; } = null!;

    /// <summary>Имя переменной Open-Meteo, например temperature_2m_max.</summary>
    public string VariableName { get; set; } = "";

    /// <summary>Дата прогноза в часовом поясе ответа.</summary>
    public string Timestamp { get; set; } = "";

    /// <summary>Числовое значение показателя; null, если API не вернуло значение.</summary>
    public double? NumericValue { get; set; }

    /// <summary>Текстовое значение показателя, например время восхода или заката.</summary>
    public string? TextValue { get; set; }

    /// <summary>Единица измерения, сообщённая API для этой переменной.</summary>
    public string? Unit { get; set; }
}
