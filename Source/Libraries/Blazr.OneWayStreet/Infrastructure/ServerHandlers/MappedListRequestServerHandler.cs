/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Blazr.OneWayStreet.Infrastructure;

/// <summary>
/// This class provides object mapping on top of the standard list handler
/// The in object is defined in the DI service definition
/// The out object is defined in ExecuteAsync
/// The in object is the Dbo object defining the record to retrieve from the data store
/// and should be defined in the DbContext
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
/// <typeparam name="TInRecord">This is the Dbo object the Handler will retrieve from the database</typeparam>
public class MappedListRequestServerHandler<TDbContext, TInRecord>
    : IListRequestHandler
    where TDbContext : DbContext
    where TInRecord : class
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<TDbContext> _factory;

    public MappedListRequestServerHandler(IDbContextFactory<TDbContext> factory, IServiceProvider serviceProvider)
    {
        _factory = factory;
        _serviceProvider = serviceProvider;
    }

    public virtual async ValueTask<ListQueryResult<TOutRecord>> ExecuteAsync<TOutRecord>(ListQueryRequest request)
        where TOutRecord : class
    {
        return await this.GetQueryAsync<TInRecord, TOutRecord>(request);
    }

    protected async ValueTask<ListQueryResult<TOut>> GetQueryAsync<TIn, TOut>(ListQueryRequest request)
        where TIn : class
        where TOut : class
    {
        int totalRecordCount = 0;

        IRecordSortHandler<TIn>? sorterHandler = null;
        IRecordFilterHandler<TIn>? filterHandler = null;

        // Get and check we havw a mapper for the Dbo object to Dco Domain Model
        IDboEntityMap<TIn, TOut>? mapper = null;
        mapper = _serviceProvider.GetService<IDboEntityMap<TIn, TOut>>();

        // Throw an exception if we have no mapper defined 
        if (mapper is null)
            throw new DataPipelineException($"No mapper is defined for {this.GetType().FullName} for {(typeof(TIn).FullName)}");

        // Get a Unit of Work DbContext for the scope of the method
        using var dbContext = _factory.CreateDbContext();
        // Turn off tracking.  We're only querying, no changes
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        // Get the IQueryable DbSet for TRecord
        IQueryable<TIn> inQuery = dbContext.Set<TIn>();

        // If we have filters defined we need to get the Filter Handler for TRecord
        // and apply the predicate delegates to the IQueryable instance
        if (request.Filters.Count() > 0)
        {
            // Get the Record Filter
            filterHandler = _serviceProvider.GetService<IRecordFilterHandler<TIn>>();

            // Throw an exception as we have filters defined, but no handler 
            if (filterHandler is null)
                throw new DataPipelineException($"Filters are defined in {this.GetType().FullName} for {(typeof(TIn).FullName)} but no FilterProvider service is registered");

            // Apply the filters
            inQuery = filterHandler.AddFiltersToQuery(request.Filters, inQuery);
        }

        // Get the total record count after applying the filters
        totalRecordCount = inQuery is IAsyncEnumerable<TIn>
            ? await inQuery.CountAsync(request.Cancellation)
            : inQuery.Count();

        // If we have sorters we need to gets the Sort Handler for TRecord
        // and apply the sorters to thw IQueryable instance
        if (request.Sorters.Count() > 0)
        {
            sorterHandler = _serviceProvider.GetService<IRecordSortHandler<TIn>>();

            if (sorterHandler is null)
                throw new DataPipelineException($"Sorters are defined in {this.GetType().FullName} for {(typeof(TIn).FullName)} but no SorterProvider service is registered");

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
        var list = outQuery is IAsyncEnumerable<TOut>
            ? await outQuery.ToListAsync()
            : outQuery.ToList();

        return ListQueryResult<TOut>.Success(list, totalRecordCount);
    }
}