using System.Text.Json.Serialization;

namespace MeteoDemo.Core.Models;

/// <summary>Ответ API прогноза Open-Meteo с метаданными, текущими, почасовыми и дневными данными.</summary>
public sealed class OpenMeteoResponse
{
    /// <summary>Широта точки прогноза в градусах.</summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    /// <summary>Долгота точки прогноза в градусах.</summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    /// <summary>Высота точки над уровнем моря в метрах.</summary>
    [JsonPropertyName("elevation")]
    public double Elevation { get; init; }

    /// <summary>Время генерации ответа сервером в миллисекундах.</summary>
    [JsonPropertyName("generationtime_ms")]
    public double GenerationTimeMilliseconds { get; init; }

    /// <summary>Смещение локального времени от UTC в секундах.</summary>
    [JsonPropertyName("utc_offset_seconds")]
    public int UtcOffsetSeconds { get; init; }

    /// <summary>Часовой пояс, для которого представлены временные метки.</summary>
    [JsonPropertyName("timezone")]
    public string Timezone { get; init; } = "";

    /// <summary>Сокращённое обозначение часового пояса.</summary>
    [JsonPropertyName("timezone_abbreviation")]
    public string TimezoneAbbreviation { get; init; } = "";

    /// <summary>Единицы измерения полей объекта current.</summary>
    [JsonPropertyName("current_units")]
    public WeatherUnits? CurrentUnits { get; init; }

    /// <summary>Значения выбранных текущих метеорологических показателей.</summary>
    [JsonPropertyName("current")]
    public CurrentWeatherData? Current { get; init; }

    /// <summary>Единицы измерения полей объекта hourly.</summary>
    [JsonPropertyName("hourly_units")]
    public WeatherUnits? HourlyUnits { get; init; }

    /// <summary>Почасовые значения прогноза для выбранных показателей.</summary>
    [JsonPropertyName("hourly")]
    public HourlyWeatherData? Hourly { get; init; }

    /// <summary>Единицы измерения полей объекта daily.</summary>
    [JsonPropertyName("daily_units")]
    public WeatherUnits? DailyUnits { get; init; }

    /// <summary>Суточные значения прогноза для выбранных показателей.</summary>
    [JsonPropertyName("daily")]
    public DailyWeatherData? Daily { get; init; }
}
