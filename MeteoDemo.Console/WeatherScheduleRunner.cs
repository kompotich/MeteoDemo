using Cronos;
using MeteoDemo.Console.Configuration;
using MeteoDemo.Core;
using MeteoDemo.Data;
using Microsoft.Extensions.DependencyInjection;

namespace MeteoDemo.Console;

/// <summary>Запускает независимые запросы погоды согласно cron-расписаниям в UTC.</summary>
/// <param name="serviceScopeFactory">Фабрика областей зависимостей для отдельных запусков.</param>
/// <param name="schedules">Настройки cron-расписаний.</param>
internal sealed class WeatherScheduleRunner(
    IServiceScopeFactory serviceScopeFactory,
    WeatherScheduleOptions schedules)
{
    private readonly CronExpression _currentSchedule = CronExpression.Parse(schedules.Current);
    private readonly CronExpression _hourlySchedule = CronExpression.Parse(schedules.Hourly);
    private readonly CronExpression _dailySchedule = CronExpression.Parse(schedules.Daily);

    /// <summary>Ожидает срабатывания расписаний и выполняет соответствующие запросы до отмены.</summary>
    /// <param name="cancellationToken">Токен остановки фоновых расписаний.</param>
    /// <returns>Асинхронная операция работы планировщика.</returns>
    public Task RunAsync(CancellationToken cancellationToken)
    {
        var currentTask = RunScheduleAsync(
            _currentSchedule,
            async (services, token) =>
            {
                var client = services.GetRequiredService<OpenMeteoClient>();
                var store = services.GetRequiredService<WeatherDataStore>();
                var weatherConsole = services.GetRequiredService<WeatherConsole>();
                var response = await client.GetCurrentWeatherAsync(token);
                await store.SaveCurrentAsync(response, token);
                weatherConsole.DisplayCurrentWeather(response);
            },
            cancellationToken);

        var hourlyTask = RunScheduleAsync(
            _hourlySchedule,
            async (services, token) =>
            {
                var client = services.GetRequiredService<OpenMeteoClient>();
                var store = services.GetRequiredService<WeatherDataStore>();
                var weatherConsole = services.GetRequiredService<WeatherConsole>();
                var response = await client.GetHourlyWeatherAsync(token);
                await store.SaveHourlyAsync(response, token);
                weatherConsole.DisplayHourlyWeather(response);
            },
            cancellationToken);

        var dailyTask = RunScheduleAsync(
            _dailySchedule,
            async (services, token) =>
            {
                var client = services.GetRequiredService<OpenMeteoClient>();
                var store = services.GetRequiredService<WeatherDataStore>();
                var weatherConsole = services.GetRequiredService<WeatherConsole>();
                var response = await client.GetDailyWeatherAsync(token);
                await store.SaveDailyAsync(response, token);
                weatherConsole.DisplayDailyWeather(response);
            },
            cancellationToken);

        return Task.WhenAll(currentTask, hourlyTask, dailyTask);
    }

    /// <summary>Запускает обработчик при каждом очередном срабатывании cron-выражения.</summary>
    /// <param name="schedule">Cron-расписание.</param>
    /// <param name="handler">Запрос, сохранение и вывод одного ответа.</param>
    /// <param name="cancellationToken">Токен остановки расписания.</param>
    /// <returns>Асинхронная операция расписания.</returns>
    private async Task RunScheduleAsync(
        CronExpression schedule,
        Func<IServiceProvider, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var nextOccurrence = schedule.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc)
                ?? throw new InvalidOperationException("The cron schedule has no future occurrence.");
            var delay = nextOccurrence - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            await using var scope = serviceScopeFactory.CreateAsyncScope();
            await handler(scope.ServiceProvider, cancellationToken);
        }
    }
}
