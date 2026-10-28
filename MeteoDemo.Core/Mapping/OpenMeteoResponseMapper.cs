using System.Text.Json;
using MeteoDemo.Core.Models;

namespace MeteoDemo.Core.Mapping;

/// <summary>Преобразует JSON-ответ Open-Meteo в типизированные DTO приложения.</summary>
public static class OpenMeteoResponseMapper
{
    /// <summary>Преобразует JSON-объект API в модель ответа приложения.</summary>
    /// <param name="source">Корневой JSON-объект ответа Open-Meteo.</param>
    /// <returns>Типизированный ответ с метаданными, текущими, почасовыми и дневными данными.</returns>
    /// <exception cref="JsonException">Ответ имеет неожиданный тип или содержит некорректное поле.</exception>
    public static OpenMeteoResponse Map(JsonElement source)
    {
        if (source.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("Open-Meteo response must be a JSON object.");
        }

        return new OpenMeteoResponse
        {
            Latitude = ReadDouble(source, "latitude"),
            Longitude = ReadDouble(source, "longitude"),
            Elevation = ReadDouble(source, "elevation"),
            GenerationTimeMilliseconds = ReadDouble(source, "generationtime_ms"),
            UtcOffsetSeconds = ReadInt32(source, "utc_offset_seconds"),
            Timezone = ReadString(source, "timezone"),
            TimezoneAbbreviation = ReadString(source, "timezone_abbreviation"),
            CurrentUnits = MapUnits(source, "current_units"),
            Current = MapCurrent(source, "current"),
            HourlyUnits = MapUnits(source, "hourly_units"),
            Hourly = MapHourly(source, "hourly"),
            DailyUnits = MapUnits(source, "daily_units"),
            Daily = MapDaily(source, "daily")
        };
    }

    /// <summary>Преобразует объект единиц измерения соответствующего раздела.</summary>
    /// <param name="source">JSON-объект ответа API.</param>
    /// <param name="propertyName">Имя поля с единицами измерения.</param>
    /// <returns>Сопоставление имени показателя с единицей или null, если поле отсутствует.</returns>
    private static WeatherUnits? MapUnits(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var unitsElement) ||
            unitsElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var units = new WeatherUnits();
        foreach (var property in unitsElement.EnumerateObject())
        {
            units.Values[property.Name] = property.Value.GetString()
                                          ?? throw new JsonException($"Unit '{property.Name}' must be a string.");
        }

