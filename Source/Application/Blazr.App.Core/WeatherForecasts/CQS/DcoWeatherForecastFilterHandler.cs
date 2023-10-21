/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Core;

public class DcoWeatherForecastFilterHandler : RecordFilterHandler<DcoWeatherForecast>, IRecordFilterHandler<DcoWeatherForecast>
{
    public override IPredicateSpecification<DcoWeatherForecast>? GetSpecification(FilterDefinition filter)
        => filter.FilterName switch
        {
            ApplicationConstants.WeatherForecast.FilterWeatherForecastsBySummary => new DcoWeatherForecastsBySummarySpecification(filter),
            _ => null
        };
}
