namespace MeteoDemo.Data.Entities;

/// <summary>Отдельно сохранённый ответ API с суточной сводкой прогноза.</summary>
public sealed class DailyWeatherSnapshot
{
    /// <summary>Первичный ключ снимка суточного прогноза.</summary>
    public long Id { get; set; }

    /// <summary>Метаданные запроса, при котором получена суточная сводка.</summary>
    public WeatherSnapshotMetadata Metadata { get; set; } = new();

    /// <summary>Суточные показатели этого ответа.</summary>
    public ICollection<DailyWeatherMeasurement> Measurements { get; set; } = new List<DailyWeatherMeasurement>();
}
