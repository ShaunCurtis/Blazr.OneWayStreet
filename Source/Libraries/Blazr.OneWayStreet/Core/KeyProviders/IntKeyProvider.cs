/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.OneWayStreet.Core;

public sealed class IntKeyProvider : IKeyProvider<int>
{
    public int GetKey(object key)
    {
        if (key is int value)
            return value;

        throw new InvalidKeyProviderException();
    }

    public int GetNew()
        => int.MinValue;

    public object GetValueObject(int key)
        => key;

    public bool IsDefault(int key)
        => key == 0;
}
