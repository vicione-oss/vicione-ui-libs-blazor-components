using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using ViciOne.Ui.Blazor.Components.Grid.Components.Columns;
using ViciOne.Ui.Blazor.Components.Grid.Components.Panes;
using ViciOne.Ui.Blazor.Components.Grid.Models;

namespace ViciOne.Ui.Blazor.Components.Grid.Components;

/// <summary>
/// Component for rendering a grid
/// </summary>
/// <typeparam name="TGridItem">The type of data represented by each row in the grid.</typeparam>
[CascadingTypeParameter(nameof(TGridItem))]
public sealed partial class Grid<TGridItem> : ComponentBase
{
    private readonly LeftPaneSectionId _leftPaneSectionId = new();

    /// <summary>
    /// Text rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute
    /// </summary>
    [Parameter] public string? CssClass { get; set; }

    /// <summary>
    /// <see langword="true"/> to enable continuous scrolling through <see cref="Items"/>,
    /// otherwise <see langword="false"/>.
    ///
    /// <para>
    /// When <see langword="true"/>, only those items are fetched and rendered that
    /// should be displayed in the scroll viewport based on the scroll position.
    /// </para>
    /// </summary>
    /// <remarks>
    /// For this to work, a height constraint must be applied to the grid via CSS.
    /// </remarks>
    [Parameter] public bool Virtualize { get; set; }

    /// <summary>
    /// A queryable source of data for the grid.
    /// </summary>
    [Parameter]
    public IQueryable<TGridItem>? Items { get; set; }

    /// <summary>
    /// Renders buttons above the grid for executing general actions associated with the grid.
    /// </summary>
    [Parameter] public RenderFragment? ActionButtons { get; set; }

    /// <summary>
    /// Renders a filter element above the grid for general filtering of grid data.
    /// </summary>
    [Parameter] public RenderFragment? Filter { get; set; }

    /// <summary>
    /// Renders footer below the grid for general information of grid data.
    /// </summary>
    [Parameter] public RenderFragment? Footer { get; set; }

    /// <summary>
    /// Defines the child components of this instance.
    /// </summary>
    /// <remarks>
    /// The grid supports column components like <see cref="PropertyColumn{TGridItem, TProp}"/>
    /// and / or pane components like <see cref="LeftPane"/> to define the child content.
    /// </remarks>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    internal IItemSelectColumn<TGridItem>? ItemSelectColumn { get; set; }
    internal INavigationColumn<TGridItem>? NavigationColumn { get; set; }
}
