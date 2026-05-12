namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Keys;

internal interface ICommonNumericPropertyKey : ICommonPropertyKey
{
    Type IntervalType { get; }
    Type LimitType { get; }
    bool IsRastered { get; }
}
