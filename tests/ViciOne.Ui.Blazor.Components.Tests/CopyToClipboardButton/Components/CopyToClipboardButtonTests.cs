using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Enums;
using ViciOne.Ui.Blazor.Components.Factories;
using ViciOne.Ui.Blazor.Components.TestingHelpers.CopyToClipboardButton.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using ViciOne.Ui.Blazor.Components.Tooltip.Enums;
using ViciOne.Ui.Localization.Resources;

using CopyToClipboardButtonComponent = ViciOne.Ui.Blazor.Components.CopyToClipboardButton.Components.CopyToClipboardButton;

namespace ViciOne.Ui.Blazor.Components.Tests.CopyToClipboardButton.Components;

public sealed partial class CopyToClipboardButtonTests
{
    [Fact]
    public void Should_copy_text_provided_on_click()
    {
        // Arrange
        using var testContext = new BunitContext();
        var copy = testContext.JSInterop.SetupForCopyToClipboardButton();

        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text")));

        // Act
        renderedComponent.Find("button").Click();

        // Assert
        copy.Invocations.Should().ContainSingle()
            .Which.Arguments.Should().Equal("text");
    }

    [Fact]
    public async Task Should_not_copy_when_disposed_before_text_to_copy_is_provided()
    {
        // Arrange
        await using var testContext = new BunitContext();
        var copy = testContext.JSInterop.SetupForCopyToClipboardButton();
        var textToCopy = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => textToCopy.Task));

        var click = renderedComponent.Find("button").ClickAsync();

        // Act
        await renderedComponent.Instance.DisposeAsync();
        textToCopy.SetResult("text");
        await click;

        // Assert
        copy.Invocations.Should().BeEmpty();
    }

    [Fact]
    public void Should_create_script_with_element_and_copied_text_after_first_render()
    {
        // Arrange
        using var testContext = new BunitContext();
        testContext.JSInterop.SetupForCopyToClipboardButton();

        // Act
        testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text")));

        // Assert
        var arguments = testContext.JSInterop.Invocations["CopyToClipboardButton"].Should().ContainSingle().Which.Arguments;
        arguments.Should().HaveCount(2);
        arguments[0].Should().BeOfType<ElementReference>();
        arguments[1].Should().BeOfType<string>().Which.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_not_throw_when_script_module_fails_to_load()
    {
        // Arrange
        // Without any setup, strict mode makes every JavaScript call fail, including the import of the module.
        await using var testContext = new BunitContext();

        // Act
        var renderAndClick = async () =>
        {
            var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
                .Add(p => p.TextToCopy, () => Task.FromResult("text")));

            await renderedComponent.Find("button").ClickAsync();
        };

        // Assert
        await renderAndClick.Should().NotThrowAsync();
    }

    [Fact]
    public void Should_create_script_once_for_several_clicks()
    {
        // Arrange
        using var testContext = new BunitContext();
        var copy = testContext.JSInterop.SetupForCopyToClipboardButton();

        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text")));

        // Act
        renderedComponent.Find("button").Click();
        renderedComponent.Find("button").Click();

        // Assert
        testContext.JSInterop.Invocations["CopyToClipboardButton"].Should().ContainSingle();
        copy.Invocations.Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_dispose_script_when_disposed()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.SetupForCopyToClipboardButton();

        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text")));

        // Act
        await renderedComponent.Instance.DisposeAsync();

        // Assert
        testContext.JSInterop.Invocations["dispose"].Should().ContainSingle();
    }

    [Theory]
    [InlineData("Small", "button--small")]
    [InlineData("Medium", "button--medium")]
    [InlineData("Large", "button--large")]
    public void Should_render_size_modifier_css_class_of_button(string sizeName, string expectedCssClass)
    {
        // Arrange
        using var testContext = new BunitContext();
        testContext.JSInterop.SetupForCopyToClipboardButton();
        var size = TypeSafeEnumFactory<CopyToClipboardButtonSize>.Create(sizeName);

        // Act
        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text"))
            .Add(p => p.Size, size));

        // Assert
        renderedComponent.Find("button").ClassList.Should().Contain(expectedCssClass);
    }

    [Fact]
    public void Should_render_medium_size_by_default()
    {
        // Arrange
        using var testContext = new BunitContext();
        testContext.JSInterop.SetupForCopyToClipboardButton();

        // Act
        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text")));

        // Assert
        renderedComponent.Find("button").ClassList.Should().Contain("button--medium");
    }

    [Theory]
    [InlineData("Top", "tooltip--top")]
    [InlineData("Right", "tooltip--right")]
    [InlineData("Bottom", "tooltip--bottom")]
    [InlineData("Left", "tooltip--left")]
    public void Should_render_tooltip_placement_modifier_css_class(string placementName, string expectedCssClass)
    {
        // Arrange
        using var testContext = new BunitContext();
        testContext.JSInterop.SetupForCopyToClipboardButton();
        var placement = TypeSafeEnumFactory<TooltipPlacement>.Create(placementName);

        // Act
        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text"))
            .Add(p => p.TooltipPlacement, placement));

        // Assert
        renderedComponent.Find(".tooltip").ClassList.Should().Contain(expectedCssClass);
    }

    [Fact]
    public void Should_render_disabled_attribute_when_not_enabled()
    {
        // Arrange
        using var testContext = new BunitContext();
        testContext.JSInterop.SetupForCopyToClipboardButton();

        // Act
        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text"))
            .Add(p => p.Enabled, false));

        // Assert
        renderedComponent.Find("button").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Should_render_title_as_title_and_aria_label()
    {
        // Arrange
        using var testContext = new BunitContext();
        testContext.JSInterop.SetupForCopyToClipboardButton();

        // Act
        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text"))
            .Add(p => p.Title, "Copy value"));

        // Assert
        var button = renderedComponent.Find("button");
        button.GetAttribute("title").Should().Be("Copy value");
        button.GetAttribute("aria-label").Should().Be("Copy value");
    }

    [Fact]
    public void Should_render_localized_copy_verb_as_title_and_aria_label_by_default()
    {
        // Arrange
        using var testContext = new BunitContext();
        testContext.JSInterop.SetupForCopyToClipboardButton();

        // Act
        var renderedComponent = testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text")));

        // Assert
        var button = renderedComponent.Find("button");
        button.GetAttribute("title").Should().Be(CommonVocabulary.CopyVerb);
        button.GetAttribute("aria-label").Should().Be(CommonVocabulary.CopyVerb);
    }

    [Fact]
    public async Task Should_dispose_module_without_creating_script_when_disposed_while_module_is_loading()
    {
        // Arrange
        await using var testContext = new BunitContext();
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        var assemblyName = typeof(CopyToClipboardButtonComponent).Assembly.GetName().Name;
        var pendingImport = PendingJSModuleImport.SetUp(testContext,
            $"./_content/{assemblyName}/copy-to-clipboard-button/components/copy-to-clipboard-button.js");

        testContext.Render<CopyToClipboardButtonComponent>(b => b
            .Add(p => p.TextToCopy, () => Task.FromResult("text")));
        await pendingImport.WaitUntilRequestedAsync();

        // Act
        await testContext.DisposeComponentsAsync();
        await pendingImport.CompleteAsync();

        // Assert
        await pendingImport.Module.Received(1).DisposeAsync();
        await pendingImport.Module.DidNotReceive()
            .InvokeConstructorAsync(Arg.Any<string>(), Arg.Any<CancellationToken>(), Arg.Any<object?[]?>());
    }
}
