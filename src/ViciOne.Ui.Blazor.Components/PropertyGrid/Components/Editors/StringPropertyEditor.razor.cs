using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Interfaces;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

/// <summary>
/// Property editor for string values
/// </summary>
public sealed partial class StringPropertyEditor
    : PropertyEditorBase<string?>, IHasUpdateKey, IHasEnterPressed<string?>, IHasEscapePressed<string?>
{
    /// <inheritdoc/>
    [Parameter] public object? UpdateKey { get; set; }

    /// <inheritdoc/>
    [Parameter] public EventCallback<string?> EnterPressed { get; set; }

    /// <inheritdoc/>
    [Parameter] public EventCallback<string?> EscapePressed { get; set; }
}
