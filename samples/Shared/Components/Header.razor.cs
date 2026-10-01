using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Shared.Components;

public sealed partial class Header : ComponentBase, IDisposable
{
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
        => NavigationManager.LocationChanged += LocationChanged;

    public void Dispose()
        => NavigationManager.LocationChanged -= LocationChanged;

    private void LocationChanged(object? sender, LocationChangedEventArgs e)
        => StateHasChanged();

    private string GetHeaderText()
    {
        var path = new Uri(NavigationManager.Uri, UriKind.Absolute).AbsolutePath[1..];
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var lastSegment = segments.Length > 0 ? segments[^1] : path;

        return DashCaseToPascalCase(lastSegment);
    }

    /// <summary>
    /// Converts a dash-case string (e.g. "my-component-name") to PascalCase (e.g. "MyComponentName").
    /// Consecutive dashes are treated as separators and ignored.
    /// </summary>
    /// <returns>PascalCase string or empty string for null/whitespace input</returns>
    private static string DashCaseToPascalCase(string? dashCase)
    {
        if (string.IsNullOrWhiteSpace(dashCase))
            return string.Empty;

        var parts = dashCase.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            // Skip empty parts defensively
            if (string.IsNullOrEmpty(part))
                continue;

            var first = char.ToUpperInvariant(part[0]);
            sb.Append(first);
            if (part.Length > 1)
                sb.Append(part.AsSpan(1));
        }

        return sb.ToString();
    }
}
