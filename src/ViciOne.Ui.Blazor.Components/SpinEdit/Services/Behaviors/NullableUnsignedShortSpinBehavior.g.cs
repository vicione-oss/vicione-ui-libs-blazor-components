// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableUnsignedShortSpinBehavior(IUnsignedShortSpinBehavior ushortSpinBehavior) : INullableUnsignedShortSpinBehavior
{
    public ushort? Increment(ushort? value, ushort interval, ushort minimum, ushort maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return ushortSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public ushort? Decrement(ushort? value, ushort interval, ushort minimum, ushort maximum)
    {
        if (value is null)
        {
            return minimum;
        }

        return ushortSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public ushort? EnsureRange(ushort? value, ushort minimum, ushort maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public ushort? EnsureRaster(ushort? value, ushort interval)
    {
        if (value is null)
            return value;

        return ushortSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out ushort? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (ushort.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
