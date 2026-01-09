// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableDecimalSpinBehavior(IDecimalSpinBehavior decimalSpinBehavior) : INullableDecimalSpinBehavior
{
    public decimal? Increment(decimal? value, decimal interval, decimal minimum, decimal maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return decimalSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public decimal? Decrement(decimal? value, decimal interval, decimal minimum, decimal maximum)
    {
        if (value is null)
        {
            var zero = default(decimal);

            value = (decimal?)(zero - interval);

            if (value < minimum)
                return minimum;
            else
                return value;
        }

        return decimalSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public decimal? EnsureRange(decimal? value, decimal minimum, decimal maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public decimal? EnsureRaster(decimal? value, decimal interval)
    {
        if (value is null)
            return value;

        return decimalSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out decimal? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (decimal.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
