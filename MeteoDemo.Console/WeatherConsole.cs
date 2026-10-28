using System.Globalization;
using MeteoDemo.Core;
using MeteoDemo.Core.Models;
using MeteoDemo.Data;

namespace MeteoDemo.Console;

/// <summary>Выводит прогноз Open-Meteo на русском языке и сохраняет ответ в базе данных.</summary>
/// <param name="openMeteoClient">Клиент для запроса прогноза.</param>
/// <param name="weatherDataStore">Хранилище для сохранения полученного снимка.</param>
internal sealed class WeatherConsole(OpenMeteoClient openMeteoClient, WeatherDataStore weatherDataStore)
{
    private static readonly CultureInfo RussianCulture = CultureInfo.GetCultureInfo("ru-RU");

    private static readonly IReadOnlyDictionary<string, (string Name, string Description)> MeasurementDescriptions =
        new Dictionary<string, (string, string)>
        {
            ["temperature_2m"] = ("Температура воздуха на высоте 2 м", "Температура воздуха на высоте двух метров над поверхностью."),
            ["relative_humidity_2m"] = ("Относительная влажность на высоте 2 м", "Отношение количества водяного пара в воздухе к максимально возможному при этой температуре."),
            ["dew_point_2m"] = ("Точка росы на высоте 2 м", "Температура, до которой должен охладиться воздух, чтобы водяной пар начал конденсироваться."),
            ["apparent_temperature"] = ("Ощущаемая температура", "Расчётное ощущение температуры с учётом ветра, влажности и солнечного излучения."),
            ["is_day"] = ("День или ночь", "1 означает светлое время суток, 0 — ночь."),
            ["precipitation_probability"] = ("Вероятность осадков", "Вероятность осадков более 0,1 мм за соответствующий час."),
            ["precipitation"] = ("Количество осадков", "Сумма дождя, ливневых осадков и снега в водном эквиваленте за предшествующий интервал."),
            ["rain"] = ("Дождь", "Количество дождя от крупномасштабных погодных систем за предшествующий интервал."),
            ["showers"] = ("Ливневые осадки", "Количество конвективных ливневых осадков за предшествующий интервал."),
            ["snowfall"] = ("Снегопад", "Высота выпавшего снега; для пересчёта в водный эквивалент Open-Meteo указывает деление на 7."),
            ["snow_depth"] = ("Высота снежного покрова", "Глубина снега на поверхности земли."),
            ["weather_code"] = ("Код погодных условий WMO", "Числовой код Всемирной метеорологической организации; ниже выводится его расшифровка."),
            ["cloud_cover"] = ("Облачность", "Доля неба, покрытая облаками."),
            ["cloud_cover_low"] = ("Низкая облачность", "Облачность и туман на высоте до 3 км."),
            ["cloud_cover_mid"] = ("Средняя облачность", "Облачность на высоте от 3 до 8 км."),
            ["cloud_cover_high"] = ("Высокая облачность", "Облачность выше 8 км."),
            ["pressure_msl"] = ("Давление, приведённое к уровню моря", "Атмосферное давление, пересчитанное к среднему уровню моря."),
            ["surface_pressure"] = ("Давление у поверхности", "Атмосферное давление на высоте рассматриваемой поверхности."),
            ["wind_speed_10m"] = ("Скорость ветра на высоте 10 м", "Скорость ветра на стандартной метеорологической высоте 10 метров."),
            ["wind_speed_80m"] = ("Скорость ветра на высоте 80 м", "Скорость ветра на высоте 80 метров над поверхностью."),
            ["wind_speed_120m"] = ("Скорость ветра на высоте 120 м", "Скорость ветра на высоте 120 метров над поверхностью."),
            ["wind_speed_180m"] = ("Скорость ветра на высоте 180 м", "Скорость ветра на высоте 180 метров над поверхностью."),
            ["wind_direction_10m"] = ("Направление ветра на высоте 10 м", "Азимут направления, откуда дует ветер: 0° — север, 90° — восток."),
            ["wind_direction_80m"] = ("Направление ветра на высоте 80 м", "Азимут направления, откуда дует ветер: 0° — север, 90° — восток."),
            ["wind_direction_120m"] = ("Направление ветра на высоте 120 м", "Азимут направления, откуда дует ветер: 0° — север, 90° — восток."),
            ["wind_direction_180m"] = ("Направление ветра на высоте 180 м", "Азимут направления, откуда дует ветер: 0° — север, 90° — восток."),
            ["wind_direction_10m_dominant"] = ("Преобладающее направление ветра", "Среднее направление ветра за день в градусах от севера по часовой стрелке."),
            ["wind_gusts_10m"] = ("Порывы ветра на высоте 10 м", "Максимальная скорость порывов ветра на высоте 10 метров за предшествующий час."),
            ["wind_speed_10m_max"] = ("Максимальная скорость ветра за день", "Максимальная скорость ветра на высоте 10 метров за сутки."),
            ["wind_gusts_10m_max"] = ("Максимальные порывы ветра за день", "Максимальная скорость порывов ветра на высоте 10 метров за сутки."),
            ["shortwave_radiation"] = ("Коротковолновое солнечное излучение", "Суммарное солнечное излучение; почасовое значение усредняется за предшествующий час."),
            ["direct_radiation"] = ("Прямое солнечное излучение", "Прямое излучение на горизонтальную поверхность; почасовое значение усредняется за предшествующий час."),
            ["direct_normal_irradiance"] = ("Прямое нормальное излучение", "Прямое излучение на поверхность, перпендикулярную солнечным лучам; почасовое значение усредняется за предыдущий час."),
            ["diffuse_radiation"] = ("Рассеянное солнечное излучение", "Рассеянное излучение; почасовое значение усредняется за предшествующий час."),
            ["global_tilted_irradiance"] = ("Излучение на наклонную поверхность", "Суммарное излучение на панель с заданными наклоном и азимутом."),
            ["terrestrial_radiation"] = ("Внеземное солнечное излучение", "Расчётное излучение на верхней границе атмосферы без учёта облаков и аэрозолей."),
            ["shortwave_radiation_sum"] = ("Сумма коротковолнового излучения за день", "Суммарное солнечное излучение за сутки."),
            ["uv_index"] = ("Ультрафиолетовый индекс", "Интенсивность ультрафиолетового излучения; индекс начинается с 0."),
            ["uv_index_clear_sky"] = ("УФ-индекс при ясном небе", "Расчётный ультрафиолетовый индекс при отсутствии облаков."),
            ["uv_index_max"] = ("Максимальный УФ-индекс за день", "Максимальное значение ультрафиолетового индекса за сутки."),
            ["uv_index_clear_sky_max"] = ("Максимальный УФ-индекс при ясном небе", "Максимальное расчётное значение УФ-индекса без облаков за сутки."),
            ["visibility"] = ("Видимость", "Расстояние видимости с учётом низкой облачности, влажности и аэрозолей."),
            ["evapotranspiration"] = ("Эвапотранспирация", "Испарение с поверхности и транспирация растений с учётом доступной влаги почвы."),
            ["et0_fao_evapotranspiration"] = ("Эталонная эвапотранспирация FAO", "Расчётная потребность в испарении для хорошо увлажнённой травяной поверхности по методике FAO-56."),
            ["vapour_pressure_deficit"] = ("Дефицит давления водяного пара", "Разница между давлением насыщенного водяного пара и фактическим парциальным давлением."),
            ["cape"] = ("Потенциальная энергия конвекции (CAPE)", "Показатель доступной энергии для развития восходящих конвективных потоков."),
            ["freezing_level_height"] = ("Высота нулевой изотермы", "Высота над уровнем моря, на которой температура достигает 0 °C."),
            ["soil_temperature_0cm"] = ("Температура почвы на глубине 0 см", "Температура поверхности земли; над водой соответствует температуре поверхности воды."),
            ["soil_temperature_6cm"] = ("Температура почвы на глубине 6 см", "Температура почвы на глубине 6 сантиметров."),
            ["soil_temperature_18cm"] = ("Температура почвы на глубине 18 см", "Температура почвы на глубине 18 сантиметров."),
            ["soil_temperature_54cm"] = ("Температура почвы на глубине 54 см", "Температура почвы на глубине 54 сантиметров."),
            ["soil_moisture_0_to_1cm"] = ("Влажность почвы на глубине 0–1 см", "Объёмная доля воды в слое почвы от 0 до 1 см."),
            ["soil_moisture_1_to_3cm"] = ("Влажность почвы на глубине 1–3 см", "Объёмная доля воды в слое почвы от 1 до 3 см."),
            ["soil_moisture_3_to_9cm"] = ("Влажность почвы на глубине 3–9 см", "Объёмная доля воды в слое почвы от 3 до 9 см."),
            ["soil_moisture_9_to_27cm"] = ("Влажность почвы на глубине 9–27 см", "Объёмная доля воды в слое почвы от 9 до 27 см."),
            ["soil_moisture_27_to_81cm"] = ("Влажность почвы на глубине 27–81 см", "Объёмная доля воды в слое почвы от 27 до 81 см."),
            ["temperature_2m_max"] = ("Максимальная температура воздуха за день", "Максимальная температура на высоте 2 метров за сутки."),
            ["temperature_2m_min"] = ("Минимальная температура воздуха за день", "Минимальная температура на высоте 2 метров за сутки."),
            ["apparent_temperature_max"] = ("Максимальная ощущаемая температура за день", "Максимальная расчётная ощущаемая температура за сутки."),
            ["apparent_temperature_min"] = ("Минимальная ощущаемая температура за день", "Минимальная расчётная ощущаемая температура за сутки."),
            ["sunrise"] = ("Восход солнца", "Время восхода солнца в указанном часовом поясе."),
            ["sunset"] = ("Закат солнца", "Время заката солнца в указанном часовом поясе."),
            ["daylight_duration"] = ("Продолжительность светового дня", "Длительность времени между восходом и закатом."),
            ["sunshine_duration"] = ("Продолжительность солнечного сияния", "Суммарное время за сутки, когда прямое солнечное излучение превышает 120 Вт/м²."),
            ["precipitation_sum"] = ("Сумма осадков за день", "Суммарное количество осадков за сутки."),
            ["rain_sum"] = ("Сумма дождя за день", "Суммарное количество дождя за сутки."),
            ["showers_sum"] = ("Сумма ливневых осадков за день", "Суммарное количество конвективных осадков за сутки."),
            ["snowfall_sum"] = ("Сумма снегопада за день", "Суммарная высота выпавшего снега за сутки."),
            ["precipitation_hours"] = ("Продолжительность осадков", "Количество часов за сутки, в которые выпадали осадки."),
            ["precipitation_probability_max"] = ("Максимальная вероятность осадков за день", "Наибольшая почасовая вероятность осадков за сутки.")
        };

