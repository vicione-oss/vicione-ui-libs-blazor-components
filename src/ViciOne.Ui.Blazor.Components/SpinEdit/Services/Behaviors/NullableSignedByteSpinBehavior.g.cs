// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableSignedByteSpinBehavior(ISignedByteSpinBehavior sbyteSpinBehavior) : INullableSignedByteSpinBehavior
{
    public sbyte? Increment(sbyte? value, sbyte interval, sbyte minimum, sbyte maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return sbyteSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public sbyte? Decrement(sbyte? value, sbyte interval, sbyte minimum, sbyte maximum)
    {
        if (value is null)
        {
            var zero = default(sbyte);

            value = (sbyte?)(zero - interval);

            if (value < minimum)
                return minimum;
            else
                return value;
        }

        return sbyteSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public sbyte? EnsureRange(sbyte? value, sbyte minimum, sbyte maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public sbyte? EnsureRaster(sbyte? value, sbyte interval)
    {
        if (value is null)
            return value;

        return sbyteSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out sbyte? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (sbyte.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
