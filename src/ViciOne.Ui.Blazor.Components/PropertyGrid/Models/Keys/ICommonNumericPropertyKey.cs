namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

internal interface ICommonNumericPropertyKey : ICommonPropertyKey
{
    Type IntervalType { get; }
    Type LimitType { get; }
    bool IsRastered { get; }
}

internal interface ICommonNumericPropertyKey<TInterval, TLimit> : ICommonNumericPropertyKey
    where TInterval : struct
    where TLimit : struct
{
    TInterval Interval { get; }
    TLimit Minimum { get; }
    TLimit Maximum { get; }
}
