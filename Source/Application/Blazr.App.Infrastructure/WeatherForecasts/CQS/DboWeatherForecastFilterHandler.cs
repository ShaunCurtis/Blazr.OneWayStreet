/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

using Blazr.Core.OWS;

public class DboWeatherForecastFilterHandler : RecordFilterHandler<DboWeatherForecast>, IRecordFilterHandler<DboWeatherForecast>
{
    public override IPredicateSpecification<DboWeatherForecast>? GetSpecification(FilterDefinition filter)
        => filter.FilterName switch
        {
            ApplicationConstants.WeatherForecast.FilterWeatherForecastsBySummary => new DboWeatherForecastsBySummarySpecification(filter),
            _ => null
        };
}
