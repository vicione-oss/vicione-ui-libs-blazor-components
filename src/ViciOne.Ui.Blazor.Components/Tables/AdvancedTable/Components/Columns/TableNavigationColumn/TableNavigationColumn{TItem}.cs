using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.TableNavigationColumn;

/// <summary>
/// A specialized column for the <see cref="AdvancedTable{TItem}"/> that provides row-level navigation capabilities.
/// </summary>
public sealed class TableNavigationColumn<TItem> : AdvancedTableColumnBase<TItem>
    where TItem : class
{
    private readonly string _id = Guid.NewGuid().ToString();

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#attr-title">title</see> attribute of the navigation buttons
    /// </summary>
    [Parameter]
    public string? Tooltip { get; set; }

    /// <summary>
    /// Raised when a navigation button has been clicked
    /// </summary>
    [Parameter]
    public EventCallback<TItem> Navigate { get; set; }

    /// <summary>
    /// Creates a column whose chooser row is suppressed by default.
    /// </summary>
    /// <remarks>
    /// The column renders no header text, so a chooser row for it would be unlabeled and hiding it would hide
    /// navigation. Markup can still opt back in.
    /// </remarks>
    public TableNavigationColumn()
        => ShowInColumnChooser = false;

    /// <inheritdoc/>
    private protected override string GetColumnId()
         => _id;

    /// <inheritdoc/>
    private protected override RenderFragment GetHeader()
        => _ => { };

    /// <inheritdoc/>
    private protected override RenderFragment<TItem> GetCellContent()
        => item => builder =>
        {
            builder.OpenComponent<TableNavigationColumnBodyCellContent<TItem>>(1);
            {
                builder.AddComponentParameter(2, nameof(TableNavigationColumnBodyCellContent<>.Item), item);
                builder.AddComponentParameter(3, nameof(TableNavigationColumnBodyCellContent<>.Tooltip), Tooltip);
                builder.AddComponentParameter(4, nameof(TableNavigationColumnBodyCellContent<>.Navigate), Navigate);
            }
            builder.CloseComponent();
        };

    /// <inheritdoc/>
    private protected override int? GetDefaultWidth()
        => 56; // 24px navigation icon with 16px of the cell's padding on either side of it
}
