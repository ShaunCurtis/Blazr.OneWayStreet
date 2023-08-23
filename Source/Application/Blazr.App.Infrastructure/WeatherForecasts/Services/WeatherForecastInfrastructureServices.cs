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
        services.AddScoped<IDboEntityMap<DboWeatherForecast, WeatherForecast>, DboWeatherForecastMap>();
        services.AddScoped<ICommandHandler<WeatherForecast>, WeatherForecastCommandHandler<InMemoryTestDbContext>>();
    }
}
