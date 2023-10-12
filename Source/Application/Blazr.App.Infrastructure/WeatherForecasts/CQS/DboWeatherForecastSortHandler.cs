/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Infrastructure;

public class DboWeatherForecastSortHandler : RecordSortHandler<DboWeatherForecast>, IRecordSortHandler<DboWeatherForecast>
{
    public DboWeatherForecastSortHandler()
    {
        DefaultSorter = (item) => item.Date;
        DefaultSortDescending = false;
    }
}
