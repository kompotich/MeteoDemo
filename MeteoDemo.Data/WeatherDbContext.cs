using MeteoDemo.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeteoDemo.Data;

/// <summary>Контекст Entity Framework Core для снимков прогноза и отдельных измерений погоды.</summary>
/// <param name="options">Настройки провайдера и подключения к базе данных.</param>
public sealed class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
{
    /// <summary>Сохранённые ответы Open-Meteo.</summary>
    public DbSet<WeatherSnapshot> WeatherSnapshots => Set<WeatherSnapshot>();

    /// <summary>Текущие, почасовые и суточные показатели сохранённых ответов.</summary>
    public DbSet<WeatherMeasurement> WeatherMeasurements => Set<WeatherMeasurement>();

    /// <summary>Настраивает первичные ключи, ограничения и связи сущностей SQLite.</summary>
    /// <param name="modelBuilder">Построитель модели EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherSnapshot>(entity =>
        {
            entity.HasKey(snapshot => snapshot.Id);
            entity.Property(snapshot => snapshot.Timezone).HasMaxLength(100).IsRequired();
            entity.Property(snapshot => snapshot.TimezoneAbbreviation).HasMaxLength(20).IsRequired();
            entity.HasIndex(snapshot => snapshot.FetchedAtUtc);

            entity.HasMany(snapshot => snapshot.Measurements)
                .WithOne(measurement => measurement.WeatherSnapshot)
                .HasForeignKey(measurement => measurement.WeatherSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeatherMeasurement>(entity =>
        {
            entity.HasKey(measurement => measurement.Id);
            entity.Property(measurement => measurement.Section).HasMaxLength(20).IsRequired();
            entity.Property(measurement => measurement.VariableName).HasMaxLength(100).IsRequired();
            entity.Property(measurement => measurement.Timestamp).HasMaxLength(40).IsRequired();
            entity.Property(measurement => measurement.Unit).HasMaxLength(30);
            entity.Property(measurement => measurement.TextValue).HasMaxLength(100);
            entity.HasIndex(measurement => new
            {
                measurement.WeatherSnapshotId,
                measurement.Section,
                measurement.Timestamp,
                measurement.VariableName
            }).IsUnique();
        });
    }
}
