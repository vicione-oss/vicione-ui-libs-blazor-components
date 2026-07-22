namespace Shared.Models;

public sealed class NavigationItemDescriptor
{
    public required string Href { get; init; }
    public required string Text { get; init; }

    public List<NavigationItemDescriptor> Children { get; } = [];
}
