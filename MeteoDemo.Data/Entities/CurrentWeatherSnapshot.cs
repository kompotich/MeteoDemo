namespace MeteoDemo.Data.Entities;

/// <summary>Отдельно сохранённый ответ API с текущими погодными условиями.</summary>
public sealed class CurrentWeatherSnapshot
{
    /// <summary>Первичный ключ снимка текущей погоды.</summary>
    public long Id { get; set; }

    /// <summary>Метаданные запроса, при котором получены текущие значения.</summary>
    public WeatherSnapshotMetadata Metadata { get; set; } = new();

    /// <summary>Длительность шага текущих измерений в секундах, если предоставлена API.</summary>
    public int? IntervalSeconds { get; set; }

    /// <summary>Текущие погодные показатели этого ответа.</summary>
    public ICollection<CurrentWeatherMeasurement> Measurements { get; set; } = new List<CurrentWeatherMeasurement>();
}
