# One Way Street

*OneWayStreet* is data pipeline loosely based on the CQS [Command/Query Separation] pattern.  This article provides an introduction and demonstrates it's usage using XUnit tests.

It's available as a Nuget Package - [Blazr.OneWayStreet](https://www.nuget.org/packages/Blazr.OneWayStreet).

> Note Version 2 is a rewrite of the library: there are too many breaking changes to document.  Version 1.x were pre-reelase versions.

It separates out:

- *Queries* - Requests for data
- *Commands* - Requests to mutation data.
 
The pattern is defined in the `IDataBroker` interface.

```csharp
public interface IDataBroker
{
    public ValueTask<ListQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ListQueryRequest request) 
        where TRecord : class;

    public ValueTask<ItemQueryResult<TRecord>> ExecuteQueryAsync<TRecord, TKey>(ItemQueryRequest<TKey> request)
        where TRecord : class;

    public ValueTask<CommandResult> ExecuteCommandAsync<TRecord>(CommandRequest<TRecord> request) 
        where TRecord : class;
}
```

Each method accepts a *Request* object providing the data required to execute the request, and returns a *Result* object.

`ExecuteQueryAsync` has two forms.  Pass an `ItemQueryRequest` and get a a single `TRecord` item, or a `ListQueryRequest` and get `TRecord` collection.

`ExecuteCommandAsync` executes an *Add/Update/Delete* action defined in the `CommandRequest`.  

```csharp
public record struct CommandRequest<TRecord>(TRecord Item, CommandState State, CancellationToken Cancellation = new());

public enum CommandState
{
    None = 0,
    Add = 1,
    Update = 2,
    Delete = int.MaxValue
}
```

The library provides two implementations:
 - A server based implementation of the pattern over Entity Framework Core.
 - An API implementation.  

### Service Definitions

Each test builds a Service Collection [as you would in a normal application].  `IServiceCollection` extension methods are used to encapsulate service provision for the framework and specific entities. 

```csharp
    private IServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddAppServerInfrastructureServices();
        services.AddLogging(builder => builder.AddDebug());

        // Create the Root and then a Scoped Provider
        var rootProvider = services.BuildServiceProvider();
        var provider = rootProvider.CreateAsyncScope().ServiceProvider;

        // get the DbContext factory and add the test data
        var factory = provider.GetService<IDbContextFactory<InMemoryTestDbContext>>();
        if (factory is not null)
            TestDataProvider.Instance().LoadDbContext<InMemoryTestDbContext>(factory);

        return provider!;
    }
```

`AddAppServerInfrastructureServices` is a `IServiceCollection` extension method.  It:

1. Adds the DBContext Factory.
2. Adds the Server Data Broker.
1. Add the IdConverter.
3. Adds the generic data handlers.
4. Calls the entity/feature specific extension methods. 

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
        services.AddWeatherForecastServerInfrastructureServices();
    }
