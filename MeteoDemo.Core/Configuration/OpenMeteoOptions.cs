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

    /// <summary>Настройки отдельного запроса текущих погодных условий.</summary>
    public required CurrentWeatherRequestOptions Current { get; init; }

    /// <summary>Настройки отдельного запроса почасового прогноза.</summary>
    public required ForecastWeatherRequestOptions Hourly { get; init; }

    /// <summary>Настройки отдельного запроса суточной сводки.</summary>
    public required ForecastWeatherRequestOptions Daily { get; init; }

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

/// <summary>Список переменных, запрашиваемых для текущих погодных условий.</summary>
public sealed class CurrentWeatherRequestOptions
{
    /// <summary>Имена текущих переменных Open-Meteo.</summary>
    public required string[] Variables { get; init; }
}

/// <summary>Список переменных и временной диапазон запроса прогноза.</summary>
public sealed class ForecastWeatherRequestOptions
{
    /// <summary>Имена почасовых или суточных переменных Open-Meteo.</summary>
    public required string[] Variables { get; init; }

    /// <summary>Количество будущих дней, запрашиваемых для этого прогноза.</summary>
    public int ForecastDays { get; init; } = 3;

    /// <summary>Количество прошедших дней, включаемых в этот прогноз.</summary>
    public int PastDays { get; init; }
}
