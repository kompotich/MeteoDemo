using System.Linq.Expressions;
using MeteoDemo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeteoDemo.Data;

/// <summary>Контекст Entity Framework Core для независимых снимков current, hourly и daily.</summary>
/// <param name="options">Настройки провайдера и подключения к базе данных.</param>
public sealed class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
{
    /// <summary>Отдельные ответы Open-Meteo с текущими погодными показателями.</summary>
    public DbSet<CurrentWeatherSnapshot> CurrentWeatherSnapshots => Set<CurrentWeatherSnapshot>();

    /// <summary>Отдельные ответы Open-Meteo с почасовым прогнозом.</summary>
    public DbSet<HourlyWeatherSnapshot> HourlyWeatherSnapshots => Set<HourlyWeatherSnapshot>();

    /// <summary>Отдельные ответы Open-Meteo с суточной сводкой.</summary>
    public DbSet<DailyWeatherSnapshot> DailyWeatherSnapshots => Set<DailyWeatherSnapshot>();

    /// <summary>Настраивает таблицы снимков, измерений и их связи.</summary>
    /// <param name="modelBuilder">Построитель модели EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CurrentWeatherSnapshot>(entity =>
        {
            entity.ToTable("CurrentWeatherSnapshots");
            entity.HasKey(snapshot => snapshot.Id);
            ConfigureMetadata(entity, snapshot => snapshot.Metadata);
            entity.HasMany(snapshot => snapshot.Measurements)
                .WithOne(measurement => measurement.Snapshot)
                .HasForeignKey(measurement => measurement.CurrentWeatherSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CurrentWeatherMeasurement>(entity =>
        {
            entity.ToTable("CurrentWeatherMeasurements");
            entity.HasKey(measurement => measurement.Id);
            ConfigureMeasurement(entity);
            entity.HasIndex(measurement => new
            {
                measurement.CurrentWeatherSnapshotId,
                measurement.Timestamp,
                measurement.VariableName
            }).IsUnique();
        });

        modelBuilder.Entity<HourlyWeatherSnapshot>(entity =>
        {
            entity.ToTable("HourlyWeatherSnapshots");
            entity.HasKey(snapshot => snapshot.Id);
            ConfigureMetadata(entity, snapshot => snapshot.Metadata);
            entity.HasMany(snapshot => snapshot.Measurements)
                .WithOne(measurement => measurement.Snapshot)
                .HasForeignKey(measurement => measurement.HourlyWeatherSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HourlyWeatherMeasurement>(entity =>
        {
            entity.ToTable("HourlyWeatherMeasurements");
            entity.HasKey(measurement => measurement.Id);
            ConfigureMeasurement(entity);
            entity.HasIndex(measurement => new
            {
                measurement.HourlyWeatherSnapshotId,
                measurement.Timestamp,
                measurement.VariableName
            }).IsUnique();
        });

        modelBuilder.Entity<DailyWeatherSnapshot>(entity =>
        {
            entity.ToTable("DailyWeatherSnapshots");
            entity.HasKey(snapshot => snapshot.Id);
            ConfigureMetadata(entity, snapshot => snapshot.Metadata);
            entity.HasMany(snapshot => snapshot.Measurements)
                .WithOne(measurement => measurement.Snapshot)
                .HasForeignKey(measurement => measurement.DailyWeatherSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DailyWeatherMeasurement>(entity =>
        {
            entity.ToTable("DailyWeatherMeasurements");
            entity.HasKey(measurement => measurement.Id);
            ConfigureMeasurement(entity);
            entity.Property(measurement => measurement.TextValue).HasMaxLength(100);
            entity.HasIndex(measurement => new
            {
                measurement.DailyWeatherSnapshotId,
                measurement.Timestamp,
                measurement.VariableName
            }).IsUnique();
        });
    }

    /// <summary>Настраивает общие метаданные ответа как встроенные в таблицу снимка поля.</summary>
    /// <typeparam name="TSnapshot">Тип снимка прогноза.</typeparam>
    /// <param name="entity">Настройка сущности снимка.</param>
    private static void ConfigureMetadata<TSnapshot>(
        EntityTypeBuilder<TSnapshot> entity,
        Expression<Func<TSnapshot, WeatherSnapshotMetadata>> metadataNavigation)
        where TSnapshot : class
    {
        entity.OwnsOne(metadataNavigation, metadata =>
        {
            metadata.Property(value => value.Timezone).HasMaxLength(100).IsRequired();
            metadata.Property(value => value.TimezoneAbbreviation).HasMaxLength(20).IsRequired();
        });
        entity.Navigation(metadataNavigation).IsRequired();
    }

    /// <summary>Настраивает общие ограничения для погодного измерения.</summary>
    /// <typeparam name="TMeasurement">Тип измерения прогноза.</typeparam>
    /// <param name="entity">Настройка сущности измерения.</param>
    private static void ConfigureMeasurement<TMeasurement>(EntityTypeBuilder<TMeasurement> entity)
        where TMeasurement : class
    {
        entity.Property("VariableName").HasMaxLength(100).IsRequired();
        entity.Property("Timestamp").HasMaxLength(40).IsRequired();
        entity.Property("Unit").HasMaxLength(30);
    }
}
