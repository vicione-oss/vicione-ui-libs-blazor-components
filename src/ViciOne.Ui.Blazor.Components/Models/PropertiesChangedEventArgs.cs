using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.Models;

/// <summary>
/// Arguments for <see cref="IHasChangeableProperties.PropertiesChanged"/> event
/// </summary>
public sealed class PropertiesChangedEventArgs(IHasChangeableProperties sender, IReadOnlySet<string> propertyNames) : EventArgs
{
    /// <summary>
    /// Instance that raised the associated <see cref="IHasChangeableProperties.PropertiesChanged"/> event
    /// </summary>
    public IHasChangeableProperties Sender => sender;

    /// <summary>
    /// Name of the properties that have been changed
    /// </summary>
    public IReadOnlySet<string> PropertyNames => propertyNames;
}
