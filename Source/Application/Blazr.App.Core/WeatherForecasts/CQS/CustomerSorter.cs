/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class CustomerSorter : RecordSorter<WeatherForecast>, IRecordSorter<WeatherForecast>
{
    protected override Expression<Func<WeatherForecast, object>> DefaultSorter { get; } = (item) => item.Date;
    protected override bool DefaultSortDescending { get; } = true;
}
