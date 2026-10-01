using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

/// <summary>
/// Specifies the selection cardinality of an <see cref="AdvancedTable.Components.AdvancedTable{TItem}"/> — how many items may be
/// selected — governing both the row-click and column selection channels alike. Cardinality only; a channel
/// is turned off or made display-only on the channel itself (<see cref="AdvancedTable.Components.AdvancedTable{TItem}.RowClickSelectionEnabled"/>
/// for the row-click channel, the select column's <c>DisplayOnly</c> for the column channel), not here.
/// </summary>
public readonly record struct SelectionMode : ITypeSafeEnumImplemention<SelectionMode>
{
    /// <summary>Clicking a row selects it and deselects any previously selected row.</summary>
#pragma warning disable CA1720 // Identifier contains type name — Single is the correct domain term for single-item selection mode
    public static readonly SelectionMode Single = new(nameof(Single));
#pragma warning restore CA1720

    /// <summary>
    /// Clicking a row replaces the selection with the clicked item.
    /// Ctrl+click toggles an individual item.
    /// Shift+click selects the contiguous range from the last anchor item to the current one.
    /// </summary>
    public static readonly SelectionMode Multiple = new(nameof(Multiple));

    private readonly string _name;

    internal SelectionMode(string name)
        => _name = name;

    /// <inheritdoc/>
    public static SelectionMode GetDefaultValue()
        => Multiple;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
