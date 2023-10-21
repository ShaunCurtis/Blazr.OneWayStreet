/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public class WeatherForecastMap : IDboEntityMap<DboWeatherForecast, DcoWeatherForecast>
{
    public DcoWeatherForecast MapTo(DboWeatherForecast item)
        => Map(item);

    public DboWeatherForecast MapTo(DcoWeatherForecast item)
        => Map(item);

    public static DcoWeatherForecast Map(DboWeatherForecast item)
        => new()
        {
            WeatherForecastUid = new(item.Uid),
            Summary = item.Summary,
            TemperatureC = item.TemperatureC,
            Date = item.Date,
        };

    public static DboWeatherForecast Map(DcoWeatherForecast item)
        => new()
        {
            Uid = item.WeatherForecastUid.Value,
            Summary = item.Summary,
            TemperatureC = item.TemperatureC,
            Date = item.Date,
        };
}
