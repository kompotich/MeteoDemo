using MeteoDemo.Console;
using MeteoDemo.Console.Configuration;
using MeteoDemo.Core;
using Microsoft.Extensions.DependencyInjection;

var config = await AppConfigLoader.LoadAsync();
var services = new ServiceCollection();

services.AddSingleton(config.OpenMeteo);
services.AddHttpClient<OpenMeteoClient>(client =>
    client.BaseAddress = new Uri(config.OpenMeteo.BaseUrl));
services.AddScoped<WeatherConsole>();

await using var serviceProvider = services.BuildServiceProvider();
await using var scope = serviceProvider.CreateAsyncScope();

var openMeteoClient = scope.ServiceProvider.GetRequiredService<OpenMeteoClient>();
var weatherConsole = scope.ServiceProvider.GetRequiredService<WeatherConsole>();

var currentWeather = await openMeteoClient.GetCurrentWeatherAsync();
var hourlyWeather = await openMeteoClient.GetHourlyWeatherAsync();
var dailyWeather = await openMeteoClient.GetDailyWeatherAsync();

weatherConsole.DisplayCurrentWeather(currentWeather);
weatherConsole.DisplayHourlyWeather(hourlyWeather);
weatherConsole.DisplayDailyWeather(dailyWeather);
