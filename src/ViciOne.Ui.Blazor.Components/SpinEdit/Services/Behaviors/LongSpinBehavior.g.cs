// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class LongSpinBehavior : ILongSpinBehavior
{
    public long Increment(long value, long interval, long minimum, long maximum)
    {
        if (value <= maximum - interval)
            return (long)(value + interval);

        return maximum;
    }

    public long Decrement(long value, long interval, long minimum, long maximum)
    {
        if (value >= minimum + interval)
            return (long)(value - interval);

        return minimum;
    }

    public long EnsureRange(long value, long minimum, long maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public long EnsureRaster(long value, long interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (long)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out long valueTyped)
        => long.TryParse(valueStr, out valueTyped);
}
