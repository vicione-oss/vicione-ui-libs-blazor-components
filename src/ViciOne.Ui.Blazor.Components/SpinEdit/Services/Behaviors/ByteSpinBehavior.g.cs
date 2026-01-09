// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class ByteSpinBehavior : IByteSpinBehavior
{
    public byte Increment(byte value, byte interval, byte minimum, byte maximum)
    {
        if (value <= maximum - interval)
            return (byte)(value + interval);

        return maximum;
    }

    public byte Decrement(byte value, byte interval, byte minimum, byte maximum)
    {
        if (value >= minimum + interval)
            return (byte)(value - interval);

        return minimum;
    }

    public byte EnsureRange(byte value, byte minimum, byte maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public byte EnsureRaster(byte value, byte interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (byte)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out byte valueTyped)
        => byte.TryParse(valueStr, out valueTyped);
}
