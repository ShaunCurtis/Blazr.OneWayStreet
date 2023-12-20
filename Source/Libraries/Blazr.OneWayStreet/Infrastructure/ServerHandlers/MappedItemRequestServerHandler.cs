/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Infrastructure;

/// <summary>
///  Executes an item query on the data store 
/// </summary>
/// <typeparam name="TDbContext">Database Context to apply query to</typeparam>
/// <typeparam name="TOutRecord">Domain Record to Map the incoming infrastructure record to</typeparam>
/// <typeparam name="TInRecord">Infrasrtructure Record to retrieved from the context</typeparam>
public sealed class MappedItemRequestServerHandler<TDbContext, TOutRecord, TInRecord>
    : IItemRequestHandler<TOutRecord>
    where TDbContext : DbContext
    where TInRecord : class, IKeyedEntity
    where TOutRecord : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<TDbContext> _factory;

    public MappedItemRequestServerHandler(IServiceProvider serviceProvider, IDbContextFactory<TDbContext> factory)
    {
        _serviceProvider = serviceProvider;
        _factory = factory;
    }

    public async ValueTask<ItemQueryResult<TOutRecord>> ExecuteAsync(ItemQueryRequest request)
    {
        return await this.GetItemAsync(request);
    }

    private async ValueTask<ItemQueryResult<TOutRecord>> GetItemAsync(ItemQueryRequest request)
    {
        // Get and check we have a mapper for the Dbo object to Dco Domain Model
        IDboEntityMap<TInRecord, TOutRecord>? mapper = null;
        mapper = _serviceProvider.GetService<IDboEntityMap<TInRecord, TOutRecord>>();

        // Throw an exception if we have no mapper defined 
        if (mapper is null)
            throw new DataPipelineException($"No mapper is defined for {this.GetType().FullName} for {(typeof(TInRecord).FullName)}");

        using var dbContext = _factory.CreateDbContext();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var inRecord = await dbContext.Set<TInRecord>().FindAsync(request.KeyValue, request.Cancellation);

        if (inRecord is null)
            return ItemQueryResult<TOutRecord>.Failure($"No record retrieved with a Uid of {request.KeyValue.ToString()}");

        var outRecord = mapper.MapTo(inRecord);

        if (outRecord is null)
            return ItemQueryResult<TOutRecord>.Failure($"Unable to map record retrieved with a Uid of {request.KeyValue.ToString()}");

        return ItemQueryResult<TOutRecord>.Success(outRecord);
    }
}