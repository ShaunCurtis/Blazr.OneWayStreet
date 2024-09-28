#  Domain/Infrastructure Objects and Mapping

Our Domain `WeatherForcast` looks like this.  Note that the identifier is a `WeatherForecastId` value object.  Specifically it's not a `Guid` or an `int`: no *Primitive Obsession*.

```csharp
public readonly record struct WeatherForecastId(Guid Value);

public sealed record WeatherForecast : IEntity, ICommandEntity
{
    public WeatherForecastUid WeatherForecastUid { get; init; } = new WeatherForecastUid(Guid.NewGuid());
    public DateOnly Date { get; init; } 
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }

    public EntityUid EntityUid => new(WeatherForecastUid.Value);
}
```

However, our data store is based on primitives so our Dbo object looks like this:

```csharp
public sealed record DboWeatherForecast : ICommandEntity
{
    [Key] public Guid Uid { get; init; } = Guid.Empty;
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
}
```

And the mapper provides the translations.

```csharp
public class DboWeatherForecastMap : IDboEntityMap<DboWeatherForecast, WeatherForecast>
{
    public WeatherForecast MapTo(DboWeatherForecast item)
        => Map(item);

    public DboWeatherForecast MapTo(WeatherForecast item)
        => Map(item);

    public static WeatherForecast Map(DboWeatherForecast item)
        => new()
        {
            WeatherForecastUid = new(item.Uid),
            Summary = item.Summary,
            TemperatureC = item.TemperatureC,
            Date = item.Date,
        };

    public static DboWeatherForecast Map(WeatherForecast item)
        => new()
        {
            Uid = item.WeatherForecastUid.Value,
            Summary = item.Summary,
            TemperatureC = item.TemperatureC,
            Date = item.Date,
        };
}
```

There's a discussion in the *Notes* on why this is good practice.

