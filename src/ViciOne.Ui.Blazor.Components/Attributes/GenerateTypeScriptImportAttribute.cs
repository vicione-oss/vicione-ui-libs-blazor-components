namespace ViciOne.Ui.Blazor.Components.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class GenerateTypeScriptImportAttribute : Attribute
{
    public required string Type { get; set; }
    public required string ModulePath { get; set; }
}
