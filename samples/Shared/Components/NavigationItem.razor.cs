using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Shared.Components;

public sealed partial class NavigationItem : ComponentBase, IDisposable
{
    private bool _expanded;
    private bool _isCurrent;

    [Parameter, EditorRequired]
    public required string Text { get; set; }

    [Parameter, EditorRequired]
    public required string Href { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        UpdateExpanded();
        UpdateIsCurrent();

        NavigationManager.LocationChanged += OnLocationChanged;
    }

    public void Dispose()
        => NavigationManager.LocationChanged -= OnLocationChanged;

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        UpdateExpanded();
        UpdateIsCurrent();

        InvokeAsync(StateHasChanged);
    }

    private void UpdateExpanded()
    {
        if (ChildContent is null)
            return;

        var path = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath;

        // Match whole path segments, so /foo does not also match /foo-bar.
        if (path == Href || path.StartsWith($"{Href}/", StringComparison.Ordinal))
            _expanded = true;
    }

    private void UpdateIsCurrent()
    {
        var path = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath;

        _isCurrent = path == Href;
    }

    private void ExpandCollapseButtonClick()
        => _expanded = !_expanded;
}
