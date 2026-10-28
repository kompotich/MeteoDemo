namespace MeteoDemo.Data.Entities;

/// <summary>Общие метаданные одного ответа Open-Meteo.</summary>
public sealed class WeatherSnapshotMetadata
{
    /// <summary>Момент получения ответа, сохранённый в UTC.</summary>
    public DateTime FetchedAtUtc { get; set; }

    /// <summary>Широта точки прогноза в градусах.</summary>
    public double Latitude { get; set; }

    /// <summary>Долгота точки прогноза в градусах.</summary>
    public double Longitude { get; set; }

    /// <summary>Высота точки над уровнем моря в метрах.</summary>
    public double Elevation { get; set; }

    /// <summary>Время формирования ответа API в миллисекундах.</summary>
    public double GenerationTimeMilliseconds { get; set; }

    /// <summary>Смещение локального времени от UTC в секундах.</summary>
    public int UtcOffsetSeconds { get; set; }

    /// <summary>Идентификатор часового пояса, использованного API.</summary>
    public string Timezone { get; set; } = "";

    /// <summary>Сокращённое обозначение часового пояса.</summary>
    public string TimezoneAbbreviation { get; set; } = "";
}
