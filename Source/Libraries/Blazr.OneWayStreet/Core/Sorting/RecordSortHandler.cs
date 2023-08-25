/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Core;

public abstract class RecordSortHandler<TRecord> 
    where TRecord : class
{
    protected Expression<Func<TRecord, object>>? DefaultSorter { get; set; }
    protected bool DefaultSortDescending { get; set; }

    /// <summary>
    /// Adds the sort definitions defined in definitions to the IQueryable query
    /// </summary>
    /// <param name="query"></param>
    /// <param name="definitions"></param>
    /// <returns></returns>
    public IQueryable<TRecord> AddSortsToQuery(IQueryable<TRecord> query, IEnumerable<SortDefinition> definitions)
    {
        if (definitions.Any())
        {
            foreach (var defintion in definitions)
                query = RecordSorterHelper.AddSort<TRecord>(query, defintion);

            return query;
        }

        query = AddDefaultSort(query);
        return query;
    }

    private IQueryable<TRecord> AddDefaultSort(IQueryable<TRecord> query)
    {
        if (this.DefaultSorter is not null)
        {
            query = this.DefaultSortDescending
            ? query.OrderByDescending(this.DefaultSorter)
            : query.OrderBy(this.DefaultSorter);
        }

        return query;
    }
}
