using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace Shared.Pages.Moveable.Components;

public sealed partial class MoveablePage
{
    private const int DefaultGridSize = 20;

    private bool _moveable;
    private bool _withMoveHandle;
    private bool _snapToGrid;
    private double? _x;
    private double? _y;
    private int _gridSize = DefaultGridSize;
    private bool _initialized;

    [Inject] private ISnapToGridPointerCaptureBehavior SnapToGridPointerCaptureBehavior { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await SnapToGridPointerCaptureBehavior.SetGridSizeAsync(_gridSize);

        _initialized = true;
    }

    private async Task GridSizeChangedAsync()
        => await SnapToGridPointerCaptureBehavior.SetGridSizeAsync(_gridSize);
}
