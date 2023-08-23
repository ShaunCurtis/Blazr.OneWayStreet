/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Core;

public interface IDataBroker
{
    public ValueTask<ListQueryResult<TRecord>> GetItemsAsync<TRecord>(ListQueryRequest request) where TRecord : class;

    public ValueTask<ItemQueryResult<TRecord>> GetItemAsync<TRecord>(ItemQueryRequest request) where TRecord : class, IEntity;

    public ValueTask<CommandResult> ExecuteCommandAsync<TRecord>(CommandRequest<TRecord> request) where TRecord : class, IEntity;
}
