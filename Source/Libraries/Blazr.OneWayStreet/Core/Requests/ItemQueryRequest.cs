/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================
namespace Blazr.OneWayStreet.Core;

public readonly record struct ItemQueryRequest( EntityUid Uid, CancellationToken Cancellation = new ());
