/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public readonly record struct WeatherForecastUid(Guid Value);

public sealed record DcoWeatherForecast : ICommandEntity
{
    public WeatherForecastUid WeatherForecastUid { get; init; } = new WeatherForecastUid(Guid.NewGuid());

    public DateOnly Date { get; init; } 

    public Temperature Temperature { get; init; }

    public string? Summary { get; init; }
}
