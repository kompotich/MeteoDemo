using MeteoDemo.Core.Models;
using MeteoDemo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeteoDemo.Data;

/// <summary>Сохраняет ответы Open-Meteo и связанные с ними измерения в SQLite.</summary>
/// <param name="dbContext">Контекст EF Core для записи снимка и измерений.</param>
public sealed class WeatherDataStore(WeatherDbContext dbContext)
{
    /// <summary>Сохраняет один ответ API вместе со всеми его текущими, почасовыми и суточными показателями.</summary>
    /// <param name="response">Преобразованный ответ Open-Meteo.</param>
    /// <param name="cancellationToken">Токен отмены записи.</param>
    /// <returns>Идентификатор созданного снимка прогноза.</returns>
    /// <exception cref="InvalidOperationException">Количество значений показателя не совпадает с количеством временных меток.</exception>
    public async Task<long> SaveAsync(OpenMeteoResponse response, CancellationToken cancellationToken = default)
    {
        var snapshot = new WeatherSnapshot
        {
            FetchedAtUtc = DateTime.UtcNow,
            Latitude = response.Latitude,
            Longitude = response.Longitude,
            Elevation = response.Elevation,
            GenerationTimeMilliseconds = response.GenerationTimeMilliseconds,
            UtcOffsetSeconds = response.UtcOffsetSeconds,
            Timezone = response.Timezone,
            TimezoneAbbreviation = response.TimezoneAbbreviation,
            CurrentIntervalSeconds = response.Current?.IntervalSeconds,
            HourlyIntervalSeconds = response.Hourly?.IntervalSeconds
        };

        AddCurrentMeasurements(snapshot, response.Current, response.CurrentUnits);
        AddArrayMeasurements(snapshot, "hourly", response.Hourly?.Time, response.Hourly?.Values, response.HourlyUnits);
        AddDailyMeasurements(snapshot, response.Daily, response.DailyUnits);

        dbContext.WeatherSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot.Id;
    }

    /// <summary>Добавляет к снимку значения текущих показателей.</summary>
    /// <param name="snapshot">Снимок, которому принадлежат измерения.</param>
    /// <param name="current">Текущие значения API или null, если раздел отсутствует.</param>
    /// <param name="units">Единицы измерения текущих значений.</param>
    private static void AddCurrentMeasurements(
        WeatherSnapshot snapshot,
        CurrentWeatherData? current,
        WeatherUnits? units)
    {
        if (current is null)
        {
            return;
        }

        foreach (var (variableName, value) in current.Values)
        {
            snapshot.Measurements.Add(new WeatherMeasurement
            {
                Section = "current",
                VariableName = variableName,
                Timestamp = current.Time,
                NumericValue = value,
                Unit = GetUnit(units, variableName)
            });
        }
    }

    /// <summary>Добавляет в снимок измерения из временного ряда.</summary>
    /// <param name="snapshot">Снимок, которому принадлежат измерения.</param>
    /// <param name="section">Название раздела ответа API.</param>
    /// <param name="timestamps">Временные метки значений или null, если раздел отсутствует.</param>
    /// <param name="values">Массивы значений, индексированные именами переменных.</param>
    /// <param name="units">Единицы измерения показателей.</param>
    /// <exception cref="InvalidOperationException">Для показателя больше значений, чем временных меток.</exception>
    private static void AddArrayMeasurements(
        WeatherSnapshot snapshot,
        string section,
        IReadOnlyList<string>? timestamps,
        IReadOnlyDictionary<string, double?[]>? values,
        WeatherUnits? units)
    {
        if (timestamps is null || values is null)
        {
            return;
        }

        foreach (var (variableName, measurements) in values)
        {
            for (var index = 0; index < measurements.Length; index++)
            {
                if (index >= timestamps.Count)
                {
                    throw new InvalidOperationException(
                        $"The {section} metric '{variableName}' has more values than timestamps.");
                }

                snapshot.Measurements.Add(new WeatherMeasurement
                {
                    Section = section,
                    VariableName = variableName,
                    Timestamp = timestamps[index],
                    NumericValue = measurements[index],
                    Unit = GetUnit(units, variableName)
                });
            }
        }
    }

    /// <summary>Добавляет числовые суточные показатели, время восхода и время заката.</summary>
    /// <param name="snapshot">Снимок, которому принадлежат измерения.</param>
    /// <param name="daily">Суточные данные API или null, если раздел отсутствует.</param>
    /// <param name="units">Единицы измерения суточных показателей.</param>
    private static void AddDailyMeasurements(
        WeatherSnapshot snapshot,
        DailyWeatherData? daily,
        WeatherUnits? units)
    {
        if (daily is null)
        {
            return;
        }

        AddArrayMeasurements(snapshot, "daily", daily.Time, daily.Values, units);
        AddTextMeasurements(snapshot, "sunrise", daily.Time, daily.Sunrise, GetUnit(units, "sunrise"));
        AddTextMeasurements(snapshot, "sunset", daily.Time, daily.Sunset, GetUnit(units, "sunset"));
    }

    /// <summary>Добавляет в снимок временной ряд текстовых значений.</summary>
    /// <param name="snapshot">Снимок, которому принадлежат измерения.</param>
    /// <param name="variableName">Имя текстового показателя.</param>
    /// <param name="timestamps">Даты или временные метки показателя.</param>
    /// <param name="values">Текстовые значения или null, если показатель не запрошен.</param>
    /// <param name="unit">Единица измерения, если она задана API.</param>
    /// <exception cref="InvalidOperationException">Для показателя больше значений, чем временных меток.</exception>
    private static void AddTextMeasurements(
        WeatherSnapshot snapshot,
        string variableName,
        IReadOnlyList<string> timestamps,
        IReadOnlyList<string>? values,
        string? unit)
    {
        if (values is null)
        {
            return;
        }

        for (var index = 0; index < values.Count; index++)
        {
            if (index >= timestamps.Count)
            {
                throw new InvalidOperationException(
                    $"The daily metric '{variableName}' has more values than timestamps.");
            }

            snapshot.Measurements.Add(new WeatherMeasurement
            {
                Section = "daily",
                VariableName = variableName,
                Timestamp = timestamps[index],
                TextValue = values[index],
                Unit = unit
            });
        }
    }

    /// <summary>Возвращает единицу измерения переменной, если она присутствует в ответе API.</summary>
    /// <param name="units">Единицы измерения раздела ответа.</param>
    /// <param name="variableName">Имя переменной Open-Meteo.</param>
    /// <returns>Строка единицы измерения или null, если она не указана.</returns>
    private static string? GetUnit(WeatherUnits? units, string variableName) =>
        units is not null && units.Values.TryGetValue(variableName, out var unit) ? unit : null;
}
