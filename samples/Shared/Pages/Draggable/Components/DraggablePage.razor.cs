using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.Enums;

namespace Shared.Pages.Draggable.Components;

public sealed partial class DraggablePage : IDisposable
{
    private const int DefaultGridSize = 20;

    private bool _draggable;
    private ModifierKey? _modifierKey;
    private bool _snapToGrid;
    private int _gridSize = DefaultGridSize;

    private readonly IEnumerable<ComboBoxItem<ModifierKey?, string>> _modifierKeyItems =
    [
        new() { Value = null, Text = "None" },
        .. Enum.GetValues<ModifierKey>().Select(k => new ComboBoxItem<ModifierKey?, string> { Value = k, Text = k.ToString() })
    ];

    [Inject] private SampleDropHandler DropHandler { get; set; } = default!;

    protected override void OnInitialized()
        => DropHandler.Changed += StateHasChanged;

    public void Dispose()
        => DropHandler.Changed -= StateHasChanged;
}
