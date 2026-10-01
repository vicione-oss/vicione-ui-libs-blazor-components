using System.Collections;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;

namespace ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;

/// <summary>
/// A concrete implementation of a <see cref="SimpleTable{TItem}"/> column that is highly customizable.
/// </summary>
public sealed class SimpleTableTemplateColumn<TItem> : AdvancedTableColumnBase<TItem>, IHasSortExpression<TItem>,
    IResizeableColumn, IFilterableColumn
        where TItem : class
{
    /// <summary>
    /// Custom content to be rendered within the column header.
    /// If provided, this overrides the usage of the <see cref="Title"/>.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// The text displayed in the column header.
    /// This serves as a fallback if <see cref="HeaderContent"/> is not specified.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Custom content to be rendered within each cell of this column.
    /// Provides access to the current <typeparamref name="TItem"/> instance for custom data visualization.
    /// </summary>
    [Parameter, EditorRequired]
    public required RenderFragment<TItem> CellContent { get; set; }

    /// <inheritdoc cref="IAdvancedTableColumn{TItem}.Id"/>
    [Parameter, EditorRequired]
    public required string Id { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public Expression<Func<TItem, object>>? SortExpression { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public IComparer? SortComparer { get; set; }

    /// <inheritdoc/>
    [Parameter]
    public int MinimumWidth { get; set; } = 50;

    /// <inheritdoc/>
    [Parameter]
    public bool Resizeable { get; set; }

    /// <summary>
    /// Starting width of the column in pixels. When not set, the column is a flex column and shares the
    /// leftover container space equally with the other flex columns, floored at <see cref="MinimumWidth"/> —
    /// and shares it out again every time that space changes.
    /// </summary>
    /// <remarks>
    /// One-way seed: it only sets the width the column starts out with. Once the user drags the column,
    /// the table's live width takes over; this parameter neither reflects nor controls the live value.
    /// A change in the space available to the table leaves it untouched. A value below
    /// <see cref="MinimumWidth"/> is clamped to <see cref="MinimumWidth"/>.
    /// </remarks>
    [Parameter]
    public int? Width { get; set; }

    bool ISortableColumn.Sortable => SortExpression is not null;

    /// <summary>
    /// Content rendered inside the filter panel when the filter icon is clicked.
    /// When <see langword="null"/>, no filter icon is shown.
    /// <para>
    /// An editor component derived from <see cref="ColumnFilterEditorBase"/> picks the
    /// <see cref="ColumnFilterEditorContext"/> up as a cascading value and needs nothing passed to it.
    /// Markup written inline here has no component to receive that cascade, so a one-off filter must be
    /// written as a component too.
    /// </para>
    /// </summary>
    [Parameter]
    public RenderFragment? FilterEditor { get; set; }

    /// <inheritdoc/>
    private protected override string GetColumnId()
        => Id;

    /// <inheritdoc/>
    private protected override RenderFragment GetHeader()
        => HeaderContent ?? (builder => builder.AddContent(0, Title));

    /// <inheritdoc/>
    private protected override RenderFragment<TItem> GetCellContent()
        => CellContent ?? (_ => _ => { });

    /// <inheritdoc/>
    private protected override int? GetDefaultWidth()
    {
        if (Width is { } width)
            return Math.Max(width, MinimumWidth);

        return null;
    }
}
