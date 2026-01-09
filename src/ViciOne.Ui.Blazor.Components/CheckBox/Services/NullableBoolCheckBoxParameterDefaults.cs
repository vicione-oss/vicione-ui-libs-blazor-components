namespace ViciOne.Ui.Blazor.Components.CheckBox.Services;

internal sealed class NullableBoolCheckBoxParameterDefaults : ICheckBoxParameterDefaults<bool?>
{
    public void Apply(ICheckBox<bool?> checkbox)
    {
        checkbox.ValueChecked = true;
        checkbox.ValueUnchecked = false;
        checkbox.ValueIndeterminate = null;
        checkbox.AllowIndeterminateState = true;
    }
}
