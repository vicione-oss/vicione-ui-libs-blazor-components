using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Grid.Services;
using ViciOne.Ui.Localization.Resources;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.Footers;

/// <summary>
///  Represents a <see cref="Grid{TGridItem}"/> footer, which contains the total number of elements and the selection.
/// </summary>
/// <typeparam name="TGridItemKey">The type of selection key.</typeparam>
public sealed partial class SelectionFooter<TGridItemKey> : IDisposable
{
    private IGridItemSelection<TGridItemKey>? _attachedSelection;

    [CascadingParameter]
    private int ItemCount { get; set; } = default!;

    /// <summary>
    /// List of the selected values
    /// </summary>
    [Parameter, EditorRequired]
    public IGridItemSelection<TGridItemKey> GridSelection { get; set; }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!ReferenceEquals(_attachedSelection, GridSelection))
        {
            if (_attachedSelection is not null)
                _attachedSelection.Changed -= GridSelectionChangedAsync;

            GridSelection.Changed += GridSelectionChangedAsync;
            _attachedSelection = GridSelection;
        }
    }

    private async void GridSelectionChangedAsync(GridItemSelectionChangedEventArgs<TGridItemKey> obj)
        => await InvokeAsync(StateHasChanged);

    private string GetContentString()
    {
        var elements = CommonVocabulary.ElementPlural;

        if (ItemCount is 1)
            elements = CommonVocabulary.Element;

        if (GridSelection.Count <= 0)
            return $"{ItemCount} {elements}";

        return $"{ItemCount} {elements} | {GridSelection.Count} {CommonVocabulary.Selected}";
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_attachedSelection is not null)
            _attachedSelection.Changed -= GridSelectionChangedAsync;
    }
}
