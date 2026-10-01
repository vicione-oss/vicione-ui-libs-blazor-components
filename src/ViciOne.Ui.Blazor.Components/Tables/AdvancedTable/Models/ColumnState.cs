using ViciOne.Ui.Blazor.Components.Tables.Shared.Enums;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Models;

/// <summary>
/// Manages the state of a table column.
/// Enables targeted updates to individual columns without triggering a full table re-render.
/// </summary>
internal sealed class ColumnState
{
    public bool Visible
    {
        get;

        set
        {
            if (value != field)
            {
                field = value;

                VisibleChanged?.Invoke(this);
            }
        }
    } = true;

    public PinSide PinSide
    {
        get;

        set
        {
            if (value != field)
            {
                field = value;

                PinSideChanged?.Invoke(this);
            }
        }
    }

    public event Action<ColumnState>? VisibleChanged;

    public event Action<ColumnState>? PinSideChanged;

    public event Action? RefreshRequested;

    public void RequestRefresh()
        => RefreshRequested?.Invoke();
}
