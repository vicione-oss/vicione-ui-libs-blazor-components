// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableByteSpinBehavior(IByteSpinBehavior byteSpinBehavior) : INullableByteSpinBehavior
{
    public byte? Increment(byte? value, byte interval, byte minimum, byte maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return byteSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public byte? Decrement(byte? value, byte interval, byte minimum, byte maximum)
    {
        if (value is null)
        {
            return minimum;
        }

        return byteSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public byte? EnsureRange(byte? value, byte minimum, byte maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public byte? EnsureRaster(byte? value, byte interval)
    {
        if (value is null)
            return value;

        return byteSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out byte? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (byte.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
