/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using Blazr.App.Core;
using Blazr.App.Infrastructure;
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
        services.AddAppTestInfrastructureServices();
        services.AddLogging(builder => builder.AddDebug());

        var provider = services.BuildServiceProvider();

        // get the DbContext factory and add the test data
        var factory = provider.GetService<IDbContextFactory<InMemoryTestDbContext>>();
        if (factory is not null)
            TestDataProvider.Instance().LoadDbContext<InMemoryTestDbContext>(factory);

        return provider!;
    }

    [Fact]
    public async void LoadAForecast()
    {
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var testDboItem = _testDataProvider.WeatherForecasts.First();
        var testUid = testDboItem.Uid;

        var testItem = DboWeatherForecastMap.Map(testDboItem);

        var request = new ItemQueryRequest(new(testUid));
        var loadResult = await broker.ExecuteQueryAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        var dbItem = loadResult.Item;

        Assert.Equal(testItem, dbItem);
    }


    [Fact]
    public async void LoadAFilteredForecastList()
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

        var loadResult = await broker.GetItemsAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        Assert.Equal(testCount, loadResult.TotalCount);
        Assert.Equal(pageSize, loadResult.Items.Count());
        Assert.Equal(testFirstItem, loadResult.Items.First());
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(0, 50)]
    [InlineData(5, 10)]
    public async void LoadAForecastList(int startIndex, int pageSize)
    {
        var provider = GetServiceProvider();
        var broker = provider.GetService<IDataBroker>()!;

        var testCount = _testDataProvider.WeatherForecasts.Count();
        var testFirstItem = DboWeatherForecastMap.Map(_testDataProvider.WeatherForecasts.Skip(startIndex).First());

        var request = new ListQueryRequest { PageSize = pageSize, StartIndex = startIndex };
        var loadResult = await broker.GetItemsAsync<WeatherForecast>(request);
        Assert.True(loadResult.Successful);

        Assert.Equal(testCount, loadResult.TotalCount);
        Assert.Equal(pageSize, loadResult.Items.Count());
        Assert.Equal(testFirstItem, loadResult.Items.First());
    }

    //[Fact]
    //public async void UpdateAnEntity()
    //{
    //    var provider = GetServiceProvider();
    //    var broker = provider.GetService<IDataBroker>()!;
    //    var factory = provider.GetService<DiodeContextFactory>()!;
    //    var contextProvider = provider.GetService<DiodeContextProvider<Customer>>()!;

    //    var cancelToken = new CancellationToken();

    //    var originalCount = _testDataProvider.Customers.Count();
    //    var expectedCount = originalCount;
    //    var testItem = _testDataProvider.TestCustomer;
    //    var testUid = testItem.Uid;

    //    var request = new ItemQueryRequest(new(testUid));
    //    var loadResult = await factory.GetEntityFromProviderAsync<Customer>(request);
    //    Assert.True(loadResult.Successful);

    //    var context = loadResult.Item;

    //    // set up event registration

    //    DiodeContextChangeEventArgs<Customer>? contextEventArgs = null;
    //    int contextTimesEventCalled = 0;

    //    context.StateHasChanged += (sender, args) =>
    //    {
    //        contextTimesEventCalled++;
    //        contextEventArgs = args;
    //    };

    //    DiodeContextChangeEventArgs<Customer>? providerEventArgs = null;
    //    int providerTimesEventCalled = 0;

    //    contextProvider.StateHasChanged += (sender, args) =>
    //    {
    //        providerTimesEventCalled++;
    //        providerEventArgs = args;
    //    };

    //    var dbItem = loadResult.Item!.ImmutableItem;

    //    var editContext = new CustomerEditContext(dbItem!);

    //    var newCustomerName = $"{editContext.CustomerName} - Edited";

    //    editContext.CustomerName = newCustomerName;

    //    var mutateResult = await contextProvider.DispatchAsync<CustomerEditContext>(editContext);
    //    Assert.True(mutateResult.Successful);

    //    // need to yield to ensure the events have been raised before we test them
    //    await Task.Yield();
    //    Assert.Equal(1, contextTimesEventCalled);
    //    Assert.Equal(1, providerTimesEventCalled);

    //    var persistResult = await factory.PersistEntityToProviderAsync<Customer>(testUid);
    //    Assert.True(persistResult.Successful);

    //    Assert.Equal(2, contextTimesEventCalled);
    //    Assert.Equal(2, providerTimesEventCalled);

    //    var listRequest = new ListQueryRequest() { StartIndex = 0, PageSize = 1000, Cancellation = cancelToken };
    //    var listResult = await broker!.GetItemsAsync<Customer>(listRequest);
    //    Assert.True(listResult.Successful);
    //    Assert.Equal(expectedCount, listResult.TotalCount);

    //    var itemRequest = new ItemQueryRequest(new(testUid), cancelToken);
    //    var itemResult = await broker!.GetItemAsync<Customer>(itemRequest);
    //    Assert.True(itemResult.Successful);

    //    var expectedItem = _testDataProvider.TestCustomer with { CustomerName = newCustomerName };
    //    Assert.Equal(expectedItem, mutateResult.Item);
    //    Assert.Equal(expectedItem, itemResult.Item);

    //    Assert.Equal(testUid, contextEventArgs!.Uid);
    //    Assert.Equal(expectedItem, contextEventArgs!.MutatedItem.ImmutableItem);
    //    Assert.Equal(testUid, providerEventArgs!.Uid);
    //    Assert.Equal(expectedItem, providerEventArgs!.MutatedItem.ImmutableItem);

    //}


    //[Fact]
    //public async void DeleteAnEntity()
    //{
    //    var provider = GetServiceProvider();
    //    var broker = provider.GetService<IDataBroker>()!;
    //    var factory = provider.GetService<DiodeContextFactory>()!;
    //    var contextProvider = provider.GetService<DiodeContextProvider<Customer>>()!;

    //    var cancelToken = new CancellationToken();

    //    var originalCount = _testDataProvider.Customers.Count();
    //    var expectedCount = originalCount - 1;
    //    var testItem = _testDataProvider.TestCustomer;
    //    var testUid = testItem.Uid;

    //    var request = new ItemQueryRequest(new(testUid));
    //    var loadResult = await factory.GetEntityFromProviderAsync<Customer>(request);
    //    Assert.True(loadResult.Successful);

    //    var context = loadResult.Item;

    //    // set up event registration

    //    DiodeContextChangeEventArgs<Customer>? contextEventArgs = null;
    //    int contextTimesEventCalled = 0;

    //    context.StateHasChanged += (sender, args) =>
    //    {
    //        contextTimesEventCalled++;
    //        contextEventArgs = args;
    //    };

    //    DiodeContextChangeEventArgs<Customer>? providerEventArgs = null;
    //    int providerTimesEventCalled = 0;

    //    contextProvider.StateHasChanged += (sender, args) =>
    //    {
    //        providerTimesEventCalled++;
    //        providerEventArgs = args;
    //    };

    //    var deleteresult = contextProvider.MarkContextForDeletion(testUid);
    //    Assert.True(deleteresult.Successful);

    //    // need to yield to ensure the events have been raised before we test them
    //    await Task.Yield();
    //    Assert.Equal(1, contextTimesEventCalled);
    //    Assert.Equal(1, providerTimesEventCalled);

    //    var persistResult = await factory.PersistEntityToProviderAsync<Customer>(testUid);

    //    Assert.True(persistResult.Successful);

    //    var listRequest = new ListQueryRequest() { StartIndex = 0, PageSize = 1000, Cancellation = cancelToken };
    //    var listResult = await broker!.GetItemsAsync<Customer>(listRequest);

    //    Assert.True(listResult.Successful);
    //    Assert.Equal(expectedCount, listResult.TotalCount);

    //    var itemRequest = new ItemQueryRequest(new(testUid), cancelToken);
    //    var itemResult = await broker!.GetItemAsync<Customer>(itemRequest);

    //    Assert.False(itemResult.Successful);
    //}

    //[Fact]
    //public async void AddANewEntity()
    //{
    //    var provider = GetServiceProvider();
    //    var broker = provider.GetService<IDataBroker>()!;
    //    var factory = provider.GetService<DiodeContextFactory>()!;
    //    var contextProvider = provider.GetService<DiodeContextProvider<Customer>>()!;

    //    var cancelToken = new CancellationToken();

    //    var originalCount = _testDataProvider.Customers.Count();
    //    var expectedCount = originalCount + 1;

    //    var addResult = factory.CreateNewEntity<Customer>();

    //    Assert.True(addResult.Successful);

    //    var testUid = addResult.Item!.Uid;

    //    var editContext = new CustomerEditContext(addResult.Item.ImmutableItem);

    //    var newCustomerName = $"Dan Air";

    //    editContext.CustomerName = newCustomerName;

    //    var mutatedResult = await contextProvider.DispatchAsync<CustomerEditContext>(editContext);

    //    Assert.True(mutatedResult.Successful);

    //    var persistResult = await factory.PersistEntityToProviderAsync<Customer>(testUid);

    //    Assert.True(persistResult.Successful);

    //    var listRequest = new ListQueryRequest() { StartIndex = 0, PageSize = 1000, Cancellation = cancelToken };
    //    var listResult = await broker!.GetItemsAsync<Customer>(listRequest);

    //    Assert.True(listResult.Successful);
    //    Assert.Equal(expectedCount, listResult.TotalCount);

    //    var itemRequest = new ItemQueryRequest(new(testUid), cancelToken);
    //    var itemResult = await broker!.GetItemAsync<Customer>(itemRequest);

    //    Assert.True(itemResult.Successful);
    //    Assert.Equal(mutatedResult.Item, itemResult.Item);
    //}
}
