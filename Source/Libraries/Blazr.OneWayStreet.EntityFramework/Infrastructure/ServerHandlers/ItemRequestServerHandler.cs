/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using Microsoft.Extensions.Logging;

namespace Blazr.OneWayStreet.Infrastructure;

public sealed class ItemRequestServerHandler<TDbContext>
    : IItemRequestHandler
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<TDbContext> _factory;
    private ILogger<ItemRequestServerHandler<TDbContext>> _logger;

    public ItemRequestServerHandler(IServiceProvider serviceProvider, IDbContextFactory<TDbContext> factory, ILogger<ItemRequestServerHandler<TDbContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _factory = factory;
        _logger = logger;
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
        IKeyProvider<TKey>? keyprovider = _serviceProvider.GetService<IKeyProvider<TKey>>();

        if (keyprovider is null)
        {
            var message = $"No IKeyProvider loaded in DI for {typeof(TKey).Name}";
            _logger.LogWarning(message);
        }

        using var dbContext = _factory.CreateDbContext();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        if (request.Key is null)
            return ItemQueryResult<TRecord>.Failure($"No Key provided");

        object key = request.Key;

        if (keyprovider != null)
            key = keyprovider.GetValueObject(request.Key);

        var record = await dbContext.Set<TRecord>()
            .FindAsync(key, request.Cancellation)
            .ConfigureAwait(false);

        if (record is null)
            return ItemQueryResult<TRecord>.Failure($"No record retrieved with the Key provided");

        return ItemQueryResult<TRecord>.Success(record);
    }
}
