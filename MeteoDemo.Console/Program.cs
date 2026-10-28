using MeteoDemo.Console;
using MeteoDemo.Console.Configuration;
using MeteoDemo.Core;
using MeteoDemo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var config = await AppConfigLoader.LoadAsync();
var services = new ServiceCollection();

services.AddSingleton(config.OpenMeteo);
services.AddHttpClient<OpenMeteoClient>(client =>
    client.BaseAddress = new Uri(config.OpenMeteo.BaseUrl));
services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlite("Data Source=weather.db"));
services.AddScoped<WeatherDatabaseInitializer>();
services.AddScoped<WeatherDataStore>();
services.AddScoped<WeatherConsole>();
services.AddSingleton(config.Schedules);
services.AddSingleton<WeatherScheduleRunner>();

await using var serviceProvider = services.BuildServiceProvider();
await using (var initializationScope = serviceProvider.CreateAsyncScope())
{
    var databaseInitializer = initializationScope.ServiceProvider
        .GetRequiredService<WeatherDatabaseInitializer>();
    await databaseInitializer.InitializeAsync();
}

using var cancellationTokenSource = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};
System.Console.CancelKeyPress += cancelHandler;

var scheduleRunner = serviceProvider.GetRequiredService<WeatherScheduleRunner>();
try
{
    await scheduleRunner.RunAsync(cancellationTokenSource.Token);
}
catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
{
}
finally
{
    System.Console.CancelKeyPress -= cancelHandler;
}
