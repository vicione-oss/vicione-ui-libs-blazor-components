namespace ViciOne.Ui.Blazor.Components.ContextMenu.Models;

/// <summary>
/// Specifies the case of application for a context menu
/// </summary>
public sealed class ContextMenuApplicableTo(params Type[] types)
{
    /// <summary>
    /// Types representing the case of application for the context menu
    /// </summary>
    public IEnumerable<Type> Types { get; } = types;
}
