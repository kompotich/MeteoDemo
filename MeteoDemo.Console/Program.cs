using MeteoDemo.Console;
using MeteoDemo.Console.Configuration;
using MeteoDemo.Core;
using Microsoft.Extensions.DependencyInjection;

var config = await AppConfigLoader.LoadAsync();
var services = new ServiceCollection();

services.AddSingleton(config.OpenMeteo);
services.AddHttpClient<OpenMeteoClient>(client =>
    client.BaseAddress = new Uri(config.OpenMeteo.BaseUrl));
services.AddTransient<WeatherConsole>();

await using var serviceProvider = services.BuildServiceProvider();
await serviceProvider.GetRequiredService<WeatherConsole>().DisplayWeatherAsync();
