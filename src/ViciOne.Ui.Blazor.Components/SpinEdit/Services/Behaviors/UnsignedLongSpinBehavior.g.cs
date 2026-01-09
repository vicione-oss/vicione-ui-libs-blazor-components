// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class UnsignedLongSpinBehavior : IUnsignedLongSpinBehavior
{
    public ulong Increment(ulong value, ulong interval, ulong minimum, ulong maximum)
    {
        if (value <= maximum - interval)
            return (ulong)(value + interval);

        return maximum;
    }

    public ulong Decrement(ulong value, ulong interval, ulong minimum, ulong maximum)
    {
        if (value >= minimum + interval)
            return (ulong)(value - interval);

        return minimum;
    }

    public ulong EnsureRange(ulong value, ulong minimum, ulong maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public ulong EnsureRaster(ulong value, ulong interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (ulong)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out ulong valueTyped)
        => ulong.TryParse(valueStr, out valueTyped);
}
