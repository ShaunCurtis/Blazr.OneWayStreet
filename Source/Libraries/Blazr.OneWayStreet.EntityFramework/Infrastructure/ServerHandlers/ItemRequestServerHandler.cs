/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Infrastructure;

public sealed class ItemRequestServerHandler<TDbContext>
    : IItemRequestHandler
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<TDbContext> _factory;
    private readonly IIdConverter _idConverter;

    public ItemRequestServerHandler(IServiceProvider serviceProvider, IDbContextFactory<TDbContext> factory, IIdConverter idConverter)
    {
        _serviceProvider = serviceProvider;
        _factory = factory;
        _idConverter = idConverter;
    }

    public async ValueTask<ItemQueryResult<TRecord>> ExecuteAsync<TRecord, TKey>(ItemQueryRequest<TKey> request)
        where TRecord : class
    {
        // Try and get a registered custom handler
        var _customHandler = _serviceProvider.GetService<IItemRequestHandler<TRecord, TKey>>();

        // If one is registered in DI and execute it
        if (_customHandler is not null)
            return await _customHandler.ExecuteAsync(request);

        // If not run the base handler
        return await this.GetItemAsync<TRecord, TKey>(request);
    }

    private async ValueTask<ItemQueryResult<TRecord>> GetItemAsync<TRecord, TKey>(ItemQueryRequest<TKey> request)
        where TRecord : class
    {
        using var dbContext = _factory.CreateDbContext();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;


        if (request.Key is null)
            return ItemQueryResult<TRecord>.Failure($"No Key provided");

       object? key = null;

        if (!_idConverter.TryConvert(request.Key, out key))
            return ItemQueryResult<TRecord>.Failure($"Could not convert provided value to an Id of {request.Key?.ToString()}");

        var record = await dbContext.Set<TRecord>()
            .FindAsync(key, request.Cancellation)
            .ConfigureAwait(false);

        if (record is null)
            return ItemQueryResult<TRecord>.Failure($"No record retrieved with the Key provided");

        return ItemQueryResult<TRecord>.Success(record);
    }
}
