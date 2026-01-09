// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class UnsignedShortSpinBehavior : IUnsignedShortSpinBehavior
{
    public ushort Increment(ushort value, ushort interval, ushort minimum, ushort maximum)
    {
        if (value <= maximum - interval)
            return (ushort)(value + interval);

        return maximum;
    }

    public ushort Decrement(ushort value, ushort interval, ushort minimum, ushort maximum)
    {
        if (value >= minimum + interval)
            return (ushort)(value - interval);

        return minimum;
    }

    public ushort EnsureRange(ushort value, ushort minimum, ushort maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public ushort EnsureRaster(ushort value, ushort interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (ushort)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out ushort valueTyped)
        => ushort.TryParse(valueStr, out valueTyped);
}
