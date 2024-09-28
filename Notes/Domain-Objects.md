#  Domain/Infrastructure Objects and Mapping

Our Domain `WeatherForcast` looks like this.  Note that the identifier is a `WeatherForecastId` value object.  Specifically it's not a `Guid` or an `int`: no *Primitive Obsession*.

```csharp
public readonly record struct WeatherForecastId(Guid Value);

public sealed record WeatherForecast : IEntity, ICommandEntity
{
    public WeatherForecastUid WeatherForecastUid { get; init; } = new WeatherForecastUid(Guid.NewGuid());
    public DateOnly Date { get; init; } 
    public int TemperatureC { get; init; }
    public string? Summary { get; init; }

    public EntityUid EntityUid => new(WeatherForecastUid.Value);
}
```

However, our data store is based on primitives so our Dbo object looks like this:

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

There's a discussion in the *Notes* on why this is good practice.

### Mocking the DI Container

For testing we need a DI container.  `GetServiceProvider`:

1. Builds out a DI root container with all the necessary services loaded.
2. Adds the debug logger.
3. Adds the test data to the database.

`_testDataProvider` provides access to the test data set to run data pipeline tests against.


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

We can now write a test to get the first record through the pipeline.  The steps are all explained in the code.

```csharp
    [Fact]
    public async void GetAForecast()
    {
        // Get a fully stocked DI container
        var provider = GetServiceProvider();

        //Get the data broker
        var broker = provider.GetService<IDataBroker>()!;

        // Get the test item from the Test Provider
        var testDboItem = _testDataProvider.WeatherForecasts.First();
        // Gets the Id to retrieve
        var testUid = testDboItem.Uid;

        // Get the Domain object - the Test data provider deals in dbo objects
        var testItem = DboWeatherForecastMap.Map(testDboItem);

        // Build an item request instance
        var request = new ItemQueryRequest(new(testUid));
        // Execute the query against the broker
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        // check the query was successful
        Assert.True(loadResult.Successful);
        
        // get the returned record 
        var dbItem = loadResult.Item;
        // check it matches the test record
        Assert.Equal(testItem, dbItem);
    }
```
