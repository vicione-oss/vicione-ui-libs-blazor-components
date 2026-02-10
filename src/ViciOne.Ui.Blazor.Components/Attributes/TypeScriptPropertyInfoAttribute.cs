namespace ViciOne.Ui.Blazor.Components.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal class TypeScriptPropertyInfoAttribute : Attribute
{
    public required string Type { get; set; }
}
