﻿/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public static class WeatherForecastInfrastructureServices
{
    public static void AddWeatherForecastServerInfrastructureServices(this IServiceCollection services)
    {
        services.AddTransient<IRecordFilterHandler<WeatherForecast>, WeatherForecastFilterHandler>();
        services.AddTransient<IRecordSortHandler<WeatherForecast>, WeatherForecastSortHandler>();
    }

    public static void AddMappedWeatherForecastServerInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IDboEntityMap<DboWeatherForecast, DcoWeatherForecast>, WeatherForecastMap>();
        services.AddScoped<IListRequestHandler<DcoWeatherForecast>, MappedListRequestServerHandler<InMemoryTestDbContext, DcoWeatherForecast, DboWeatherForecast>>();
        services.AddScoped<IItemRequestHandler<DcoWeatherForecast>, MappedItemRequestServerHandler<InMemoryTestDbContext, DcoWeatherForecast, DboWeatherForecast>>();
        services.AddScoped<ICommandHandler<DcoWeatherForecast>, MappedCommandServerHandler<InMemoryTestDbContext, DcoWeatherForecast, DboWeatherForecast>>();

        //services.AddTransient<IRecordFilterHandler<DcoWeatherForecast>, DcoWeatherForecastFilterHandler>();
        //services.AddTransient<IRecordSortHandler<DcoWeatherForecast>, DcoWeatherForecastSortHandler>();

        services.AddTransient<IRecordFilterHandler<DboWeatherForecast>, DboWeatherForecastFilterHandler>();
        services.AddTransient<IRecordSortHandler<DboWeatherForecast>, DboWeatherForecastSortHandler>();
    }
}
