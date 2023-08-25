# Lists

> Work in Progress

Working with Lists presents some significant challenges in any data pipeline.

Consider the classic *Repository Pattern* or *API* `GetAll`.  Seems Ok, but:

1. How many is all.  What happens if it's a million records?  Does the application handle that gracefully?
2. Do we get all and then filter it if we want a list of say all weather forecasts with a summary of *balmy*.  Pretty wasteful.
3. What does an empty or null dataset mean?

*One Way Street* defines a list query pattern like this:

```csharp
 public ValueTask<ListQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ListQueryRequest request) where TRecord : class;
 ```

The `ListQueryRequest` provides paging and defines filters and sorting to apply to the list.

```csharp
public sealed record ListQueryRequest
{
    public int StartIndex { get; init; } = 0;
    public int PageSize { get; init; } = 1000;
    public CancellationToken Cancellation { get; set; } = new();
    public IEnumerable<FilterDefinition> Filters { get; init; } = Enumerable.Empty<FilterDefinition>();
    public IEnumerable<SortDefinition> Sorters { get; init; } = Enumerable.Empty<SortDefinition>();
}
```

### Filters

Filters are defind as filter definitions which are a simple name/data pair.  The loose coupling means a `ListQueryRequest` can be passed over an API.
 

```csharp
public record struct FilterDefinition
{
    public string FilterName { get; init; } = string.Empty;
    public string FilterData { get; init; } = string.Empty;
}
```

Each filter is defined as class based on the *Specification* pattern:

```csharp
public class WeatherForecastsBySummarySpecification : PredicateSpecification<WeatherForecast>
{
    private string _summary;

    public WeatherForecastsBySummarySpecification(string summary)
    {
        _summary = summary;
    }

    public WeatherForecastsBySummarySpecification(FilterDefinition filter)
    {
        _summary = filter.FilterData.ToString();
    }

    public override Expression<Func<WeatherForecast, bool>> Expression
        => item => _summary.Equals(item.Summary, StringComparison.CurrentCultureIgnoreCase);
}
```

The `IRecordFilter` implementation provides the mapping between the `FilterName` in the definition and the specification class used to apply the filter.

`AddFilterToQuery` enumerates the list of filter definitions, and adds them to the provided `IQueryable` object.

```csharp
public interface IRecordFilter<TRecord>
    where TRecord : class
{
    public IQueryable<TRecord> AddFiltersToQuery(IEnumerable<FilterDefinition> filters, IQueryable<TRecord> query)
    {
        foreach (var filter in filters)
        {
            var specification = GetSpecification(filter);
            if (specification != null)
                query = specification.AsQueryAble(query);
        }

        if (query is IQueryable)
            return query;

        return query.AsQueryable();
    }

    protected IPredicateSpecification<TRecord>? GetSpecification(FilterDefinition filter)
        => null;
}
```

The entity implementation provides the map.

```csharp
public class WeatherForecastFilter : IRecordFilter<WeatherForecast>
{
    public IPredicateSpecification<WeatherForecast>? GetSpecification(FilterDefinition filter)
        => filter.FilterName switch
        {
            ApplicationConstants.WeatherForecast.FilterWeatherForecastsBySummary => new WeatherForecastsBySummarySpecification(filter),
            _ => null
        };
}
```

The filter is defined in DI:

```csharp
services.AddTransient<IRecordFilter<WeatherForecast>, WeatherForecastFilter>();
```

### Sorters

A sort definition defines a sort field by name and a direction.

```csharp
public record struct SortDefinition
{
    public string SortField { get; init; } = string.Empty;
    public bool SortDescending { get; init; }
}
```
The `RecordSorter` builds the sort expressions from the sort definitions and applies them to the provided `IQueryable`.

```csharp
public class RecordSorter<TRecord> : IRecordSorter<TRecord>
    where TRecord : class
{
    protected virtual Expression<Func<TRecord, object>>? DefaultSorter => null;
    protected virtual bool DefaultSortDescending { get; }

    public IQueryable<TRecord> AddSortsToQuery(IQueryable<TRecord> query, IEnumerable<SortDefinition> definitions)
    {
        if (definitions.Count() == 0)
        {
            query = AddDefaultSort(query);
            return query;
        }

        foreach (var defintion in definitions)
            query = AddSort(query, defintion);

        return query;
    }

    protected IQueryable<TRecord> AddSort(IQueryable<TRecord> query, SortDefinition definition)
    {
        Expression<Func<TRecord, object>>? expression = null;

        if (RecordSorterFactory.TryBuildSortExpression(definition.SortField, out expression))
        {
            if (expression is not null)
            {
                query = definition.SortDescending
                    ? query.OrderByDescending(expression)
                    : query.OrderBy(expression);
            }
        }

        return query;
    }

    protected IQueryable<TRecord> AddDefaultSort(IQueryable<TRecord> query)
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
```

The entity implementation defines the default sorter:

```csharp
public class WeatherForecastSorter : RecordSorter<WeatherForecast>, IRecordSorter<WeatherForecast>
{
    protected override Expression<Func<WeatherForecast, object>> DefaultSorter { get; } = (item) => item.Date;
    protected override bool DefaultSortDescending { get; } = false;
}
```

>Need More




