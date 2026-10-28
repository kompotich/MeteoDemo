using MeteoDemo.Core.Models;
using MeteoDemo.Data.Entities;

namespace MeteoDemo.Data;

/// <summary>Сохраняет отдельно полученные ответы Open-Meteo в соответствующих таблицах SQLite.</summary>
/// <param name="dbContext">Контекст EF Core для записи снимков прогноза.</param>
public sealed class WeatherDataStore(WeatherDbContext dbContext)
{
    /// <summary>Сохраняет только текущие погодные показатели и метаданные их ответа.</summary>
    /// <param name="response">Ответ Open-Meteo с заполненным разделом current.</param>
    /// <param name="cancellationToken">Токен отмены записи.</param>
    /// <returns>Идентификатор сохранённого снимка текущей погоды.</returns>
    /// <exception cref="ArgumentNullException">Ответ равен null.</exception>
    /// <exception cref="InvalidOperationException">Ответ не содержит current.</exception>
    public async Task<long> SaveCurrentAsync(
        OpenMeteoResponse response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.Current is not { } current)
        {
            throw new InvalidOperationException("The Open-Meteo response does not contain current weather data.");
        }

        var snapshot = new CurrentWeatherSnapshot
        {
            Metadata = CreateMetadata(response),
            IntervalSeconds = current.IntervalSeconds
        };

        AddCurrentMeasurements(snapshot, current, response.CurrentUnits);

