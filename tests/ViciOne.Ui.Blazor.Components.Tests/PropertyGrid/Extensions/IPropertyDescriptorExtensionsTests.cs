namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Extensions;

public sealed partial class IPropertyDescriptorExtensionsTests
{
    private enum Direction { North, South, SouthEast };

    private sealed class Foo
    {
        public string Name { get; set; } = "Unnamed";
        public string? Description { get; set; }
        public int Priority { get; set; } = 1;
        public Direction Direction { get; set; } = Direction.South;
    }
}
