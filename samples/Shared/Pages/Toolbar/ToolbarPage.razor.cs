using Microsoft.AspNetCore.Components;
using Shared.Pages.ComboBox.Models;
using Shared.Pages.Toolbar.Models;

namespace Shared.Pages.Toolbar;

public sealed partial class ToolbarPage : ComponentBase
{
    private bool _visible = true;
    private bool _active = true;

    private readonly List<SampleObject> _sampleObjects = [..
        Enumerable.Range(1, 10).Select(i => new SampleObject
        {
            Name = $"Sample Object {i}",
            Value = i
        })
    ];

    private readonly SampleNumber _number = new();

    private void ToggleActive()
        => _active = !_active;

    private void ToggleVisible()
        => _visible = !_visible;
}
