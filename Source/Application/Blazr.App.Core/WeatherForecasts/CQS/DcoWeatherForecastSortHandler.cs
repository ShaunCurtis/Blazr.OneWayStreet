/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class DcoWeatherForecastSortHandler : RecordSortHandler<DcoWeatherForecast>, IRecordSortHandler<DcoWeatherForecast>
{
    public DcoWeatherForecastSortHandler()
    {
        DefaultSorter = (item) => item.Date;
        DefaultSortDescending = false;
    }
}
