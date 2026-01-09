// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableUnsignedLongSpinBehavior(IUnsignedLongSpinBehavior ulongSpinBehavior) : INullableUnsignedLongSpinBehavior
{
    public ulong? Increment(ulong? value, ulong interval, ulong minimum, ulong maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return ulongSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public ulong? Decrement(ulong? value, ulong interval, ulong minimum, ulong maximum)
    {
        if (value is null)
        {
            return minimum;
        }

        return ulongSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public ulong? EnsureRange(ulong? value, ulong minimum, ulong maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public ulong? EnsureRaster(ulong? value, ulong interval)
    {
        if (value is null)
            return value;

        return ulongSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out ulong? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (ulong.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
