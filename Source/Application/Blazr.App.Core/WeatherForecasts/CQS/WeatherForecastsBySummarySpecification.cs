/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecastsBySummarySpecification : PredicateSpecification<WeatherForecast>
{
    private string _summary;

    public WeatherForecastsBySummarySpecification(string summary)
    {
        _summary = summary;
    }

    public WeatherForecastsBySummarySpecification(FilterDefinition filter)
    {
        _summary = filter.FilterData.ToString();
    }

    public override Expression<Func<WeatherForecast, bool>> Expression
        => item => _summary.Equals(item.Summary, StringComparison.CurrentCultureIgnoreCase);
}
