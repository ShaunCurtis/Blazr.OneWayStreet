/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public readonly record struct WeatherForecastId : IEntityKey
{
    public Guid Value { get; init; }
    public object KeyValue => this.Value;

    public WeatherForecastId(Guid value)
        => this.Value = value;

    public static WeatherForecastId NewEntity
        => new(Guid.Empty);
}

public sealed record DcoWeatherForecast : ICommandEntity
{
    public WeatherForecastId WeatherForecastId { get; init; } = WeatherForecastId.NewEntity;

    public DateOnly Date { get; init; } 

    public Temperature Temperature { get; init; }

    public string? Summary { get; init; }
}
