/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public static class WeatherForecastInfrastructureServices
{
    public static void AddWeatherForecastServerInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IDboEntityMap<DboWeatherForecast, WeatherForecast>, WeatherForecastMap>();
        services.AddScoped<IListRequestHandler, MappedListRequestServerHandler<InMemoryTestDbContext, DboWeatherForecast>>();
        services.AddScoped<IItemRequestHandler, MappedItemRequestServerHandler<InMemoryTestDbContext, DboWeatherForecast>>();
        services.AddScoped<ICommandHandler, MappedCommandServerHandler<InMemoryTestDbContext, DboWeatherForecast>>();

        services.AddTransient<IRecordFilterHandler<WeatherForecast>, WeatherForecastFilterHandler>();
        services.AddTransient<IRecordSortHandler<WeatherForecast>, WeatherForecastSortHandler>();

        services.AddTransient<IRecordFilterHandler<DboWeatherForecast>, DboWeatherForecastFilterHandler>();
        services.AddTransient<IRecordSortHandler<DboWeatherForecast>, DboWeatherForecastSortHandler>();

    }
}
