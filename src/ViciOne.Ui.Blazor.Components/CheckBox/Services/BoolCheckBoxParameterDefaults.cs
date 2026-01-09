namespace ViciOne.Ui.Blazor.Components.CheckBox.Services;

internal sealed class BoolCheckBoxParameterDefaults : ICheckBoxParameterDefaults<bool>
{
    public void Apply(ICheckBox<bool> checkbox)
    {
        checkbox.ValueChecked = true;
        checkbox.ValueUnchecked = false;
        checkbox.AllowIndeterminateState = false;
    }
}
