// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableDoubleSpinBehavior(IDoubleSpinBehavior doubleSpinBehavior) : INullableDoubleSpinBehavior
{
    public double? Increment(double? value, double interval, double minimum, double maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return doubleSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public double? Decrement(double? value, double interval, double minimum, double maximum)
    {
        if (value is null)
        {
            var zero = default(double);

            value = (double?)(zero - interval);

            if (value < minimum)
                return minimum;
            else
                return value;
        }

        return doubleSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public double? EnsureRange(double? value, double minimum, double maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public double? EnsureRaster(double? value, double interval)
    {
        if (value is null)
            return value;

        return doubleSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out double? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (double.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
