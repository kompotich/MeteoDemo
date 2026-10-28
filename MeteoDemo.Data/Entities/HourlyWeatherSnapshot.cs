namespace MeteoDemo.Data.Entities;

/// <summary>Отдельно сохранённый ответ API с почасовым прогнозом.</summary>
public sealed class HourlyWeatherSnapshot
{
    /// <summary>Первичный ключ снимка почасового прогноза.</summary>
    public long Id { get; set; }

    /// <summary>Метаданные запроса, при котором получен почасовой прогноз.</summary>
    public WeatherSnapshotMetadata Metadata { get; set; } = new();

    /// <summary>Длительность шага почасового прогноза в секундах, если предоставлена API.</summary>
    public int? IntervalSeconds { get; set; }

    /// <summary>Почасовые показатели этого ответа.</summary>
    public ICollection<HourlyWeatherMeasurement> Measurements { get; set; } = new List<HourlyWeatherMeasurement>();
}
