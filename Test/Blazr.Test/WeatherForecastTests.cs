/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Core;
using Blazr.App.Infrastructure;
using Blazr.Core.OWS;
using Blazr.OneWayStreet.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazr.Test;

public class WeatherForecastTests
{
    private TestDataProvider _testDataProvider;

    public WeatherForecastTests()
        => _testDataProvider = TestDataProvider.Instance();

    private ServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddAppServerInfrastructureServices();
        services.AddLogging(builder => builder.AddDebug());

        var provider = services.BuildServiceProvider();

        // get the DbContext factory and add the test data
        var factory = provider.GetService<IDbContextFactory<InMemoryTestDbContext>>();
        if (factory is not null)
            TestDataProvider.Instance().LoadDbContext<InMemoryTestDbContext>(factory);

        return provider!;
    }

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
        var testItem = WeatherForecastMap.Map(testDboItem);

        // Build an item request instance
        var request = ItemQueryRequest.Create(new(testUid), testUid);
        // Execute the query against the broker
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        // check the query was successful
        Assert.True(loadResult.Successful);
        
        // get the returned record 
        var dbItem = loadResult.Item;
        // check it matches the test record
        Assert.Equal(testItem, dbItem);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(0, 50)]
    [InlineData(5, 10)]
    public async void GetForecastList(int startIndex, int pageSize)
    {
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var testCount = _testDataProvider.WeatherForecasts.Count();
        var testFirstItem = WeatherForecastMap.Map(_testDataProvider.WeatherForecasts.Skip(startIndex).First());

        var request = new ListQueryRequest { PageSize = pageSize, StartIndex = startIndex };
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        Assert.Equal(testCount, loadResult.TotalCount);
        Assert.Equal(pageSize, loadResult.Items.Count());
        Assert.Equal(testFirstItem, loadResult.Items.First());
    }

    [Fact]
    public async void GetAFilteredForecastList()
    {
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var pageSize = 2;
        var testSummary = "Warm";
        var testQuery = _testDataProvider.WeatherForecasts.Where(item => testSummary.Equals(item.Summary, StringComparison.CurrentCultureIgnoreCase));

        var testCount = testQuery.Count();
        var testFirstItem = WeatherForecastMap.Map(testQuery.First());

        var filterDefinition = new FilterDefinition(ApplicationConstants.WeatherForecast.FilterWeatherForecastsBySummary, "Warm");
        var filters = new List<FilterDefinition>() { filterDefinition };
        var request = new ListQueryRequest { PageSize = pageSize, StartIndex = 0, Filters = filters };

        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        Assert.Equal(testCount, loadResult.TotalCount);
        Assert.Equal(pageSize, loadResult.Items.Count());
        Assert.Equal(testFirstItem, loadResult.Items.First());
    }

    [Fact]
    public async void GetASortedForecastList()
    {
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var testCount = _testDataProvider.WeatherForecasts.Count();
        var testFirstItem = WeatherForecastMap.Map(_testDataProvider.WeatherForecasts.Last());

        SortDefinition sort = new("Date", true);
        var sortList = new List<SortDefinition>() { sort }; 

        var request = new ListQueryRequest { PageSize = 10000, StartIndex = 0, Sorters = sortList };
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        Assert.Equal(testFirstItem, loadResult.Items.First());

        sort = new("Date", false);
        sortList = new List<SortDefinition>() { sort };

        request = new ListQueryRequest { PageSize = 100000, StartIndex = 0, Sorters = sortList };
        loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        Assert.Equal(testFirstItem, loadResult.Items.Last());
    }

    [Fact]
    public async void UpdateAForecast()
    {
        // Get a fully stocked DI container
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var testDboItem = _testDataProvider.WeatherForecasts.First();
        var testUid = testDboItem.Uid;

        var testItem = WeatherForecastMap.Map(testDboItem);

        var request = ItemQueryRequest.Create(new(testUid), testUid);
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        var dbItem = loadResult.Item!;

        var recordEditContext = new WeatherForecastEditContext(dbItem);

        recordEditContext.TemperatureC = recordEditContext.TemperatureC + 10;

        // In a real edit setting, you would be doing validation to ensure the
        // recordEditContext values are valid before attempting to save the record
        // Note that the validation is on the WeatherForecastEditContext, not WeatherForecast!
        var newItem = recordEditContext.AsRecord;

        var command = new CommandRequest<WeatherForecast>(newItem, CommandState.Update);
        var commandResult = await broker.ExecuteCommandAsync<WeatherForecast>(command);
        Assert.True(commandResult.Successful);

        request = ItemQueryRequest.Create(new(testUid), testUid);
        loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        var dbNewItem = loadResult.Item!;

        Assert.Equal(newItem, dbNewItem);

        var testCount = _testDataProvider.WeatherForecasts.Count();

        var queryRequest = new ListQueryRequest { PageSize = 10, StartIndex = 0 };
        var queryResult = await broker.ExecuteQueryAsync<WeatherForecast>(queryRequest);
        Assert.True(queryResult.Successful);

        Assert.Equal(testCount, queryResult.TotalCount);
    }

    [Fact]
    public async void DeleteAForecast()
    {
        // Get a fully stocked DI container
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var testDboItem = _testDataProvider.WeatherForecasts.First();
        var testUid = testDboItem.Uid;

        var testItem = WeatherForecastMap.Map(testDboItem);

        var command = new CommandRequest<WeatherForecast>(testItem, CommandState.Delete);
        var commandResult = await broker.ExecuteCommandAsync<WeatherForecast>(command);
        Assert.True(commandResult.Successful);

        var testCount = _testDataProvider.WeatherForecasts.Count() - 1;

        var queryRequest = new ListQueryRequest { PageSize = 10, StartIndex = 0 };
        var queryResult = await broker.ExecuteQueryAsync<WeatherForecast>(queryRequest);
        Assert.True(queryResult.Successful);

        Assert.Equal(testCount, queryResult.TotalCount);
    }

    [Fact]
    public async void AddAForecast()
    {
        // Get a fully stocked DI container
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var newItem = new WeatherForecast { WeatherForecastUid = new(Guid.NewGuid()), Date = DateOnly.FromDateTime(DateTime.Now), Summary = "Testing", TemperatureC = 30 };

        var command = new CommandRequest<WeatherForecast>(newItem, CommandState.Add);
        var commandResult = await broker.ExecuteCommandAsync<WeatherForecast>(command);
        Assert.True(commandResult.Successful);

        var request = ItemQueryRequest.Create(newItem.EntityUid, newItem.EntityUid.Value);
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        var dbNewItem = loadResult.Item!;

        Assert.Equal(newItem, dbNewItem);

        var testCount = _testDataProvider.WeatherForecasts.Count() + 1;

        var queryRequest = new ListQueryRequest { PageSize = 10, StartIndex = 0 };
        var queryResult = await broker.ExecuteQueryAsync<WeatherForecast>(queryRequest);
        Assert.True(queryResult.Successful);

        Assert.Equal(testCount, queryResult.TotalCount);
    }
}
