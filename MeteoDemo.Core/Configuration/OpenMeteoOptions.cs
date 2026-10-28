namespace MeteoDemo.Core.Configuration;

/// <summary>Параметры запроса прогноза Open-Meteo, загружаемые из конфигурации приложения.</summary>
public sealed class OpenMeteoOptions
{
    /// <summary>Базовый URL конечной точки прогноза.</summary>
    public required string BaseUrl { get; init; }

    /// <summary>Широта точки прогноза в градусах.</summary>
    public double Latitude { get; init; }

    /// <summary>Долгота точки прогноза в градусах.</summary>
    public double Longitude { get; init; }

    /// <summary>Список переменных для блока текущих погодных условий.</summary>
    public required string[] Current { get; init; }

    /// <summary>Список переменных для почасового прогноза.</summary>
    public required string[] Hourly { get; init; }

    /// <summary>Список переменных для суточной сводки.</summary>
    public required string[] Daily { get; init; }

    /// <summary>Количество дней прогноза, запрашиваемых у API.</summary>
    public int ForecastDays { get; init; } = 3;

    /// <summary>Количество прошедших дней, включаемых в ответ.</summary>
    public int PastDays { get; init; }

    /// <summary>Часовой пояс результата; значение auto выбирает пояс координат.</summary>
    public string Timezone { get; init; } = "auto";

    /// <summary>Единицы температуры, например celsius или fahrenheit.</summary>
    public string TemperatureUnit { get; init; } = "celsius";

    /// <summary>Единицы скорости ветра, например kmh, ms, mph или kn.</summary>
    public string WindSpeedUnit { get; init; } = "kmh";

    /// <summary>Единицы количества осадков, например mm или inch.</summary>
    public string PrecipitationUnit { get; init; } = "mm";

    /// <summary>Угол наклона поверхности для расчёта global_tilted_irradiance.</summary>
    public int Tilt { get; init; } = 45;

    /// <summary>Азимут поверхности для расчёта global_tilted_irradiance.</summary>
    public int Azimuth { get; init; }
}
