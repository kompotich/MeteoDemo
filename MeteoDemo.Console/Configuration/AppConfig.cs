using MeteoDemo.Core.Configuration;

namespace MeteoDemo.Console.Configuration;

/// <summary>Корневая конфигурация консольного приложения.</summary>
internal sealed class AppConfig
{
    /// <summary>Параметры получения погоды от Open-Meteo.</summary>
    public required OpenMeteoOptions OpenMeteo { get; init; }

    /// <summary>Cron-расписания запуска запросов погоды.</summary>
    public required WeatherScheduleOptions Schedules { get; init; }
}
