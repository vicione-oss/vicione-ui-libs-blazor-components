using AwesomeAssertions;
using Bunit;
using ViciOne.Ui.Blazor.Components.CheckBox.Extensions;
using Xunit;

using BoolCheckBox = ViciOne.Ui.Blazor.Components.CheckBox.CheckBox<bool>;
using NullableBoolCheckBox = ViciOne.Ui.Blazor.Components.CheckBox.CheckBox<bool?>;

namespace ViciOne.Ui.Blazor.Components.Tests.CheckBox;

public sealed class CheckBoxTests : IDisposable
{
    private readonly BunitContext _testContext = new();

    public CheckBoxTests()
        => _testContext.Services.AddCheckBox();

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Should_render_checked_icon()
    {
        // Act
        var renderedComponent = _testContext.Render<BoolCheckBox>(parameters => parameters
            .Add(p => p.Value, true));

        // Assert
        renderedComponent.FindAll(".checked-icon").Should().ContainSingle();
        renderedComponent.FindAll(".indeterminate-icon").Should().BeEmpty();
        renderedComponent.FindAll("svg").Should().BeEmpty();
    }

    [Fact]
    public void Should_render_indeterminate_icon()
    {
        // Act
        var renderedComponent = _testContext.Render<NullableBoolCheckBox>();

        // Assert
        renderedComponent.FindAll(".indeterminate-icon").Should().ContainSingle();
        renderedComponent.FindAll("svg").Should().BeEmpty();
    }

    [Fact]
    public void Should_render_no_indeterminate_icon_when_unchecked()
    {
        // Act
        var renderedComponent = _testContext.Render<BoolCheckBox>();

        // Assert
        renderedComponent.FindAll(".checked-icon").Should().ContainSingle();
        renderedComponent.FindAll(".indeterminate-icon").Should().BeEmpty();
        renderedComponent.FindAll("svg").Should().BeEmpty();
    }
}
