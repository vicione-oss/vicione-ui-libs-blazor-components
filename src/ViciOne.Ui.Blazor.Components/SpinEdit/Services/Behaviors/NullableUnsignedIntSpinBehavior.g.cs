// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableUnsignedIntSpinBehavior(IUnsignedIntSpinBehavior uintSpinBehavior) : INullableUnsignedIntSpinBehavior
{
    public uint? Increment(uint? value, uint interval, uint minimum, uint maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return uintSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public uint? Decrement(uint? value, uint interval, uint minimum, uint maximum)
    {
        if (value is null)
        {
            return minimum;
        }

        return uintSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public uint? EnsureRange(uint? value, uint minimum, uint maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public uint? EnsureRaster(uint? value, uint interval)
    {
        if (value is null)
            return value;

        return uintSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out uint? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (uint.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
