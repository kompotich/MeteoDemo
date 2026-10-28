using System.Text.Json;

namespace MeteoDemo.Console.Configuration;

internal static class AppConfigLoader
{
    /// <summary>Загружает и десериализует config.json из выходного каталога приложения.</summary>
    /// <param name="cancellationToken">Токен отмены чтения файла.</param>
    /// <returns>Настройки приложения.</returns>
    /// <exception cref="IOException">Не удалось прочитать файл конфигурации.</exception>
    /// <exception cref="System.Text.Json.JsonException">Содержимое файла не соответствует модели конфигурации.</exception>
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
