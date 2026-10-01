using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;

/// <summary>
/// Base class for a column's filter editor. Carries only what every editor shares: the cascaded
/// <see cref="ColumnFilterEditorContext"/>, a guard that the editor was placed in a column's filter slot, and the
/// apply/cancel plumbing.
/// <para>
/// A derived editor holds its own draft — one text value, a pair of bounds, a set of selected values, whatever it
/// needs — seeds it from <see cref="ColumnFilterEditorContext.ColumnFilter"/> in its own <see cref="OnInitialized"/>,
/// wires its own inputs, and turns the draft into a filter in <see cref="BuildFilter"/>.
/// </para>
/// </summary>
public abstract class ColumnFilterEditorBase : ComponentBase
{
    /// <summary>
    /// Cascaded by <see cref="ColumnFilterButton"/> and available to any editor component placed in a
    /// column's <c>FilterEditor</c> slot. Markup written inline in that slot has no component to receive a
    /// cascading value, so a one-off filter must be written as a component.
    /// </summary>
    [CascadingParameter]
    protected ColumnFilterEditorContext Context { get; set; } = default!;

    /// <summary>
    /// Builds the filter to apply from the editor's own draft. Returns <see langword="null"/> to clear the
    /// column's filter — each editor decides what an empty draft means for its own shape.
    /// </summary>
    /// <remarks>
    /// Must not throw: it runs on the user's Apply click, and an unhandled exception there kills the Blazor
    /// circuit.
    /// </remarks>
    protected abstract IColumnFilter? BuildFilter();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (Context is null)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} must be placed inside a column's {nameof(IFilterableColumn.FilterEditor)} slot.");
        }

        base.OnInitialized();
    }

    /// <summary>
    /// Applies the built filter and closes the panel.
    /// </summary>
    protected async Task ApplyAsync()
    {
        await Context.ColumnFilterChanged.InvokeAsync(BuildFilter());

        await Context.CloseRequested.InvokeAsync();
    }

    /// <summary>
    /// Closes the panel without changing the filter.
    /// </summary>
    protected Task CancelAsync()
        => Context.CloseRequested.InvokeAsync();
}
