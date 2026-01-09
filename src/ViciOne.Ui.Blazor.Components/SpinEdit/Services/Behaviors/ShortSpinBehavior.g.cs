// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class ShortSpinBehavior : IShortSpinBehavior
{
    public short Increment(short value, short interval, short minimum, short maximum)
    {
        if (value <= maximum - interval)
            return (short)(value + interval);

        return maximum;
    }

    public short Decrement(short value, short interval, short minimum, short maximum)
    {
        if (value >= minimum + interval)
            return (short)(value - interval);

        return minimum;
    }

    public short EnsureRange(short value, short minimum, short maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public short EnsureRaster(short value, short interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (short)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out short valueTyped)
        => short.TryParse(valueStr, out valueTyped);
}
