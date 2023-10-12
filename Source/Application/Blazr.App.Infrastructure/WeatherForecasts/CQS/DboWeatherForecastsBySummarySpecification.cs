/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public class DboWeatherForecastsBySummarySpecification : PredicateSpecification<DboWeatherForecast>
{
    private string _summary;

    public DboWeatherForecastsBySummarySpecification(string summary)
    {
        _summary = summary;
    }

    public DboWeatherForecastsBySummarySpecification(FilterDefinition filter)
    {
        _summary = filter.FilterData.ToString();
    }

    public override Expression<Func<DboWeatherForecast, bool>> Expression
        => item => _summary.Equals(item.Summary, StringComparison.CurrentCultureIgnoreCase);
}
