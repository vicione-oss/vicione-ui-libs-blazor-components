using Microsoft.AspNetCore.Components;
using Shared.Pages.Toolbar.Models;

namespace Shared.Pages.Toolbar.Components;

public sealed partial class NumberToolbarGroup : ComponentBase, IDisposable
{
    private readonly List<int> _values = [.. Enumerable.Range(1, 10)];

    [Parameter, EditorRequired]
    public required SampleNumber Number { get; set; }

    public void Dispose()
        => Number.Changed -= OnNumberChanged;

    protected override void OnInitialized()
        => Number.Changed += OnNumberChanged;

    private void Decrement()
        => Number.SetValue(Math.Max(Number.Value - 1, _values[0]));

    private void Increment()
        => Number.SetValue(Math.Min(Number.Value + 1, _values[^1]));

    private void OnNumberChanged()
        => InvokeAsync(StateHasChanged);
}
