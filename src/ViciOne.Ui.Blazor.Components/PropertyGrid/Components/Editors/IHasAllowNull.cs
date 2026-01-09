namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

internal interface IHasAllowNull
{
    /// <summary>
    /// Indicates whether the editor allows <see langword="null"/> values.
    /// </summary>
    bool AllowNull { get; set; }
}
