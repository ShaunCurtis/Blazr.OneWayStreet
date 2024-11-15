/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

using UuidExtensions;

namespace Blazr.OneWayStreet.Core;

public sealed class LongKeyProvider : IKeyProvider<long>
{
    public long GetKey(object key)
    {
        if (key is long value)
            return value;

        throw new InvalidKeyProviderException();
    }

    public long GetNew()
        => long.MinValue;

    public object GetValueObject(long key)
        => key;

    public bool IsDefault(long key)
        => key == 0;
}
