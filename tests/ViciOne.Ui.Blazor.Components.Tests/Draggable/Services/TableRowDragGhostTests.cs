using AwesomeAssertions;
using ViciOne.Ui.Blazor.Components.Draggable.Services;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.Draggable.Services;

public sealed class TableRowDragGhostTests
{
    [Fact]
    public void Assert_property_values()
    {
        // Arrange
        var dragGhost = new TableRowDragGhost();

        // Act
        var descriptor = dragGhost.GetJsModule();

        // Assert
        descriptor.ModuleName.Should().EndWith("draggable/table-row-drag-ghost.js");
        descriptor.CreateFunction.Name.Should().Be("createDragGhost");
        descriptor.CreateFunction.Args.Should().BeNull();
    }
}
