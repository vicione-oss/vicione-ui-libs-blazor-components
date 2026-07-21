using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using ViciOne.Ui.Blazor.Components.Enums;

namespace Shared.Pages.Draggable.Components;

public sealed partial class DraggablePage : IDisposable
{
    private const int DefaultGridSize = 20;

    private bool _draggable = true;
    private ModifierKey? _modifierKey;
    private bool _snapToGrid;
    private int _gridSize = DefaultGridSize;

    private TimeTickerDragGhost? _timeTickerDragGhost;

    private readonly IEnumerable<ComboBoxItem<ModifierKey?, string>> _modifierKeyItems =
    [
        new() { Value = null, Text = "None" },
        .. Enum.GetValues<ModifierKey>().Select(k => new ComboBoxItem<ModifierKey?, string> { Value = k, Text = k.ToString() })
    ];

    // Bound via @ref. The reference is only assigned after the first render, at which point Shape B still
    // holds the null it received during that render. Re-render when the reference first arrives so the ghost
    // flows down to Shape B and it re-attaches its drag interaction with the custom ghost.
    private TimeTickerDragGhost? TimeTickerDragGhostRef
    {
        get => _timeTickerDragGhost;
        set
        {
            if (_timeTickerDragGhost == value)
                return;

            _timeTickerDragGhost = value;

            StateHasChanged();
        }
    }

    [Inject] private ITableRowDragGhost TableRowDragGhost { get; set; } = default!;
    [Inject] private SampleDropHandler DropHandler { get; set; } = default!;

    protected override void OnInitialized()
        => DropHandler.Changed += StateHasChanged;

    public void Dispose()
        => DropHandler.Changed -= StateHasChanged;
}
