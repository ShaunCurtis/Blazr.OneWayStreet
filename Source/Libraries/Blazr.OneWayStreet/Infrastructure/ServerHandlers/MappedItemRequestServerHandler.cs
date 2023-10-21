/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Infrastructure;

public sealed class MappedItemRequestServerHandler<TDbContext, TIn>
    : IItemRequestHandler
    where TDbContext : DbContext
    where TIn : class, IKeyedEntity
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<TDbContext> _factory;

    public MappedItemRequestServerHandler(IServiceProvider serviceProvider, IDbContextFactory<TDbContext> factory)
    {
        _serviceProvider = serviceProvider;
        _factory = factory;
    }

    public async ValueTask<ItemQueryResult<TOut>> ExecuteAsync<TOut>(ItemQueryRequest request)
        where TOut : class
    {
        // Try and get a registered custom handler
        var _customHandler = _serviceProvider.GetService<IItemRequestHandler<TOut>>();

        // If we one is registered in DI and execute it
        if (_customHandler is not null)
            return await _customHandler.ExecuteAsync(request);

        // If not run the base handler
        return await this.GetItemAsync<TOut>(request);
    }

    private async ValueTask<ItemQueryResult<TOut>> GetItemAsync<TOut>(ItemQueryRequest request)
    where TOut : class
    {
        // Get and check we have a mapper for the Dbo object to Dco Domain Model
        IDboEntityMap<TIn, TOut>? mapper = null;
        mapper = _serviceProvider.GetService<IDboEntityMap<TIn, TOut>>();

        // Throw an exception if we have no mapper defined 
        if (mapper is null)
            throw new DataPipelineException($"No mapper is defined for {this.GetType().FullName} for {(typeof(TIn).FullName)}");

        using var dbContext = _factory.CreateDbContext();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var inRecord = await dbContext.Set<TIn>().FindAsync(request.KeyValue, request.Cancellation);

        if (inRecord is null)
            return ItemQueryResult<TOut>.Failure($"No record retrieved with a Uid of {request.KeyValue.ToString()}");

        var outRecord = mapper.MapTo(inRecord);

        if (outRecord is null)
            return ItemQueryResult<TOut>.Failure($"Unable to map record retrieved with a Uid of {request.KeyValue.ToString()}");

        return ItemQueryResult<TOut>.Success(outRecord);
    }
}