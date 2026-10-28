namespace MeteoDemo.Data.Entities;

/// <summary>Одно значение показателя из почасового прогноза.</summary>
public sealed class HourlyWeatherMeasurement
{
    /// <summary>Первичный ключ измерения.</summary>
    public long Id { get; set; }

    /// <summary>Внешний ключ снимка почасового прогноза.</summary>
    public long HourlyWeatherSnapshotId { get; set; }

    /// <summary>Навигационное свойство к снимку почасового прогноза.</summary>
    public HourlyWeatherSnapshot Snapshot { get; set; } = null!;

    /// <summary>Имя переменной Open-Meteo, например temperature_2m.</summary>
    public string VariableName { get; set; } = "";

    /// <summary>Временная метка в часовом поясе ответа.</summary>
    public string Timestamp { get; set; } = "";

    /// <summary>Числовое значение показателя; null, если API не вернуло значение.</summary>
    public double? NumericValue { get; set; }

    /// <summary>Единица измерения, сообщённая API для этой переменной.</summary>
    public string? Unit { get; set; }
}
