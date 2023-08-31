# One Way Street

One Way Street is a read only data pipeline loosely based on CQS [Command/Query Separation].

It separates out:

- *Queries* - Requests for data from the primary data store
- *Commands* - Requests to mutation data within the primary data store.

The pattern can be defined [and summarised] in an `IDataBroker` interface.

```csharp
public interface IDataBroker
{
    public ValueTask<ListQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ListQueryRequest request) where TRecord : class;

    public ValueTask<ItemQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ItemQueryRequest request) where TRecord : class, IEntity;

    public ValueTask<CommandResult> ExecuteCommandAsync<TRecord>(CommandRequest<TRecord> request) where TRecord : class, IEntity;
}
```

Each method accepts a *Request* object tht provides the data required and returns a *Result* object.

`ExecuteQueryAsync` has two forms.  One for getting a `TRecord` item, and one for getting a collection of `TRecord`.

`ExecuteCommandAsync` has a single form.  The type of command is defined in the `CommandRequest`.  

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

### GetAForecast Test

`GetAForecast` demonstrates thw basic daa pipeline coding pattern.

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

### GetForecastList

`GetForecastList` demonstrates how to get a paged list from the data provider.

```csharp

    [Theory]
    [InlineData(0, 10)]
    [InlineData(0, 50)]
    [InlineData(5, 10)]
    public async void GetForecastList(int startIndex, int pageSize)
    {
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var testCount = _testDataProvider.WeatherForecasts.Count();
        var testFirstItem = DboWeatherForecastMap.Map(_testDataProvider.WeatherForecasts.Skip(startIndex).First());

        var request = new ListQueryRequest { PageSize = pageSize, StartIndex = startIndex };
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        Assert.Equal(testCount, loadResult.TotalCount);
        Assert.Equal(pageSize, loadResult.Items.Count());
        Assert.Equal(testFirstItem, loadResult.Items.First());
    }
```

### Filter

`GetAFilteredForecastList` demostrates how to add filtering to a request.
```csharp
    [Fact]
    public async void GetAFilteredForecastList()
    {
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var pageSize = 2;
        var testSummary = "Warm";
        var testQuery = _testDataProvider.WeatherForecasts.Where(item => testSummary.Equals(item.Summary, StringComparison.CurrentCultureIgnoreCase));

        var testCount = testQuery.Count();
        var testFirstItem = DboWeatherForecastMap.Map(testQuery.First());

        var filterDefinition = new FilterDefinition(ApplicationConstants.WeatherForecast.FilterWeatherForecastsBySummary, "Warm");
        var filters = new List<FilterDefinition>() { filterDefinition };
        var request = new ListQueryRequest { PageSize = pageSize, StartIndex = 0, Filters = filters };

        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        Assert.Equal(testCount, loadResult.TotalCount);
        Assert.Equal(pageSize, loadResult.Items.Count());
        Assert.Equal(testFirstItem, loadResult.Items.First());
    }
```

### UpdateAForecast Test

The `UpdateAForecast` test method demonstrates the basic usage of the command pipeline.

```csharp
    [Fact]
    public async void UpdateAForecast()
    {
        // Get a fully stocked DI container
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var testDboItem = _testDataProvider.WeatherForecasts.First();
        var testUid = testDboItem.Uid;
        var testItem = DboWeatherForecastMap.Map(testDboItem);

        var request = new ItemQueryRequest(new(testUid));
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);

        var dbItem = loadResult.Item!;
        var newItem = dbItem with { TemperatureC = dbItem.TemperatureC + 10 };

        var command = new CommandRequest<WeatherForecast>(newItem, CommandState.Update);
        var commandResult = await broker.ExecuteCommandAsync<WeatherForecast>(command);

        request = new ItemQueryRequest(new(testUid));
        loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        var dbNewItem = loadResult.Item!;

        Assert.Equal(newItem, dbNewItem);

        var testCount = _testDataProvider.WeatherForecasts.Count();

        var queryRequest = new ListQueryRequest { PageSize = 10, StartIndex = 0 };
        var queryResult = await broker.ExecuteQueryAsync<WeatherForecast>(queryRequest);
        Assert.True(queryResult.Successful);

        Assert.Equal(testCount, queryResult.TotalCount);
    }
```
