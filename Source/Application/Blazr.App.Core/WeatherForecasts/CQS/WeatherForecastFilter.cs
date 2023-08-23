/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecastFilter : IRecordFilter<WeatherForecast>
{
    public IPredicateSpecification<WeatherForecast>? GetSpecification(FilterDefinition filter)
        => filter.FilterName switch
        {
            ApplicationConstants.WeatherForecast.FilterWeatherForecastsBySummary => new WeatherForecastsBySummarySpecification(filter),
            _ => null
        };
}
