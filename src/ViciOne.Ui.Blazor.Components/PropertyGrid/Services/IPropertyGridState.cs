using ViciOne.Ui.Blazor.Components.Interfaces;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

/// <summary>
/// State of a property grid
/// </summary>
public interface IPropertyGridState : IHasChangeableProperties, IHasUpdateLock
{
    /// <summary>
    /// Items displayed in the property grid.
    /// </summary>
    IReadOnlyCollection<IPropertyGridItem> Items { get; internal set; }

    /// <summary>
    /// Map of dependents for each property grid item.
    /// </summary>
    internal IReadOnlyDictionary<IPropertyGridItem, HashSet<IPropertyGridItem>> ItemDependentsMap { get; set; }

    /// <summary>
    /// Optional comparer for sorting category groups.
    ///
    /// <para>
    /// When <see langword="null"/>, <see cref="IAlphabeticalCategoryComparer"/> is used.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The package comes with several predefined comparers:
    /// <list type="bullet">
    /// <item><see cref="IAlphabeticalCategoryComparer"/></item>
    /// <item><see cref="IReverseAlphabeticalCategoryComparer"/></item>
    /// <item><see cref="IInsertionOrderCategoryComparer"/></item>
    /// </list>
    /// </remarks>
    IComparer<string>? CategoryComparer { get; set; }

    /// <summary>
    /// Set <see langword="true"/> when properties should be grouped by categories
    /// based on <see cref="IPropertyDescriptor.Category"/>, otherwise <see langword="false"/>.
    /// </summary>
    bool GroupByCategory { get; set; }

    /// <summary>
    /// Optional comparer for sorting property entries within the grid or within each category group.
    ///
    /// <para>
    /// When <see langword="null"/>, <see cref="IAlphabeticalPropertyComparer"/> is used.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The package comes with several predefined comparers:
    /// <list type="bullet">
    /// <item><see cref="IAlphabeticalPropertyComparer"/></item>
    /// <item><see cref="IReverseAlphabeticalPropertyComparer"/></item>
    /// <item><see cref="IInsertionOrderPropertyComparer"/></item>
    /// </list>
    /// </remarks>
    IComparer<IPropertyGridItem>? PropertyComparer { get; set; }

    /// <summary>
    /// Set <see langword="true"/> when messages displayed in the property grid should have a
    /// close button and should not hide on focus change, otherwise <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="InfoMessage">Info messages</see> always have a close button as these messages
    /// are not part of validation handling and therefore are not removed in any way after being added,
    /// except when the user clicks the close button.
    /// </remarks>
    bool KeepMessages { get; set; }
}

/// <inheritdoc/>
public interface IPropertyGridState<TContext> : IPropertyGridState;
