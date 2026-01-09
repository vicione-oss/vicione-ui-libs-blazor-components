// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableFloatSpinBehavior(IFloatSpinBehavior floatSpinBehavior) : INullableFloatSpinBehavior
{
    public float? Increment(float? value, float interval, float minimum, float maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return floatSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public float? Decrement(float? value, float interval, float minimum, float maximum)
    {
        if (value is null)
        {
            var zero = default(float);

            value = (float?)(zero - interval);

            if (value < minimum)
                return minimum;
            else
                return value;
        }

        return floatSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public float? EnsureRange(float? value, float minimum, float maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public float? EnsureRaster(float? value, float interval)
    {
        if (value is null)
            return value;

        return floatSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out float? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (float.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
