/// ============================================================
/// Author: Shaun Curtis, Cold Elm Coders
/// License: Use And Donate
/// If you use it, donate something to a charity somewhere
/// ============================================================

namespace Blazr.OneWayStreet.Infrastructure;

public class IdConverter : IIdConverter
{
    public object Convert(object value)
    {
        if (this.TryConvert(value, out object? outValue))
            return outValue;

        return value;
    }

    public bool TryConvert(object inValue, [NotNullWhen(true)] out object? outValue)
    {
        switch (inValue)
        {
            case int:
                outValue = inValue;
                return true;

            case long:
                outValue = inValue;
                return true;

            case IRecordId recordId:
                outValue = recordId.GetKeyObject();
                return true;
        }

        if (long.TryParse(inValue.ToString(), out long longValue))
        {
            outValue = longValue;
            return true;
        }

        if (Guid.TryParse(inValue.ToString(), out Guid guidValue))
        {
            outValue = guidValue;
            return true;
        }

        outValue = null;
        return false;
    }
}
