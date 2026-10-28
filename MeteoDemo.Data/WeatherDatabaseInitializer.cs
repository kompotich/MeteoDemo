using Microsoft.EntityFrameworkCore;

namespace MeteoDemo.Data;

/// <summary>Создаёт схему SQLite при запуске приложения, если база ещё не создана.</summary>
/// <param name="dbContext">Контекст базы данных приложения.</param>
public sealed class WeatherDatabaseInitializer(WeatherDbContext dbContext)
{
    /// <summary>Инициализирует базу данных на основе текущей модели EF Core.</summary>
    /// <param name="cancellationToken">Токен отмены операции создания базы.</param>
    /// <returns>Асинхронная операция инициализации.</returns>
    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        dbContext.Database.EnsureCreatedAsync(cancellationToken);
}
