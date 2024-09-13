/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class DcoWeatherForecastEditContext
{
    public string Name => "WeatherForecast Edit Context";

    public WeatherForecastId WeatherForecastId { get; private set; }

    public string? Summary { get; set; }

    public decimal? TemperatureC { get; set; }

    public DateOnly? Date { get; set; }

    public Guid Uid => WeatherForecastId.Value;

    public DcoWeatherForecastEditContext(DcoWeatherForecast record)
    {
        this.WeatherForecastId = record.WeatherForecastId;
        this.Summary = record.Summary;
        this.TemperatureC = record.Temperature.TemperatureC;
        this.Date = record.Date;
    }

    public DcoWeatherForecast AsRecord => new()
    {
        WeatherForecastId = WeatherForecastId,
        Summary = Summary ?? "Not Set",
        Date = this.Date ?? DateOnly.FromDateTime(DateTime.Now),
        Temperature = new(this.TemperatureC ?? 0)
    };
}
