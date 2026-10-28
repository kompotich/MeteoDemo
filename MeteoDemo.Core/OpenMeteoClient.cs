using System.Text.Json;
using MeteoDemo.Core.Configuration;
using MeteoDemo.Core.Mapping;
using MeteoDemo.Core.Models;

namespace MeteoDemo.Core;

public sealed class OpenMeteoClient(HttpClient httpClient, OpenMeteoOptions options)
{
    public async Task<OpenMeteoResponse> GetWeatherAsync(CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["latitude"] = options.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["longitude"] = options.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["current"] = string.Join(',', options.Current),
            ["hourly"] = string.Join(',', options.Hourly),
            ["daily"] = string.Join(',', options.Daily),
            ["forecast_days"] = options.ForecastDays.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["past_days"] = options.PastDays.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["timezone"] = options.Timezone,
            ["temperature_unit"] = options.TemperatureUnit,
            ["wind_speed_unit"] = options.WindSpeedUnit,
            ["precipitation_unit"] = options.PrecipitationUnit,
            ["tilt"] = options.Tilt.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["azimuth"] = options.Azimuth.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        var query = string.Join("&", parameters.Select(parameter =>
            $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));

        using var response = await httpClient.GetAsync($"?{query}", cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        return OpenMeteoResponseMapper.Map(document.RootElement);
    }
}
