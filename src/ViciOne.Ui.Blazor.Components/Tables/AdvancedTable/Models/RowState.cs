namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

/// <summary>
/// Manages the state of a <see cref="Components.AdvancedTable{TItem}"/> row.
/// Enables targeted updates to individual rows without triggering a full table re-render.
/// </summary>
internal sealed class RowState
{
    public bool Hovered
    {
        get;

        internal set
        {
            if (value != field)
            {
                field = value;

                HoveredChanged?.Invoke();
            }
        }
    }

    public event Action? HoveredChanged;
}
