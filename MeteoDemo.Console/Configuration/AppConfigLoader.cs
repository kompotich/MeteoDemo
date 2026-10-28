using System.Text.Json;

namespace MeteoDemo.Console.Configuration;

internal static class AppConfigLoader
{
    public static async Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        var configJson = await File.ReadAllTextAsync(configPath, cancellationToken);

        return JsonSerializer.Deserialize<AppConfig>(
                   configJson,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidOperationException("Could not read application configuration.");
    }
}