        return units;
    }

    /// <summary>Преобразует блок текущих погодных показателей.</summary>
    /// <param name="source">JSON-объект ответа API.</param>
    /// <param name="propertyName">Имя поля с текущими данными.</param>
    /// <returns>Текущие показатели или null, если блок отсутствует.</returns>
    private static CurrentWeatherData? MapCurrent(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var dataElement) ||
            dataElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var data = new CurrentWeatherData
        {
            Time = ReadString(dataElement, "time"),
            IntervalSeconds = ReadOptionalInt32(dataElement, "interval")
        };

        foreach (var property in dataElement.EnumerateObject())
        {
            if (property.Name is "time" or "interval")
            {
                continue;
            }

            data.Values[property.Name] = ReadNullableDouble(property.Value, property.Name);
        }

        return data;
    }

    /// <summary>Преобразует блок почасовых значений в массивы показателей.</summary>
    /// <param name="source">JSON-объект ответа API.</param>
    /// <param name="propertyName">Имя поля с почасовыми данными.</param>
    /// <returns>Почасовой прогноз или null, если блок отсутствует.</returns>
    private static HourlyWeatherData? MapHourly(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var dataElement) ||
            dataElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var data = new HourlyWeatherData
        {
            Time = ReadStringArray(dataElement, "time"),
            IntervalSeconds = ReadOptionalInt32(dataElement, "interval")
        };

        foreach (var property in dataElement.EnumerateObject())
        {
            if (property.Name is "time" or "interval")
            {
                continue;
            }

            data.Values[property.Name] = ReadNullableDoubleArray(property.Value, property.Name);
        }

        return data;
    }

    /// <summary>Преобразует блок суточных значений и времён восхода и заката.</summary>
    /// <param name="source">JSON-объект ответа API.</param>
    /// <param name="propertyName">Имя поля с суточными данными.</param>
    /// <returns>Суточная сводка или null, если блок отсутствует.</returns>
    private static DailyWeatherData? MapDaily(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var dataElement) ||
            dataElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var data = new DailyWeatherData
        {
            Time = ReadStringArray(dataElement, "time"),
            Sunrise = ReadOptionalStringArray(dataElement, "sunrise"),
            Sunset = ReadOptionalStringArray(dataElement, "sunset")
        };

        foreach (var property in dataElement.EnumerateObject())
        {
            if (property.Name is "time" or "sunrise" or "sunset")
            {
                continue;
            }

            data.Values[property.Name] = ReadNullableDoubleArray(property.Value, property.Name);
        }

        return data;
    }

    /// <summary>Читает обязательное числовое поле типа double.</summary>
    /// <param name="source">JSON-объект, содержащий поле.</param>
    /// <param name="propertyName">Имя числового поля.</param>
    /// <returns>Прочитанное число.</returns>
    private static double ReadDouble(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Number ||
            !value.TryGetDouble(out var number))
        {
            throw new JsonException($"Open-Meteo response property '{propertyName}' must be a number.");
        }

        return number;
    }

    /// <summary>Читает обязательное целочисленное поле.</summary>
    /// <param name="source">JSON-объект, содержащий поле.</param>
    /// <param name="propertyName">Имя целочисленного поля.</param>
    /// <returns>Прочитанное целое число.</returns>
    private static int ReadInt32(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Number ||
            !value.TryGetInt32(out var number))
        {
            throw new JsonException($"Open-Meteo response property '{propertyName}' must be an integer.");
        }

        return number;
    }

    /// <summary>Читает необязательное целочисленное поле.</summary>
    /// <param name="source">JSON-объект, содержащий поле.</param>
    /// <param name="propertyName">Имя целочисленного поля.</param>
    /// <returns>Целое число либо null, если поле отсутствует или равно null.</returns>
    private static int? ReadOptionalInt32(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
        {
            throw new JsonException($"Open-Meteo response property '{propertyName}' must be an integer when present.");
        }

        return number;
    }

    /// <summary>Читает обязательное строковое поле.</summary>
    /// <param name="source">JSON-объект, содержащий поле.</param>
    /// <param name="propertyName">Имя строкового поля.</param>
    /// <returns>Значение поля.</returns>
    private static string ReadString(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Open-Meteo response property '{propertyName}' must be a string.");
        }

        return value.GetString()!;
    }

    /// <summary>Читает обязательный массив строк.</summary>
    /// <param name="source">JSON-объект, содержащий массив.</param>
    /// <param name="propertyName">Имя поля-массива.</param>
    /// <returns>Массив строковых значений.</returns>
    private static string[] ReadStringArray(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"Open-Meteo response property '{propertyName}' must be an array.");
        }

        return value.EnumerateArray().Select(item =>
            item.ValueKind == JsonValueKind.String
                ? item.GetString()!
                : throw new JsonException($"Open-Meteo response array '{propertyName}' must contain strings."))
            .ToArray();
    }

    /// <summary>Читает необязательный массив строк.</summary>
    /// <param name="source">JSON-объект, содержащий массив.</param>
    /// <param name="propertyName">Имя поля-массива.</param>
    /// <returns>Массив строк или null, если поле отсутствует или равно null.</returns>
    private static string[]? ReadOptionalStringArray(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return ReadStringArray(source, propertyName);
    }

    /// <summary>Читает массив чисел, допускающий null для отдельных значений.</summary>
    /// <param name="source">JSON-массив показателей.</param>
    /// <param name="propertyName">Имя показателя для сообщения об ошибке.</param>
    /// <returns>Массив чисел и null-значений.</returns>
    private static double?[] ReadNullableDoubleArray(JsonElement source, string propertyName)
    {
        if (source.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"Open-Meteo response property '{propertyName}' must be an array.");
        }

        return source.EnumerateArray().Select(value => ReadNullableDouble(value, propertyName)).ToArray();
    }

    /// <summary>Читает число или null из значения JSON.</summary>
    /// <param name="source">JSON-значение показателя.</param>
    /// <param name="propertyName">Имя показателя для сообщения об ошибке.</param>
    /// <returns>Числовое значение либо null.</returns>
    private static double? ReadNullableDouble(JsonElement source, string propertyName) =>
        source.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Number when source.TryGetDouble(out var number) => number,
            _ => throw new JsonException($"Open-Meteo response property '{propertyName}' must contain numbers or null.")
        };
}
