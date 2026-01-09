namespace ViciOne.Ui.Blazor.Components.CheckBox.Services;

internal interface ICheckBoxParameterDefaults<TValue>
{
    void Apply(ICheckBox<TValue> checkbox);
}
