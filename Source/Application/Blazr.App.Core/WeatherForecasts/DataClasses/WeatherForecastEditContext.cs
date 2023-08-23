/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecastEditContext 
{
    public string Name => "WeatherForecast Edit Context";

    public WeatherForecastUid WeatherForecastUid { get; private set; }

    public string? Summary { get; set; }

    public int? TemperatureC { get; set; }

    public DateOnly? Date { get; set; }

    public Guid Uid => WeatherForecastUid.Value;

    public WeatherForecastEditContext(WeatherForecast record)
    {
        this.WeatherForecastUid = record.WeatherForecastUid;
        this.Summary = record.Summary;
        this.TemperatureC = record.TemperatureC;
        this.Date = record.Date;
    }
}
