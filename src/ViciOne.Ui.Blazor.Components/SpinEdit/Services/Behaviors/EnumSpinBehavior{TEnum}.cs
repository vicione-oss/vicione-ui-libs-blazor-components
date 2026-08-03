using ViciOne.Ui.Blazor.Components.Enums;
using ViciOne.Ui.Blazor.Components.Factories;

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

internal sealed class EnumSpinBehavior<TEnum>(ISpinBehavior<int, int, int> intSpinBehavior) : IEnumSpinBehavior<TEnum>
    where TEnum : struct, IEquatable<TEnum>, ITypeSafeEnumImplemention<TEnum>
{
    private readonly IList<TEnum> _enumValues = [.. TypeSafeEnumFactory<TEnum>.CreateAll()];

    public TEnum Increment(TEnum value, int interval, TEnum minimum, TEnum maximum)
    {
        var valueIndex = _enumValues.IndexOf(value);
        var minimumIndex = _enumValues.IndexOf(minimum);
        var maximumIndex = _enumValues.IndexOf(maximum);

        valueIndex = intSpinBehavior.Increment(valueIndex, interval, minimumIndex, maximumIndex);

        return _enumValues[valueIndex];
    }

    public TEnum Decrement(TEnum value, int interval, TEnum minimum, TEnum maximum)
    {
        var valueIndex = _enumValues.IndexOf(value);
        var minimumIndex = _enumValues.IndexOf(minimum);
        var maximumIndex = _enumValues.IndexOf(maximum);

        valueIndex = intSpinBehavior.Decrement(valueIndex, interval, minimumIndex, maximumIndex);

        return _enumValues[valueIndex];
    }

    public TEnum EnsureRange(TEnum value, TEnum minimum, TEnum maximum)
    {
        var valueIndex = _enumValues.IndexOf(value);
        var minimumIndex = _enumValues.IndexOf(minimum);
        var maximumIndex = _enumValues.IndexOf(maximum);

        valueIndex = intSpinBehavior.EnsureRange(valueIndex, minimumIndex, maximumIndex);

        return _enumValues[valueIndex];
    }

    public TEnum EnsureRaster(TEnum value, int interval)
    {
        var valueIndex = _enumValues.IndexOf(value);

        valueIndex = intSpinBehavior.EnsureRaster(valueIndex, interval);

        return _enumValues[valueIndex];
    }

    public bool TryParse(string valueStr, out TEnum valueTyped)
        => TypeSafeEnumFactory<TEnum>.TryCreate(valueStr, out valueTyped);
}
