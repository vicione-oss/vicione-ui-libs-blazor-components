// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class IntSpinBehavior : IIntSpinBehavior
{
    public int Increment(int value, int interval, int minimum, int maximum)
    {
        if (value <= maximum - interval)
            return (int)(value + interval);

        return maximum;
    }

    public int Decrement(int value, int interval, int minimum, int maximum)
    {
        if (value >= minimum + interval)
            return (int)(value - interval);

        return minimum;
    }

    public int EnsureRange(int value, int minimum, int maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public int EnsureRaster(int value, int interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (int)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out int valueTyped)
        => int.TryParse(valueStr, out valueTyped);
}
