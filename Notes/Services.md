# Demo System

> Work in Progress
 
The demo system uses an EF In-Memory database and a `TestDataProvider` to provide test data to load into the database and test the data pipeline against.  You can review the code in the Repo at *Blazr.App.Infrastructure/DataSources*.

`TestDataProvider` is a singleton class that generates a test data set when first created.

All the application infrastructure services are defined in the `ApplicationInfrastructureServices` extension class.

`AddAppServerInfrastructureServices` adds all the core and infrastructure services.  It:

1. Adds the DbContextFactory.
2. Adds the `ServerDataBroker` implementation of `IDataBroker`.
3. Adds the server implementation of the query and command handlers.
4. Calls the extension methods for the individual entities. 

```csharp
public static void AddAppServerInfrastructureServices(this IServiceCollection services)
{
    services.AddDbContextFactory<InMemoryTestDbContext>(options
        => options.UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid().ToString()}"));

    services.AddScoped<IDataBroker, DataBroker>();
    services.AddScoped<IIdConverter, IdConverter>();

    // Add the standard handlers
    services.AddScoped<IListRequestHandler, ListRequestServerHandler<InMemoryTestDbContext>>();
    services.AddScoped<IItemRequestHandler, ItemRequestServerHandler<InMemoryTestDbContext>>();
    services.AddScoped<ICommandHandler, CommandServerHandler<InMemoryTestDbContext>>();

    // Add any individual entity services
    services.AddMappedWeatherForecastServerInfrastructureServices();
}
```

`AddWeatherForecastServerInfrastructureServices` looks like this.  It adds:
1. The domain to infrastructure object mapper.
2. Custom Handlers.
3. Filters and Sorters

```csharp
    public static void AddMappedWeatherForecastServerInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IDboEntityMap<DboWeatherForecast, DcoWeatherForecast>, WeatherForecastMap>();
        services.AddScoped<IListRequestHandler<DcoWeatherForecast>, MappedListRequestServerHandler<InMemoryTestDbContext, DcoWeatherForecast, DboWeatherForecast>>();
        services.AddScoped<IItemRequestHandler<DcoWeatherForecast, WeatherForecastId>, MappedItemRequestServerHandler<InMemoryTestDbContext, DcoWeatherForecast, DboWeatherForecast, WeatherForecastId>>();
        services.AddScoped<ICommandHandler<DcoWeatherForecast>, MappedCommandServerHandler<InMemoryTestDbContext, DcoWeatherForecast, DboWeatherForecast>>();

        services.AddTransient<IRecordFilterHandler<DboWeatherForecast>, DboWeatherForecastFilterHandler>();
        services.AddTransient<IRecordSortHandler<DboWeatherForecast>, DboWeatherForecastSortHandler>();
    }
```
