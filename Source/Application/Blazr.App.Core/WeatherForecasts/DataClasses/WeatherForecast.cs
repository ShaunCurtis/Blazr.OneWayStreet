/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using System.ComponentModel.DataAnnotations;

namespace Blazr.App.Core;

public sealed record WeatherForecast : ICommandEntity
{
    [Key] public Guid WeatherForecastUid { get; init; }

    public DateOnly Date { get; init; } 

    public int TemperatureC { get; init; }

    public string? Summary { get; init; }
}
