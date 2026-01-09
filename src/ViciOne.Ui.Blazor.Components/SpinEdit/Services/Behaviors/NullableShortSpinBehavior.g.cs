// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableShortSpinBehavior(IShortSpinBehavior shortSpinBehavior) : INullableShortSpinBehavior
{
    public short? Increment(short? value, short interval, short minimum, short maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return shortSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public short? Decrement(short? value, short interval, short minimum, short maximum)
    {
        if (value is null)
        {
            var zero = default(short);

            value = (short?)(zero - interval);

            if (value < minimum)
                return minimum;
            else
                return value;
        }

        return shortSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public short? EnsureRange(short? value, short minimum, short maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public short? EnsureRaster(short? value, short interval)
    {
        if (value is null)
            return value;

        return shortSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out short? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (short.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
