using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.MonochromeIcons.Components;
using ViciOne.Ui.MonochromeIcons.Core.Enums;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.Columns;

/// <summary>
/// Represents a <see cref="Grid{TGridItem}"/> column whose cells display a navigation button when row is hovered.
/// </summary>
/// <typeparam name="TGridItem">Grid item type</typeparam>
public sealed class NavigationColumn<TGridItem> : ColumnBase<TGridItem>, INavigationColumn<TGridItem>, IDisposable
{
    [CascadingParameter]
    private new Grid<TGridItem> Grid { get; set; } = default!;

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#attr-title">title</see> attribute of the navigation buttons
    /// </summary>
    [Parameter]
    public string? NavigationButtonTitle { get; set; }

    /// <summary>
    /// Raised when a navigation button has been clicked
    /// </summary>
    [Parameter]
    public EventCallback<TGridItem> Navigate { get; set; }

    /// <inheritdoc/>
    public override GridSort<TGridItem>? SortBy { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Grid?.NavigationColumn = this;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Class?.Length > 0 && Class != "navigate-column" && !Class.Contains(" navigate-column", StringComparison.Ordinal))
            Class += " navigate-column";
        else
            Class = "navigate-column";
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Grid is not null && Grid.NavigationColumn == this)
            Grid.NavigationColumn = null;
    }

    /// <inheritdoc/>
    protected override void CellContent(RenderTreeBuilder builder, TGridItem item)
    {
        builder.OpenElement(1, "button");
        {
            builder.AddAttribute(2, "class", "navigate-button");

            if (!string.IsNullOrWhiteSpace(NavigationButtonTitle))
                builder.AddAttribute(3, "title", NavigationButtonTitle);

            builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, () => NavigateButtonClickAsync(item)));

            builder.OpenComponent<MonochromeIcon>(5);
            {
                builder.AddComponentParameter(6, "Name", MonochromeIconName.ExpanderLightRight);
                builder.AddComponentParameter(7, "Size", MonochromeIconSize.SmallMedium);
            }
            builder.CloseComponent();
        }
        builder.CloseElement();
    }

    private async Task NavigateButtonClickAsync(TGridItem item)
    {
        if (Navigate.HasDelegate)
            await Navigate.InvokeAsync(item);
    }
}
