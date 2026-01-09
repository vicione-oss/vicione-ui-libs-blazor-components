namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Attributes;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
internal class GenerateNumericValueTypeDescriptorAttribute : Attribute
{
    public required Type Type { get; set; }
    public required string Alias { get; set; }
}
