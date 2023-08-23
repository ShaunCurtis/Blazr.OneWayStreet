/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.App.Infrastructure;

public static class ApplicationInfrastructureServices
{
    public static void AddAppServerInfrastructureServices(this IServiceCollection services)
        => services.AddAppServerDataServices<InMemoryTestDbContext>(options
            => options.UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid().ToString()}"));

    public static void AddAppTestInfrastructureServices(this IServiceCollection services)
        => services.AddAppServerDataServices<InMemoryTestDbContext>(options
            => options.UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid().ToString()}"));

    public static void AddAppServerDataServices<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> options) where TDbContext : DbContext
    {
        AddAppServerInfrastructureServices<TDbContext>(services, options);
    }

    private static void AddAppServerInfrastructureServices<TDbContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> options) where TDbContext : DbContext
    {
        services.AddDbContextFactory<TDbContext>(options);
        services.AddScoped<IDataBroker, ServerDataBroker>();

        // Add the standard handlers
        services.AddScoped<IListRequestHandler, ListRequestServerHandler<InMemoryTestDbContext>>();
        services.AddScoped<IItemRequestHandler, ItemRequestServerHandler<InMemoryTestDbContext>>();
        services.AddScoped<ICommandHandler, CommandServerHandler<InMemoryTestDbContext>>();


        // Add custom handlers
        services.AddScoped<ICommandHandler<WeatherForecast>, WeatherForecastCommandHandler<InMemoryTestDbContext>>();

        services.AddWeatherForecastServerInfrastructureServices();
    }

    public static void AddTestData(IServiceProvider provider)
    {
        var factory = provider.GetService<IDbContextFactory<InMemoryTestDbContext>>();

        if (factory is not null)
            TestDataProvider.Instance().LoadDbContext<InMemoryTestDbContext>(factory);
    }
}
