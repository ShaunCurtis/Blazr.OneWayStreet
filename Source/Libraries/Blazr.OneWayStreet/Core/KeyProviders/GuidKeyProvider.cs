/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using UuidExtensions;

namespace Blazr.OneWayStreet.Core;

public sealed class GuidKeyProvider : IKeyProvider<Guid>
{
    public Guid GetKey(object key)
    {
        if (key is Guid value)
            return value;

        throw new InvalidKeyProviderException();
    }

    public Guid GetNew()
        => Uuid7.Guid();
    public object GetValueObject(Guid key)
        => key;

    public bool IsDefault(Guid key)
        => key == Guid.Empty;
}
