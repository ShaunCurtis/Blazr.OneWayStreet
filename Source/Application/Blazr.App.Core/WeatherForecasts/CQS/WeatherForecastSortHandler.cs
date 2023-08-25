/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.App.Core;

public class WeatherForecastSortHandler : RecordSortHandler<WeatherForecast>, IRecordSortHandler<WeatherForecast>
{
    public WeatherForecastSortHandler()
    {
        DefaultSorter = (item) => item.Date;
        DefaultSortDescending = false;
    }
}