```

`AddWeatherForecastServerInfrastructureServices` adds the entity specific services.  In this case, just the filter and sorter handlers.

```csharp
public static void AddWeatherForecastServerInfrastructureServices(this IServiceCollection services)
{
    services.AddTransient<IRecordFilterHandler<WeatherForecast>, WeatherForecastFilterHandler>();
    services.AddTransient<IRecordSortHandler<WeatherForecast>, WeatherForecastSortHandler>();
}
```

## Basic Setup Tests

### GetAForecast Test

`GetAForecast` demonstrates the basic data pipeline coding pattern.  The inline comments expain the detail.

The test gets an Id from the test provider and requests the record through the `IDataBroker`.

```csharp
[Fact]
public async void GetAForecast()
{
    // Get a fully stocked DI container
    var provider = GetServiceProvider();

    //Injects the data broker
    var broker = provider.GetService<IDataBroker>()!;

    // Get the test item and it's Id from the Test Provider
    var testItem = _testDataProvider.WeatherForecasts.First();
    var testUid = testItem.WeatherForecastUid;

    // Builds an item request instance and Executes the query against the broker
    var request = ItemQueryRequest.Create(testUid);
    var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);

    // check the query was successful
    Assert.True(loadResult.Successful);
    
    // get the returned record 
    var dbItem = loadResult.Item;

    // check it matches the test record
    Assert.Equal(testItem, dbItem);
}
```

### GetForecastList

`GetForecastList` demonstrates a paged list request.

```csharp
[Theory]
[InlineData(0, 10)]
[InlineData(0, 50)]
[InlineData(5, 10)]
public async void GetForecastList(int startIndex, int pageSize)
{
    var provider = GetServiceProvider();
    var broker = provider.GetService<IDataBroker>()!;

    // Get the total expected count and the first record of the page
    var testCount = _testDataProvider.WeatherForecasts.Count();
    var testFirstItem = _testDataProvider.WeatherForecasts.Skip(startIndex).First();

    // Create a request and execute it against the broker
    var request = new ListQueryRequest { PageSize = pageSize, StartIndex = startIndex };
    var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
    Assert.True(loadResult.Successful);

    // Check the results are as expected
    Assert.Equal(testCount, loadResult.TotalCount);
    Assert.Equal(pageSize, loadResult.Items.Count());
    Assert.Equal(testFirstItem, loadResult.Items.First());
}
```

### Filter

`GetAFilteredForecastList` demonstrates filtering a paged list from the data provider.

Filters are defined in classes using the *Specification* pattern and passed by theoir names.  This loose coupling works in both server and API's contexts.

```csharp
[Fact]
public async void GetAFilteredForecastList()
{
    var provider = GetServiceProvider();
    var broker = provider.GetService<IDataBroker>()!;

    // Set up the test data
    var pageSize = 2;
    var testSummary = "Warm";
    var testQuery = _testDataProvider.WeatherForecasts.Where(item => testSummary.Equals(item.Summary, StringComparison.CurrentCultureIgnoreCase));
    var testCount = testQuery.Count();
    var testFirstItem = testQuery.First();

    // define the filter to use
    var filterDefinition = new FilterDefinition(ApplicationConstants.WeatherForecast.FilterWeatherForecastsBySummary, "Warm");
    var filters = new List<FilterDefinition>() { filterDefinition };

    // Define the query and execute it against the broker
    var request = new ListQueryRequest { PageSize = pageSize, StartIndex = 0, Filters = filters };
    var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
    Assert.True(loadResult.Successful);

    // Test the results are as expected
    Assert.Equal(testCount, loadResult.TotalCount);
    Assert.Equal(pageSize, loadResult.Items.Count());
    Assert.Equal(testFirstItem, loadResult.Items.First());
}
```

### DeleteAForecast Test

The `UpdateAForecast` test method demonstrates the command pipeline: in this case a Delete.

```csharp
[Fact]
public async void DeleteAForecast()
{
    // Get a fully stocked DI container
    var provider = GetServiceProvider();
    var broker = provider.GetService<IDataBroker>()!;

    // get the test record
    var testItem = _testDataProvider.WeatherForecasts.First();
    var testUid = testItem.WeatherForecastUid;
    var testCount = _testDataProvider.WeatherForecasts.Count() - 1;

    // build a command and execute it against the database
    var command = new CommandRequest<WeatherForecast>(testItem, CommandState.Delete);
    var commandResult = await broker.ExecuteCommandAsync<WeatherForecast>(command);
    Assert.True(commandResult.Successful);

    // build a item request and ensure the record no longwer exists
    var request = ItemQueryRequest.Create(testUid);
    var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
    Assert.False(loadResult.Successful);

    // build a list query and check we have one less rcord 
    var queryRequest = new ListQueryRequest { PageSize = 10, StartIndex = 0 };
    var queryResult = await broker.ExecuteQueryAsync<WeatherForecast>(queryRequest);
    Assert.True(queryResult.Successful);
    Assert.Equal(testCount, queryResult.TotalCount);
}
```

### Editing a Record

"How do you edit a Record"?

This test demonstrates using a record edit context object, in this case `WeatherForecastEditContext`, to edit a record.  In a real world setting your edit form would plug into the record edit context, your validation would be on the record edit context, and you would create the `CommandRequest` object from `AsRecord`.    

```csharp
[Fact]
public async void UpdateAForecast()
{
    // Get a fully stocked DI container
    var provider = GetServiceProvider();
    var broker = provider.GetService<IDataBroker>()!;

    // Get a record id to edit
    var testItem = _testDataProvider.WeatherForecasts.First();
    var testUid = testItem.WeatherForecastUid;

    // Build an item query and execute it against the broker to get the record to edit
    var request = ItemQueryRequest.Create(testUid);
    var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
    Assert.True(loadResult.Successful);
    var dbItem = loadResult.Item!;

    // construct a recordEditContext for the record
    // Normally you would plug your edit form fields into this context
    // We just update the temperature
    var recordEditContext = new WeatherForecastEditContext(dbItem);
    recordEditContext.TemperatureC = recordEditContext.TemperatureC + 10;

    // In a real edit setting, you would be doing validation to ensure the
    // recordEditContext values are valid before attempting to save the record
    // Note that the validation is on the WeatherForecastEditContext, not WeatherForecast!
    var newItem = recordEditContext.AsRecord;

    // Create an update command and execute it against the broker
    var command = new CommandRequest<WeatherForecast>(newItem, CommandState.Update);
    var commandResult = await broker.ExecuteCommandAsync<WeatherForecast>(command);
    Assert.True(commandResult.Successful);

    // Get the updated record from the broker and test they are the same
    request = ItemQueryRequest.Create(testUid);
    loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
    Assert.True(loadResult.Successful);
    var dbNewItem = loadResult.Item!;
    Assert.Equal(newItem, dbNewItem);

    // Execute a list query against the data broker and check the count is still the same
    // i.e. we haven't added a record instead of updating one
    var queryRequest = new ListQueryRequest { PageSize = 10, StartIndex = 0 };
    var queryResult = await broker.ExecuteQueryAsync<WeatherForecast>(queryRequest);
    Assert.True(queryResult.Successful);

    var testCount = _testDataProvider.WeatherForecasts.Count();
    Assert.Equal(testCount, queryResult.TotalCount);
}
```

## Mapped Record Tests

In a normal application, the database table objects are mapped to domain objects.

The Service Provider setup is the same.

```csharp
    private IServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddAppServerMappedInfrastructureServices();
        services.AddLogging(builder => builder.AddDebug());

        // Create the Root and then a Scoped Provider
        var rootProvider = services.BuildServiceProvider();
        var provider = rootProvider.CreateAsyncScope().ServiceProvider;

        // get the DbContext factory and add the test data
        var factory = provider.GetService<IDbContextFactory<InMemoryTestDbContext>>();
        if (factory is not null)
            TestDataProvider.Instance().LoadDbContext<InMemoryTestDbContext>(factory);

        return provider!;
    }
