using MeteoDemo.Core.Configuration;

namespace MeteoDemo.Console.Configuration;

/// <summary>Корневая конфигурация консольного приложения.</summary>
internal sealed class AppConfig
{
    /// <summary>Параметры получения погоды от Open-Meteo.</summary>
    public required OpenMeteoOptions OpenMeteo { get; init; }

    /// <summary>Параметры подключения к локальной базе данных.</summary>
    public required DatabaseOptions Database { get; init; }
}

/// <summary>Параметры подключения к хранилищу данных.</summary>
internal sealed class DatabaseOptions
{
    /// <summary>Строка подключения SQLite, используемая Entity Framework Core.</summary>
    public required string ConnectionString { get; init; }
}
