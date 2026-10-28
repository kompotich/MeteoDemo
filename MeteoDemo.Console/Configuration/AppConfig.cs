using MeteoDemo.Core.Configuration;

namespace MeteoDemo.Console.Configuration;

internal sealed class AppConfig
{
    public required OpenMeteoOptions OpenMeteo { get; init; }
}
