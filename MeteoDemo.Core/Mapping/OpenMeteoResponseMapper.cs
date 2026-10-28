using System.Text.Json;
using MeteoDemo.Core.Models;

namespace MeteoDemo.Core.Mapping;

/// <summary>Преобразует JSON-ответ Open-Meteo в типизированные DTO приложения.</summary>
public static class OpenMeteoResponseMapper
{
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

    private static string ReadString(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Open-Meteo response property '{propertyName}' must be a string.");
        }

        return value.GetString()!;
    }

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

    private static string[]? ReadOptionalStringArray(JsonElement source, string propertyName)
    {
        if (!source.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return ReadStringArray(source, propertyName);
    }

    private static double?[] ReadNullableDoubleArray(JsonElement source, string propertyName)
    {
        if (source.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"Open-Meteo response property '{propertyName}' must be an array.");
        }

        return source.EnumerateArray().Select(value => ReadNullableDouble(value, propertyName)).ToArray();
    }

    private static double? ReadNullableDouble(JsonElement source, string propertyName) =>
        source.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Number when source.TryGetDouble(out var number) => number,
            _ => throw new JsonException($"Open-Meteo response property '{propertyName}' must contain numbers or null.")
        };
}
