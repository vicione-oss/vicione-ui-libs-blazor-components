namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

internal readonly record struct CommonNumericPropertyKey<TInterval, TLimit>(string Category, string Name, Type ValueType, TInterval Interval, TLimit Minimum, TLimit Maximum, bool IsRastered)
    : ICommonNumericPropertyKey<TInterval, TLimit>
        where TInterval : struct
        where TLimit : struct
{
    public Type IntervalType => typeof(TInterval);
    public Type LimitType => typeof(TLimit);
}
