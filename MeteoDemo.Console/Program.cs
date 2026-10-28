using MeteoDemo.Console;
using MeteoDemo.Console.Configuration;
using MeteoDemo.Core;
using MeteoDemo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var config = await AppConfigLoader.LoadAsync();
var services = new ServiceCollection();

services.AddSingleton(config.OpenMeteo);
services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlite(config.Database.ConnectionString));
services.AddScoped<WeatherDataStore>();
services.AddScoped<WeatherDatabaseInitializer>();
services.AddHttpClient<OpenMeteoClient>(client =>
    client.BaseAddress = new Uri(config.OpenMeteo.BaseUrl));
services.AddScoped<WeatherConsole>();

await using var serviceProvider = services.BuildServiceProvider();
await using var scope = serviceProvider.CreateAsyncScope();
await scope.ServiceProvider.GetRequiredService<WeatherDatabaseInitializer>().InitializeAsync();
await scope.ServiceProvider.GetRequiredService<WeatherConsole>().DisplayWeatherAsync();
