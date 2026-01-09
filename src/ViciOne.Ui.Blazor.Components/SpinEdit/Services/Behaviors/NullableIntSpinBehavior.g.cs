// Auto-generated code

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class NullableIntSpinBehavior(IIntSpinBehavior intSpinBehavior) : INullableIntSpinBehavior
{
    public int? Increment(int? value, int interval, int minimum, int maximum)
    {
        if (value is null)
        {
            if (minimum > interval)
                return minimum;
            else
                return interval;
        }

        return intSpinBehavior.Increment(value.Value, interval, minimum, maximum);
    }

    public int? Decrement(int? value, int interval, int minimum, int maximum)
    {
        if (value is null)
        {
            var zero = default(int);

            value = (int?)(zero - interval);

            if (value < minimum)
                return minimum;
            else
                return value;
        }

        return intSpinBehavior.Decrement(value.Value, interval, minimum, maximum);
    }

    public int? EnsureRange(int? value, int minimum, int maximum)
    {
        if (value < minimum)
            return minimum;

        if (value > maximum)
            return maximum;

        return value;
    }

    public int? EnsureRaster(int? value, int interval)
    {
        if (value is null)
            return value;

        return intSpinBehavior.EnsureRaster(value.Value, interval);
    }

    public bool TryParse(string valueStr, out int? valueTyped)
    {
        if (string.IsNullOrEmpty(valueStr))
        {
            valueTyped = null;

            return true;
        }

        if (int.TryParse(valueStr, out var i))
        {
            valueTyped = i;

            return true;
        }

        valueTyped = default;
        return false;
    }
}
