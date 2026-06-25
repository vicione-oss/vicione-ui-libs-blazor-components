using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.PointerCapture.Services;

namespace Shared.Pages.Resizeable.Components;

public sealed partial class ResizeablePage
{
    private const int DefaultGridSize = 20;

    private bool _resizeable;
    private bool _snapToGrid;
    private int _gridSize = DefaultGridSize;
    private bool _initialized;
    private DomRect? _shapeDomRect;

    [Inject] private ISnapToGridPointerCaptureBehavior SnapToGridPointerCaptureBehavior { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await SnapToGridPointerCaptureBehavior.SetGridSizeAsync(_gridSize);

        _initialized = true;
    }

    private async Task GridSizeChangedAsync()
        => await SnapToGridPointerCaptureBehavior.SetGridSizeAsync(_gridSize);
}