        dbContext.CurrentWeatherSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot.Id;
    }

    /// <summary>Сохраняет только почасовые показатели и метаданные их ответа.</summary>
    /// <param name="response">Ответ Open-Meteo с заполненным разделом hourly.</param>
    /// <param name="cancellationToken">Токен отмены записи.</param>
    /// <returns>Идентификатор сохранённого снимка почасового прогноза.</returns>
    /// <exception cref="ArgumentNullException">Ответ равен null.</exception>
    /// <exception cref="InvalidOperationException">Ответ не содержит hourly или длины временных рядов не совпадают.</exception>
    public async Task<long> SaveHourlyAsync(
        OpenMeteoResponse response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.Hourly is not { } hourly)
        {
            throw new InvalidOperationException("The Open-Meteo response does not contain hourly weather data.");
        }

        var snapshot = new HourlyWeatherSnapshot
        {
            Metadata = CreateMetadata(response),
            IntervalSeconds = hourly.IntervalSeconds
        };

        AddHourlyMeasurements(snapshot, hourly, response.HourlyUnits);

        dbContext.HourlyWeatherSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot.Id;
    }

    /// <summary>Сохраняет только суточные показатели и метаданные их ответа.</summary>
    /// <param name="response">Ответ Open-Meteo с заполненным разделом daily.</param>
    /// <param name="cancellationToken">Токен отмены записи.</param>
    /// <returns>Идентификатор сохранённого снимка суточного прогноза.</returns>
    /// <exception cref="ArgumentNullException">Ответ равен null.</exception>
    /// <exception cref="InvalidOperationException">Ответ не содержит daily или длины временных рядов не совпадают.</exception>
    public async Task<long> SaveDailyAsync(
        OpenMeteoResponse response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.Daily is not { } daily)
        {
            throw new InvalidOperationException("The Open-Meteo response does not contain daily weather data.");
        }

        var snapshot = new DailyWeatherSnapshot
        {
            Metadata = CreateMetadata(response)
        };

        AddDailyMeasurements(snapshot, daily, response.DailyUnits);

        dbContext.DailyWeatherSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(cancellationToken);
        return snapshot.Id;
    }

    /// <summary>Создаёт метаданные отдельного ответа API.</summary>
    /// <param name="response">Ответ Open-Meteo.</param>
    /// <returns>Метаданные снимка с временем получения и координатами ответа.</returns>
    private static WeatherSnapshotMetadata CreateMetadata(OpenMeteoResponse response) => new()
    {
        FetchedAtUtc = DateTime.UtcNow,
        Latitude = response.Latitude,
        Longitude = response.Longitude,
        Elevation = response.Elevation,
        GenerationTimeMilliseconds = response.GenerationTimeMilliseconds,
        UtcOffsetSeconds = response.UtcOffsetSeconds,
        Timezone = response.Timezone,
        TimezoneAbbreviation = response.TimezoneAbbreviation
    };

    /// <summary>Добавляет текущие показатели в снимок.</summary>
    /// <param name="snapshot">Снимок текущей погоды.</param>
    /// <param name="current">Текущие показатели из ответа API.</param>
    /// <param name="units">Единицы измерения текущих показателей.</param>
    private static void AddCurrentMeasurements(
        CurrentWeatherSnapshot snapshot,
        CurrentWeatherData current,
        WeatherUnits? units)
    {
        foreach (var (variableName, value) in current.Values)
        {
            snapshot.Measurements.Add(new CurrentWeatherMeasurement
            {
                VariableName = variableName,
                Timestamp = current.Time,
                NumericValue = value,
                Unit = GetUnit(units, variableName)
            });
        }
    }

    /// <summary>Добавляет почасовые показатели в снимок с проверкой количества временных меток.</summary>
    /// <param name="snapshot">Снимок почасового прогноза.</param>
    /// <param name="hourly">Почасовые значения из ответа API.</param>
    /// <param name="units">Единицы измерения почасовых показателей.</param>
    private static void AddHourlyMeasurements(
        HourlyWeatherSnapshot snapshot,
        HourlyWeatherData hourly,
        WeatherUnits? units)
    {
        foreach (var (variableName, measurements) in hourly.Values)
        {
            EnsureMatchingLengths("hourly", variableName, hourly.Time.Length, measurements.Length);

            for (var index = 0; index < measurements.Length; index++)
            {
                snapshot.Measurements.Add(new HourlyWeatherMeasurement
                {
                    VariableName = variableName,
                    Timestamp = hourly.Time[index],
                    NumericValue = measurements[index],
                    Unit = GetUnit(units, variableName)
                });
            }
        }
    }

    /// <summary>Добавляет суточные числовые показатели в сущности измерений.</summary>
    /// <param name="measurements">Коллекция для добавления суточных измерений.</param>
    /// <param name="timestamps">Даты суточного прогноза.</param>
    /// <param name="values">Массивы значений показателей.</param>
    /// <param name="units">Единицы измерения суточных показателей.</param>
    private static void AddNumericMeasurements(
        ICollection<DailyWeatherMeasurement> measurements,
        IReadOnlyList<string> timestamps,
        IReadOnlyDictionary<string, double?[]> values,
        WeatherUnits? units)
    {
        foreach (var (variableName, items) in values)
        {
            EnsureMatchingLengths("daily", variableName, timestamps.Count, items.Length);

            for (var index = 0; index < items.Length; index++)
            {
                measurements.Add(new DailyWeatherMeasurement
                {
                    VariableName = variableName,
                    Timestamp = timestamps[index],
                    NumericValue = items[index],
                    Unit = GetUnit(units, variableName)
                });
            }
        }
    }

    /// <summary>Добавляет числовые и текстовые суточные показатели в снимок.</summary>
    /// <param name="snapshot">Снимок суточного прогноза.</param>
    /// <param name="daily">Суточные показатели из ответа API.</param>
    /// <param name="units">Единицы измерения суточных показателей.</param>
    private static void AddDailyMeasurements(
        DailyWeatherSnapshot snapshot,
        DailyWeatherData daily,
        WeatherUnits? units)
    {
        AddNumericMeasurements(snapshot.Measurements, daily.Time, daily.Values, units);
        AddTextMeasurements(
            snapshot.Measurements,
            daily.Time,
            "sunrise",
            daily.Sunrise,
            GetUnit(units, "sunrise"));
        AddTextMeasurements(
            snapshot.Measurements,
            daily.Time,
            "sunset",
            daily.Sunset,
            GetUnit(units, "sunset"));
    }

    /// <summary>Добавляет текстовые суточные показатели, например время восхода и заката.</summary>
    /// <param name="measurements">Коллекция для добавления суточных измерений.</param>
    /// <param name="timestamps">Даты суточного прогноза.</param>
    /// <param name="variableName">Имя текстового показателя.</param>
    /// <param name="values">Значения показателя или null, если он не был запрошен.</param>
    /// <param name="unit">Единица измерения, если она указана API.</param>
    private static void AddTextMeasurements(
        ICollection<DailyWeatherMeasurement> measurements,
        IReadOnlyList<string> timestamps,
        string variableName,
        IReadOnlyList<string>? values,
        string? unit)
    {
        if (values is null)
        {
            return;
        }

        EnsureMatchingLengths("daily", variableName, timestamps.Count, values.Count);

        for (var index = 0; index < values.Count; index++)
        {
            measurements.Add(new DailyWeatherMeasurement
            {
                VariableName = variableName,
                Timestamp = timestamps[index],
                TextValue = values[index],
                Unit = unit
            });
        }
    }

    /// <summary>Проверяет соответствие числа значений числу временных меток.</summary>
    /// <param name="section">Название раздела ответа API.</param>
    /// <param name="variableName">Имя переменной с несовпадающей длиной.</param>
    /// <param name="timestampCount">Число временных меток.</param>
    /// <param name="valueCount">Число значений переменной.</param>
    /// <exception cref="InvalidOperationException">Количество значений не совпадает с количеством временных меток.</exception>
    private static void EnsureMatchingLengths(
        string section,
        string variableName,
        int timestampCount,
        int valueCount)
    {
        if (timestampCount != valueCount)
        {
            throw new InvalidOperationException(
                $"The {section} metric '{variableName}' has {valueCount} values but {timestampCount} timestamps.");
        }
    }

    /// <summary>Возвращает единицу измерения переменной, если она присутствует в ответе API.</summary>
    /// <param name="units">Единицы измерения раздела ответа.</param>
    /// <param name="variableName">Имя переменной Open-Meteo.</param>
    /// <returns>Строка единицы измерения или null, если она не указана.</returns>
    private static string? GetUnit(WeatherUnits? units, string variableName) =>
        units is not null && units.Values.TryGetValue(variableName, out var unit) ? unit : null;
}
