# One Way Street

> Work in Progress
 
One Way Street is a data pipeline based on CQS [Command/Query Separation].

There are two *pipes* within the pipeline:
1. The Query Pipeline which fetches either a single record or a collection of records from the record store.
2. The Command Pipeline which takes a record and applies a change to the data store.

A data broker provides the gateway in to the pipeline.  The `IDataBroker` interface looks like this:

```
public interface IDataBroker
{
    public ValueTask<ListQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ListQueryRequest request) where TRecord : class;

    public ValueTask<ItemQueryResult<TRecord>> ExecuteQueryAsync<TRecord>(ItemQueryRequest request) where TRecord : class, IEntity;

    public ValueTask<CommandResult> ExecuteCommandAsync<TRecord>(CommandRequest<TRecord> request) where TRecord : class, IEntity;
}
```

Each method takes a *Request* and returns a *Result*.

To provide an overview of the pipleine I'll walk the reader through the configuration and some XUnit.