```

As are the default handlers

```csharp
    public static void AddAppServerMappedInfrastructureServices(this IServiceCollection services)
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

The differences are in the specific WeatherForecast services.  We add:

1. The mapper to map between the inbfrastructure DTO and the Domain DTO.
1. Mapped Handlers with mapping DTO definitions.

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

The tests are basically the same with a bit of mapping to test for the correct entity.

```csharp
    [Fact]
    public async Task GetAForecast()
    {
        // Get a fully stocked DI container
        var provider = GetServiceProvider();

        //Get the data broker
        var broker = provider.GetService<IDataBroker>()!;

        // Get the test item from the Test Provider
        var testDboItem = _testDataProvider.DboWeatherForecasts.First();
        // Gets the Id to retrieve
        var testUid = testDboItem.Uid;

        // Get the Domain object - the Test data provider deals in dbo objects
        var testItem = WeatherForecastMap.Map(testDboItem);
        var testWeatherForecastId = testItem.WeatherForecastId;

        // Build an item request instance
        var request = ItemQueryRequest<WeatherForecastId>.Create(testWeatherForecastId);

        // Execute the query against the broker
        var loadResult = await broker.ExecuteQueryAsync<DcoWeatherForecast, WeatherForecastId>(request);

        // check the query was successful
        Assert.True(loadResult.Successful);

        // get the returned record 
        var dbItem = loadResult.Item;
        // check it matches the test record
        Assert.Equal(testItem, dbItem);
    }
```