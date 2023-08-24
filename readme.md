# One Way Street

One Way Street is a read only data pipeline based on CQS [Command/Query Separation].

There are two *pipes* within the pipeline:
1. The Query Pipeline which fetches either a single record or a collection of records from the record store.
2. The Command Pipeline which takes a record and applies a change to the data store.

A data broker provides the gateway in to the pipeline.  The `IDataBroker` interface looks like this:

```
public interface IDataBroker
{
    public ValueTask<ListQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ListQueryRequest request) where TRecord : class;
    public ValueTask<ItemQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ItemQueryRequest request) where TRecord : class, IEntity;
    public ValueTask<CommandResult> ExecuteCommandAsync<TRecord>(CommandRequest<TRecord> request) where TRecord : class, IEntity;
}
```

Each method takes a *Request* and returns a *Result*.

To provide an overview of the pipleine I'll walk the reader through the configuration and some XUnit.

The demo system uses an EF In-Memory database and a `TestDataProvider` to provide test data to load into the database and test the data pipeline against.  You can review the cooide in the Repo at *Blazr.App.Infrastructure/DataSources*.

All the application infrastructure services are defined in the `ApplicationInfrastructureServices` extension class.

`AddAppServerInfrastructureServices` adds all the core and infrastructure services.

It:

1. Adds the DbContextFactory.
2. Adds the `ServerDataBroker` implementation of `IDataBroker`.
3. Adds the server implementation of the query and command handlers.
4. Calls the extension methods for the individual entities. 

```csharp
public static void AddAppServerInfrastructureServices(this IServiceCollection services)
{
    services.AddDbContextFactory<InMemoryTestDbContext>(options
        => options.UseInMemoryDatabase($"TestDatabase-{Guid.NewGuid().ToString()}"));

    services.AddScoped<IDataBroker, ServerDataBroker>();

    // Add the standard handlers
    services.AddScoped<IListRequestHandler, ListRequestServerHandler<InMemoryTestDbContext>>();
    services.AddScoped<IItemRequestHandler, ItemRequestServerHandler<InMemoryTestDbContext>>();
    services.AddScoped<ICommandHandler, CommandServerHandler<InMemoryTestDbContext>>();

    // Add any individual entity services
    services.AddWeatherForecastServerInfrastructureServices();
}
```

`AddWeatherForecastServerInfrastructureServices` looks like this.

It adds:
1. The domain to infrastructure object mapper.
2. A custom `ICommandHandler`.

```csharp
    public static void AddWeatherForecastServerInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IDboEntityMap<DboWeatherForecast, WeatherForecast>, DboWeatherForecastMap>();
        services.AddScoped<ICommandHandler<WeatherForecast>, WeatherForecastCommandHandler<InMemoryTestDbContext>>();
    }
```

### Domain/Infrastructure Objects and Mapping

Our Domain `WeatherForcast` looks like this.  Note that the identifier is a `WeatherForecastUid` value object.  Specifically it's not a `Guid` or an `int`: no *Primitive Obsession*.

```csharp
public readonly record struct WeatherForecastUid(Guid Value);

public sealed record WeatherForecast : IEntity, ICommandEntity
{
    public WeatherForecastUid WeatherForecastUid { get; init; } = new WeatherForecastUid(Guid.NewGuid());
    public DateOnly Date { get; init; } 
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }

    public EntityUid EntityUid => new(WeatherForecastUid.Value);
}
```

However, our data store is based on prmitives so our Dbo object looks like this:

```csharp
public sealed record DboWeatherForecast : ICommandEntity
{
    [Key] public Guid Uid { get; init; } = Guid.Empty;
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }
}
```

And the mapper provides the translations.

```csharp
public class DboWeatherForecastMap : IDboEntityMap<DboWeatherForecast, WeatherForecast>
{
    public WeatherForecast MapTo(DboWeatherForecast item)
        => Map(item);

    public DboWeatherForecast MapTo(WeatherForecast item)
        => Map(item);

    public static WeatherForecast Map(DboWeatherForecast item)
        => new()
        {
            WeatherForecastUid = new(item.Uid),
            Summary = item.Summary,
            TemperatureC = item.TemperatureC,
            Date = item.Date,
        };

    public static DboWeatherForecast Map(WeatherForecast item)
        => new()
        {
            Uid = item.WeatherForecastUid.Value,
            Summary = item.Summary,
            TemperatureC = item.TemperatureC,
            Date = item.Date,
        };
}
```

There's a discussion in the *Notes* on why this is a good practice.

### Mocking the DI Container

```csharp
public class WeatherForecastTests
{
    private TestDataProvider _testDataProvider;

    public WeatherForecastTests()
        => _testDataProvider = TestDataProvider.Instance();

    private ServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddAppTestInfrastructureServices();
        services.AddLogging(builder => builder.AddDebug());

        var provider = services.BuildServiceProvider();

        // get the DbContext factory and add the test data
        var factory = provider.GetService<IDbContextFactory<InMemoryTestDbContext>>();
        if (factory is not null)
            TestDataProvider.Instance().LoadDbContext<InMemoryTestDbContext>(factory);

        return provider!;
    }

//...
}
```


