namespace MeteoDemo.Console.Configuration;

/// <summary>Cron-расписания независимых запросов погоды, интерпретируемые в UTC.</summary>
internal sealed class WeatherScheduleOptions
{
    /// <summary>Cron-выражение запроса текущей погоды.</summary>
    public required string Current { get; init; }

    /// <summary>Cron-выражение запроса почасового прогноза.</summary>
    public required string Hourly { get; init; }

    /// <summary>Cron-выражение запроса суточного прогноза.</summary>
    public required string Daily { get; init; }
}
