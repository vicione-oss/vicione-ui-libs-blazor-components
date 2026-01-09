// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableLongSpinBehavior(ILongSpinBehavior longSpinBehavior) : INullableLongSpinBehavior
{
    public long? Increment(long? value, long interval, long minimum, long maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return longSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public long? Decrement(long? value, long interval, long minimum, long maximum)
    {
        if (value is null)
        {
            var zero = default(long);

            value = (long?)(zero - interval);

            if (value < minimum)
                return minimum;
            else
                return value;
        }

        return longSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public long? EnsureRange(long? value, long minimum, long maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public long? EnsureRaster(long? value, long interval)
    {
        if (value is null)
            return value;

        return longSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out long? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (long.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
