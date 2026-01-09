namespace ViciOne.Ui.Blazor.Components.CheckBox;

internal interface ICheckBox<TValue>
{
    TValue ValueChecked { get; set; }
    TValue ValueUnchecked { get; set; }
    TValue ValueIndeterminate { get; set; }
    bool AllowIndeterminateState { get; set; }
}
