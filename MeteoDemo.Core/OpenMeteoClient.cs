using System.Text.Json;
using MeteoDemo.Core.Configuration;
using MeteoDemo.Core.Mapping;
using MeteoDemo.Core.Models;

namespace MeteoDemo.Core;

/// <summary>Клиент для получения прогноза погоды через публичный API Open-Meteo.</summary>
/// <param name="httpClient">HTTP-клиент, управляемый фабрикой HttpClientFactory.</param>
/// <param name="options">Настройки координат и параметров запроса.</param>
public sealed class OpenMeteoClient(HttpClient httpClient, OpenMeteoOptions options)
{
    /// <summary>Запрашивает текущие погодные условия.</summary>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса и чтения ответа.</param>
    /// <returns>Ответ Open-Meteo с текущими показателями.</returns>
    /// <exception cref="HttpRequestException">Сервер вернул неуспешный HTTP-статус.</exception>
    /// <exception cref="System.Text.Json.JsonException">Ответ API имеет некорректный формат.</exception>
    public Task<OpenMeteoResponse> GetCurrentWeatherAsync(CancellationToken cancellationToken = default)
    {
        var parameters = CreateCommonParameters();
        parameters["current"] = string.Join(',', options.Current.Variables);
        AddSolarPanelParameters(parameters);
        return FetchWeatherAsync(parameters, cancellationToken);
    }

    /// <summary>Запрашивает почасовой прогноз на заданный диапазон дат.</summary>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса и чтения ответа.</param>
    /// <returns>Ответ Open-Meteo с почасовыми показателями.</returns>
    /// <exception cref="HttpRequestException">Сервер вернул неуспешный HTTP-статус.</exception>
    /// <exception cref="System.Text.Json.JsonException">Ответ API имеет некорректный формат.</exception>
    public Task<OpenMeteoResponse> GetHourlyWeatherAsync(CancellationToken cancellationToken = default)
    {
        var parameters = CreateCommonParameters();
        parameters["hourly"] = string.Join(',', options.Hourly.Variables);
        AddForecastRange(parameters, options.Hourly);
        AddSolarPanelParameters(parameters);
        return FetchWeatherAsync(parameters, cancellationToken);
    }

    /// <summary>Запрашивает суточную сводку на заданный диапазон дат.</summary>
    /// <param name="cancellationToken">Токен отмены HTTP-запроса и чтения ответа.</param>
    /// <returns>Ответ Open-Meteo с суточными показателями.</returns>
    /// <exception cref="HttpRequestException">Сервер вернул неуспешный HTTP-статус.</exception>
    /// <exception cref="System.Text.Json.JsonException">Ответ API имеет некорректный формат.</exception>
    public Task<OpenMeteoResponse> GetDailyWeatherAsync(CancellationToken cancellationToken = default)
    {
        var parameters = CreateCommonParameters();
        parameters["daily"] = string.Join(',', options.Daily.Variables);
        AddForecastRange(parameters, options.Daily);
        return FetchWeatherAsync(parameters, cancellationToken);
    }

    /// <summary>Создаёт общие для всех запросов координатные, временные и единичные параметры.</summary>
    /// <returns>Словарь параметров API без списка запрашиваемых переменных.</returns>
    private Dictionary<string, string> CreateCommonParameters() => new()
    {
        ["latitude"] = options.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
        ["longitude"] = options.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
        ["timezone"] = options.Timezone,
        ["temperature_unit"] = options.TemperatureUnit,
        ["wind_speed_unit"] = options.WindSpeedUnit,
        ["precipitation_unit"] = options.PrecipitationUnit
    };

    /// <summary>Добавляет настройки временного диапазона почасового или суточного прогноза.</summary>
    /// <param name="parameters">Словарь параметров запроса API.</param>
    /// <param name="forecast">Настройки диапазона конкретного прогноза.</param>
    private static void AddForecastRange(
        IDictionary<string, string> parameters,
        Configuration.ForecastWeatherRequestOptions forecast)
    {
        parameters["forecast_days"] = forecast.ForecastDays.ToString(System.Globalization.CultureInfo.InvariantCulture);
        parameters["past_days"] = forecast.PastDays.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Добавляет параметры ориентации солнечной панели для расчёта излучения.</summary>
    /// <param name="parameters">Словарь параметров запроса API.</param>
    private void AddSolarPanelParameters(IDictionary<string, string> parameters)
    {
        parameters["tilt"] = options.Tilt.ToString(System.Globalization.CultureInfo.InvariantCulture);
        parameters["azimuth"] = options.Azimuth.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Выполняет HTTP-запрос и преобразует полученный JSON-ответ в DTO.</summary>
    /// <param name="parameters">Параметры запроса Open-Meteo.</param>
    /// <param name="cancellationToken">Токен отмены запроса и чтения ответа.</param>
    /// <returns>Преобразованный ответ API.</returns>
    private async Task<OpenMeteoResponse> FetchWeatherAsync(
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        var query = string.Join("&", parameters.Select(parameter =>
            $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));

        using var response = await httpClient.GetAsync($"?{query}", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        return OpenMeteoResponseMapper.Map(document.RootElement);
    }
}
