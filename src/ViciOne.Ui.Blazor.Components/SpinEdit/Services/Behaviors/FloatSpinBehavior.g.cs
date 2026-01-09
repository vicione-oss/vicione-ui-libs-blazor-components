// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class FloatSpinBehavior : IFloatSpinBehavior
{
    public float Increment(float value, float interval, float minimum, float maximum)
    {
        if (value <= maximum - interval)
            return (float)(value + interval);

        return maximum;
    }

    public float Decrement(float value, float interval, float minimum, float maximum)
    {
        if (value >= minimum + interval)
            return (float)(value - interval);

        return minimum;
    }

    public float EnsureRange(float value, float minimum, float maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public float EnsureRaster(float value, float interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (float)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out float valueTyped)
        => float.TryParse(valueStr, out valueTyped);
}
