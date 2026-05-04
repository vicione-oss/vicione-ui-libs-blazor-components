using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace Shared.Pages.Moveable.Components;

public sealed partial class MoveablePage
{
    private const int DefaultGridSize = 20;

    private bool _moveable;
    private bool _withMoveHandle;
    private bool _snapToGrid;
    private double? _x = DefaultGridSize * 2;
    private double? _y = DefaultGridSize * 3;
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
