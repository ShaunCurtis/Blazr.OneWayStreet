/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public sealed record DboWeatherForecast : ICommandEntity, IKeyedEntity
{
    [Key] public Guid Uid { get; init; } = Guid.Empty;

    public DateOnly Date { get; init; }

    public decimal TemperatureC { get; init; }

    public string? Summary { get; init; }

    public object KeyValue => Uid;
}
