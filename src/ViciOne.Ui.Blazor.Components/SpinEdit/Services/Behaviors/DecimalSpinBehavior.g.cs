// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class DecimalSpinBehavior : IDecimalSpinBehavior
{
    public decimal Increment(decimal value, decimal interval, decimal minimum, decimal maximum)
    {
        if (value <= maximum - interval)
            return (decimal)(value + interval);

        return maximum;
    }

    public decimal Decrement(decimal value, decimal interval, decimal minimum, decimal maximum)
    {
        if (value >= minimum + interval)
            return (decimal)(value - interval);

        return minimum;
    }

    public decimal EnsureRange(decimal value, decimal minimum, decimal maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public decimal EnsureRaster(decimal value, decimal interval)
    {
        var remainder = value % interval;
        if (remainder != 0)
            value = (decimal)(value - remainder);

        return value;
    }

    public bool TryParse(string valueStr, out decimal valueTyped)
        => decimal.TryParse(valueStr, out valueTyped);
}
