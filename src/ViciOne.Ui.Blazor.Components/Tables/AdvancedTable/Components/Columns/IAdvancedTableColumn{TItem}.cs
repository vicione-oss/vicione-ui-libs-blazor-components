using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;

internal interface IAdvancedTableColumn<TItem>
    where TItem : class
{
    /// <summary>
    /// The unique identifier of the column.
    /// </summary>
    /// <remarks>
    /// Is used whenever something applies to the column, e.g. sorting.
    /// </remarks>
    string Id { get; }

    bool Visible { get; }

    PinSide PinSide { get; }

    /// <summary>
    /// Defines if the column is shown in the column chooser.
    /// If <see langword="false"/>, it is not shown and as a result can not be hidden.
    /// <para>
    /// Columns that carry structure rather than data — a select column or a navigation column — render no
    /// header text, so leaving them chooser-eligible offers the user an unlabeled row that hides selection
    /// or navigation. Set this to <see langword="false"/> on them.
    /// </para>
    /// </summary>
    bool ShowInColumnChooser { get; }

    /// <summary>
    /// The content rendered as the column's header.
    /// </summary>
    RenderFragment Header { get; }

    /// <summary>
    /// The content rendered into the column's cell for a row's item.
    /// </summary>
    RenderFragment<TItem> CellContent { get; }

    /// <summary>
    /// The width of the column in pixels before any user resize. <see langword="null"/> when the column shares
    /// the remaining space equally with the other columns that carry no default width.
    /// </summary>
    int? DefaultWidth { get; }

    /// <summary>
    /// The column's live visibility state, as most recently handed to <see cref="SetState"/>.
    /// </summary>
    ColumnState? State { get; }

    void SetState(ColumnState? columnState);
}
