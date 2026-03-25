using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using ViciOne.Ui.Blazor.Components.Toolbar.Components;

namespace ViciOne.Ui.Blazor.Components.TestingHelpers.Toolbar.Components;

/// <summary>
/// Tests for types of <typeparamref name="T"/> which inherit from <see cref="ToolbarItemBase"/>
/// </summary>
internal sealed class ToolbarItemTests<T>
    where T : ToolbarItemBase, new()
{
    private readonly List<ElementReference> _elementReferences = [];

    /// <summary>
    /// Asserts that the value passed via parameter <see cref="ToolbarItemBase.CssClass"/> is rendered into the class attribute
    /// of the most outer HTML element of the component.
    /// </summary>
    public void ShouldRenderCssClass(string cssClass)
    {
        // Arrange
        using var testContext = new BunitContext();
        SetupTestContext(testContext);

        var toolbarItemParent = Substitute.For<IToolbarItemParent>();

        // Act
        var renderedComponent = testContext.Render<T>(b => b
            .AddCascadingValue(toolbarItemParent)
            .Add(x => x.CssClass, cssClass));

        var toolbarItem = renderedComponent.Find($".{cssClass}:first-child");

        // Assert
        toolbarItem.Should().NotBeNull();
    }

    private void SetupTestContext(BunitContext testContext)
    {
        var resizeObserver = Substitute.For<IResizeObserver>();

        testContext.Services.AddScoped(_ => resizeObserver);

        resizeObserver.ObserveAsync(Arg.Do<ElementReference>(_elementReferences.Add), Arg.Any<bool>());
    }
}
