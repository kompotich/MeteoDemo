namespace MeteoDemo.Data.Entities;

/// <summary>Одно значение показателя из ответа с текущей погодой.</summary>
public sealed class CurrentWeatherMeasurement
{
    /// <summary>Первичный ключ измерения.</summary>
    public long Id { get; set; }

    /// <summary>Внешний ключ снимка текущей погоды.</summary>
    public long CurrentWeatherSnapshotId { get; set; }

    /// <summary>Навигационное свойство к снимку текущей погоды.</summary>
    public CurrentWeatherSnapshot Snapshot { get; set; } = null!;

    /// <summary>Имя переменной Open-Meteo, например temperature_2m.</summary>
    public string VariableName { get; set; } = "";

    /// <summary>Время наблюдения в часовом поясе ответа.</summary>
    public string Timestamp { get; set; } = "";

    /// <summary>Числовое значение показателя; null, если API не вернуло значение.</summary>
    public double? NumericValue { get; set; }

    /// <summary>Единица измерения, сообщённая API для этой переменной.</summary>
    public string? Unit { get; set; }
}
