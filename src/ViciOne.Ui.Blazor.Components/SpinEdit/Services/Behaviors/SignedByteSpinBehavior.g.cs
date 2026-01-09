// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class SignedByteSpinBehavior : ISignedByteSpinBehavior
{
    public sbyte Increment(sbyte value, sbyte interval, sbyte minimum, sbyte maximum)
    {
        if (value <= maximum - interval)
            return (sbyte)(value + interval);

        return maximum;
    }

    public sbyte Decrement(sbyte value, sbyte interval, sbyte minimum, sbyte maximum)
    {
        if (value >= minimum + interval)
            return (sbyte)(value - interval);

        return minimum;
    }

    public sbyte EnsureRange(sbyte value, sbyte minimum, sbyte maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public sbyte EnsureRaster(sbyte value, sbyte interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (sbyte)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out sbyte valueTyped)
        => sbyte.TryParse(valueStr, out valueTyped);
}
