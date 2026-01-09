// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class DoubleSpinBehavior : IDoubleSpinBehavior
{
    public double Increment(double value, double interval, double minimum, double maximum)
    {
        if (value <= maximum - interval)
            return (double)(value + interval);

        return maximum;
    }

    public double Decrement(double value, double interval, double minimum, double maximum)
    {
        if (value >= minimum + interval)
            return (double)(value - interval);

        return minimum;
    }

    public double EnsureRange(double value, double minimum, double maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public double EnsureRaster(double value, double interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (double)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out double valueTyped)
        => double.TryParse(valueStr, out valueTyped);
}
