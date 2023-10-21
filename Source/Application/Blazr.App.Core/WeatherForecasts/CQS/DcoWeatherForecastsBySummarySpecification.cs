/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class DcoWeatherForecastsBySummarySpecification : PredicateSpecification<DcoWeatherForecast>
{
    private string _summary;

    public DcoWeatherForecastsBySummarySpecification(string summary)
    {
        _summary = summary;
    }

    public DcoWeatherForecastsBySummarySpecification(FilterDefinition filter)
    {
        _summary = filter.FilterData.ToString();
    }

    public override Expression<Func<DcoWeatherForecast, bool>> Expression
        => item => _summary.Equals(item.Summary, StringComparison.CurrentCultureIgnoreCase);
}
