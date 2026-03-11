using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components;

/// <summary>
/// Component that displays and edits properties of one or more instances.
/// </summary>
public sealed partial class PropertyGrid<TContext> : ComponentBase, IDisposable
{
    private IPropertyGridController<TContext>? _controller;
    private IPropertyGridEvents? _events;
    private IPropertyGridState? _state;

    /// <summary>
    /// Controller for the property grid
    /// </summary>
    [Parameter, EditorRequired] public required IPropertyGridController<TContext> Controller { get; set; }

    private void ContextMenuVisibilityChanged(bool visible)
        => _events?.NotifyContextMenuVisibilityChanged(visible);

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Controller != _controller)
        {
            _controller = Controller;

            _state?.PropertiesChanged -= StatePropertiesChangedAsync;

            _state = Controller.State;
            _state.PropertiesChanged += StatePropertiesChangedAsync;

            _events = _controller.Events;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
        => _state?.PropertiesChanged -= StatePropertiesChangedAsync;

    private async void StatePropertiesChangedAsync(PropertiesChangedEventArgs args)
    {
        if (args.PropertyNames.Contains(nameof(IPropertyGridState.Items)) ||
            args.PropertyNames.Contains(nameof(IPropertyGridState.GroupByCategory)) ||
            args.PropertyNames.Contains(nameof(IPropertyGridState.KeepMessages)))
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}