    private static readonly string[] WindDirections =
        ["север", "северо-северо-восток", "северо-восток", "востоко-северо-восток", "восток", "востоко-юго-восток", "юго-восток", "юго-юго-восток", "юг", "юго-юго-запад", "юго-запад", "западо-юго-запад", "запад", "западо-северо-запад", "северо-запад", "северо-северо-запад"];

    /// <summary>Получает прогноз, сохраняет его и выводит метаданные и показатели в консоль.</summary>
    /// <param name="cancellationToken">Токен отмены запроса, сохранения и вывода прогноза.</param>
    /// <returns>Асинхронная операция получения и отображения прогноза.</returns>
    public async Task DisplayWeatherAsync(CancellationToken cancellationToken = default)
    {
        var weather = await openMeteoClient.GetWeatherAsync(cancellationToken);
        var snapshotId = await weatherDataStore.SaveAsync(weather, cancellationToken);

        global::System.Console.WriteLine("ПРОГНОЗ ПОГОДЫ OPEN-METEO");
        global::System.Console.WriteLine($"Снимок прогноза сохранён в SQLite (ID: {snapshotId}).");
        global::System.Console.WriteLine("Пояснения показателей: https://open-meteo.com/en/docs");
        global::System.Console.WriteLine($"Координаты: {weather.Latitude:F4}, {weather.Longitude:F4}; высота: {weather.Elevation} м");
        global::System.Console.WriteLine($"Часовой пояс: {weather.Timezone} ({weather.TimezoneAbbreviation}), смещение UTC: {weather.UtcOffsetSeconds} с");
        global::System.Console.WriteLine($"Время формирования ответа: {weather.GenerationTimeMilliseconds.ToString("0.##", RussianCulture)} мс");

        if (weather.Current is not null)
        {
            DisplaySection(
                "ТЕКУЩИЕ ПОКАЗАТЕЛИ",
                [weather.Current.Time],
                weather.Current.Values.ToDictionary(pair => pair.Key, pair => new double?[] { pair.Value }),
                weather.CurrentUnits,
                weather.Current.IntervalSeconds);
        }

        if (weather.Hourly is not null)
        {
            DisplaySection("ПОЧАСОВЫЕ ДАННЫЕ", weather.Hourly.Time, weather.Hourly.Values, weather.HourlyUnits, weather.Hourly.IntervalSeconds);
        }

        if (weather.Daily is not null)
        {
            DisplaySection("ДНЕВНАЯ СВОДКА", weather.Daily.Time, weather.Daily.Values, weather.DailyUnits);
            DisplayTextSeries("Восход солнца", "sunrise", weather.Daily.Time, weather.Daily.Sunrise, weather.DailyUnits);
            DisplayTextSeries("Закат солнца", "sunset", weather.Daily.Time, weather.Daily.Sunset, weather.DailyUnits);
        }
    }

    /// <summary>Выводит один раздел прогноза с описаниями переменных, единицами и временными метками.</summary>
    /// <param name="title">Заголовок раздела.</param>
    /// <param name="timestamps">Временные метки измерений.</param>
    /// <param name="values">Значения показателей, индексированные именами переменных API.</param>
    /// <param name="units">Единицы измерения показателей.</param>
    /// <param name="intervalSeconds">Длительность шага данных в секундах, если она задана.</param>
    private static void DisplaySection(
        string title,
        IReadOnlyList<string> timestamps,
        IReadOnlyDictionary<string, double?[]> values,
        WeatherUnits? units,
        int? intervalSeconds = null)
    {
        global::System.Console.WriteLine();
        global::System.Console.WriteLine($"=== {title} ===");
        if (intervalSeconds is > 0)
        {
            global::System.Console.WriteLine($"Интервал измерений: {intervalSeconds} с");
        }

        foreach (var (key, measurements) in values)
        {
            var descriptor = MeasurementDescriptions.TryGetValue(key, out var known)
                ? known
                : (Name: key, Description: "Описание показателя отсутствует в локальном справочнике.");
            var unit = units is not null && units.Values.TryGetValue(key, out var knownUnit)
                ? knownUnit
                : "единица не указана";

            global::System.Console.WriteLine();
            global::System.Console.WriteLine($"{descriptor.Name} [{unit}]");
            global::System.Console.WriteLine($"  {descriptor.Description}");

            for (var index = 0; index < measurements.Length; index++)
            {
                var timestamp = index < timestamps.Count ? timestamps[index] : $"запись {index + 1}";
                global::System.Console.WriteLine($"  {timestamp}: {FormatMeasurement(key, measurements[index])}");
            }
        }
    }

    /// <summary>Выводит текстовый временной ряд, например время восхода или заката солнца.</summary>
    /// <param name="title">Название ряда для консольного вывода.</param>
    /// <param name="key">Имя соответствующей переменной в данных о единицах измерения.</param>
    /// <param name="timestamps">Даты или временные метки значений.</param>
    /// <param name="values">Текстовые значения; null означает, что показатель не запрошен.</param>
    /// <param name="units">Единицы измерения раздела.</param>
    private static void DisplayTextSeries(
        string title,
        string key,
        IReadOnlyList<string> timestamps,
        string[]? values,
        WeatherUnits? units)
    {
        if (values is null)
        {
            return;
        }

        var unit = units is not null && units.Values.TryGetValue(key, out var knownUnit)
            ? knownUnit
            : "единица не указана";
        global::System.Console.WriteLine();
        global::System.Console.WriteLine($"{title} [{unit}]");
        for (var index = 0; index < values.Length; index++)
        {
            var timestamp = index < timestamps.Count ? timestamps[index] : $"запись {index + 1}";
            global::System.Console.WriteLine($"  {timestamp}: {values[index]}");
        }
    }

    /// <summary>Форматирует число с учётом специальных кодов погоды и направления ветра.</summary>
    /// <param name="key">Имя показателя Open-Meteo.</param>
    /// <param name="value">Числовое значение показателя или null.</param>
    /// <returns>Строковое представление значения для консольного вывода.</returns>
    private static string FormatMeasurement(string key, double? value)
    {
        if (value is null)
        {
            return "нет данных";
        }

        if (key == "weather_code")
        {
            var code = (int)value.Value;
            return $"{code} — {DescribeWeatherCode(code)}";
        }

        if (key == "is_day")
        {
            return value == 1 ? "1 — день" : "0 — ночь";
        }

        if (key.StartsWith("wind_direction_", StringComparison.Ordinal))
        {
            var normalizedDegrees = (value.Value % 360 + 360) % 360;
            var direction = WindDirections[(int)Math.Round(normalizedDegrees / 22.5, MidpointRounding.AwayFromZero) % WindDirections.Length];
            return $"{FormatNumber(value.Value)}° ({direction})";
        }

        return FormatNumber(value.Value);
    }

    /// <summary>Форматирует число с использованием русской культуры.</summary>
    /// <param name="value">Числовое значение.</param>
    /// <returns>Строковое представление числа.</returns>
    private static string FormatNumber(double value) => value.ToString("G", RussianCulture);

    /// <summary>Возвращает русское описание кода погоды Всемирной метеорологической организации.</summary>
    /// <param name="code">Числовой код погодных условий WMO.</param>
    /// <returns>Краткое описание погодных условий.</returns>
    private static string DescribeWeatherCode(int code) => code switch
    {
        0 => "ясно",
        1 => "преимущественно ясно",
        2 => "переменная облачность",
        3 => "пасмурно",
        45 => "туман",
        48 => "изморозевый туман",
        51 => "слабая морось",
        53 => "умеренная морось",
        55 => "сильная морось",
        56 => "слабая ледяная морось",
        57 => "сильная ледяная морось",
        61 => "слабый дождь",
        63 => "умеренный дождь",
        65 => "сильный дождь",
        66 => "слабый ледяной дождь",
        67 => "сильный ледяной дождь",
        71 => "слабый снегопад",
        73 => "умеренный снегопад",
        75 => "сильный снегопад",
        77 => "снежные зёрна",
        80 => "слабый ливень",
        81 => "умеренный ливень",
        82 => "сильный ливень",
        85 => "слабый снежный ливень",
        86 => "сильный снежный ливень",
        95 => "гроза",
        96 => "гроза с небольшим градом",
        99 => "гроза с сильным градом",
        _ => "неизвестный код WMO"
    };
}
