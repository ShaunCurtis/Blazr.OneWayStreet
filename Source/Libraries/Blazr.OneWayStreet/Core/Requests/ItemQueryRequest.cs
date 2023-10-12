/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Core;

public readonly record struct ItemQueryRequest
{
    public EntityUid Uid { get; init; }
    public object KeyValue  { get; init; }
    public CancellationToken Cancellation { get; init; }

    public ItemQueryRequest(EntityUid? uid, object? keyValue, CancellationToken? cancellation = null)
    {
        this.Uid = uid ?? new(Guid.Empty);
        this.KeyValue = keyValue ?? Guid.Empty;
        this.Cancellation = cancellation ?? new(); ;
    }
    public static ItemQueryRequest Create(EntityUid uid, object keyValue, CancellationToken? cancellation = null)
        => new ItemQueryRequest(uid, keyValue, cancellation ?? new());
}
