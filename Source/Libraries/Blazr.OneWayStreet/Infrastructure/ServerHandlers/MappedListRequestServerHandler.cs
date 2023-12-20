/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Blazr.OneWayStreet.Infrastructure;

/// <summary>
///  Executes a collection query on the data store 
/// </summary>
/// <typeparam name="TDbContext">Database Context to apply query to</typeparam>
/// <typeparam name="TOutRecord">Domain Record to Map the incoming infrastructure record to</typeparam>
/// <typeparam name="TInRecord">Infrasrtructure Record to retrieved from the context</typeparam>
public class MappedListRequestServerHandler<TDbContext, TOutRecord, TInRecord> : IListRequestHandler<TOutRecord>
    where TDbContext : DbContext
    where TInRecord : class
    where TOutRecord : class
{
    private readonly IServiceProvider _serviceProvider;

    private readonly IDbContextFactory<TDbContext> _factory;

    public MappedListRequestServerHandler(IDbContextFactory<TDbContext> factory, IServiceProvider serviceProvider)
    {
        _factory = factory;
        _serviceProvider = serviceProvider;
    }

    public virtual async ValueTask<ListQueryResult<TOutRecord>> ExecuteAsync(ListQueryRequest request)
    {
        return await GetQueryAsync(request);
    }

    protected async ValueTask<ListQueryResult<TOutRecord>> GetQueryAsync(ListQueryRequest request)
    {
        int totalRecordCount = 0;

        IRecordSortHandler<TInRecord>? sorterHandler = null;
        IRecordFilterHandler<TInRecord>? filterHandler = null;

        // Get and check we havw a mapper for the Dbo object to Dco Domain Model
        IDboEntityMap<TInRecord, TOutRecord>? mapper = null;
        mapper = _serviceProvider.GetService<IDboEntityMap<TInRecord, TOutRecord>>();

        // Throw an exception if we have no mapper defined 
        if (mapper is null)
            throw new DataPipelineException($"No mapper is defined for {this.GetType().FullName} for {(typeof(TOutRecord).FullName)}");

        // Get a Unit of Work DbContext for the scope of the method
        using var dbContext = _factory.CreateDbContext();
        // Turn off tracking.  We're only querying, no changes
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        // Get the IQueryable DbSet for TRecord
        IQueryable<TInRecord> inQuery = dbContext.Set<TInRecord>();

        // If we have filters defined we need to get the Filter Handler for TRecord
        // and apply the predicate delegates to the IQueryable instance
        if (request.Filters.Count() > 0)
        {
            // Get the Record Filter
            filterHandler = _serviceProvider.GetService<IRecordFilterHandler<TInRecord>>();

            // Throw an exception as we have filters defined, but no handler 
            if (filterHandler is null)
                throw new DataPipelineException($"Filters are defined in {this.GetType().FullName} for {(typeof(TOutRecord).FullName)} but no FilterProvider service is registered");

            // Apply the filters
            inQuery = filterHandler.AddFiltersToQuery(request.Filters, inQuery);
        }

        // Get the total record count after applying the filters
        totalRecordCount = inQuery is IAsyncEnumerable<TInRecord>
            ? await inQuery.CountAsync(request.Cancellation)
            : inQuery.Count();

        // If we have sorters we need to gets the Sort Handler for TRecord
        // and apply the sorters to thw IQueryable instance
        if (request.Sorters.Count() > 0)
        {
            sorterHandler = _serviceProvider.GetService<IRecordSortHandler<TInRecord>>();

            if (sorterHandler is null)
                throw new DataPipelineException($"Sorters are defined in {this.GetType().FullName} for {(typeof(TInRecord).FullName)} but no SorterProvider service is registered");

            inQuery = sorterHandler.AddSortsToQuery(inQuery, request.Sorters);
        }

        // Apply paging to the filtered and sorted IQueryable
        if (request.PageSize > 0)
            inQuery = inQuery
                .Skip(request.StartIndex)
                .Take(request.PageSize);

        // Apply the mapping to the query
        var outQuery = inQuery.Select(item => mapper.MapTo(item));

        // Materialize the out list from the data source
        var list = outQuery is IAsyncEnumerable<TOutRecord>
            ? await outQuery.ToListAsync()
            : outQuery.ToList();

        return ListQueryResult<TOutRecord>.Success(list, totalRecordCount);
    }
}

