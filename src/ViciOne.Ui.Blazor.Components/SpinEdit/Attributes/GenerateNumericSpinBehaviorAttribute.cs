namespace ViciOne.Ui.Blazor.Components.SpinEdit.Attributes;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
internal class GenerateNumericSpinBehaviorAttribute : Attribute
{
    public required Type Type { get; set; }
    public required string Alias { get; set; }
}
