// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class UnsignedIntSpinBehavior : IUnsignedIntSpinBehavior
{
    public uint Increment(uint value, uint interval, uint minimum, uint maximum)
    {
        if (value <= maximum - interval)
            return (uint)(value + interval);

        return maximum;
    }

    public uint Decrement(uint value, uint interval, uint minimum, uint maximum)
    {
        if (value >= minimum + interval)
            return (uint)(value - interval);

        return minimum;
    }

    public uint EnsureRange(uint value, uint minimum, uint maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public uint EnsureRaster(uint value, uint interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (uint)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out uint valueTyped)
        => uint.TryParse(valueStr, out valueTyped);
}
