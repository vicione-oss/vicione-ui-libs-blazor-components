using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components.Filters;

public sealed class FilterEditorFrameTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public FilterEditorFrameTests()
        => _testContext.JSInterop.Mode = JSRuntimeMode.Loose;

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Title_and_body_render_into_their_containers()
    {
        // Act
        var renderedComponent = RenderFrame();

        // Assert
        renderedComponent.Find(".header-container").TextContent.Trim().Should().Be("Contains");
        renderedComponent.Find(".body-container .editor-body").TextContent.Trim().Should().Be("Body");
    }

    [Fact]
    public void Buttons_render_in_english_by_default()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        try
        {
            // Act
            var renderedComponent = RenderFrame();

            // Assert
            ButtonTexts(renderedComponent).Should().Equal("Apply", "Cancel");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public void Buttons_render_in_german_for_a_german_culture()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("de");

        try
        {
            // Act
            var renderedComponent = RenderFrame();

            // Assert
            ButtonTexts(renderedComponent).Should().Equal("Anwenden", "Abbrechen");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task Apply_button_applies()
    {
        // Arrange
        var applied = 0;
        var canceled = 0;
        var renderedComponent = RenderFrame(() => applied++, () => canceled++);

        // Act
        await renderedComponent.FindAll(".button-container button")[0].ClickAsync(new MouseEventArgs());

        // Assert
        applied.Should().Be(1);
        canceled.Should().Be(0);
    }

    [Fact]
    public async Task Cancel_button_cancels()
    {
        // Arrange
        var applied = 0;
        var canceled = 0;
        var renderedComponent = RenderFrame(() => applied++, () => canceled++);

        // Act
        await renderedComponent.FindAll(".button-container button")[1].ClickAsync(new MouseEventArgs());

        // Assert
        canceled.Should().Be(1);
        applied.Should().Be(0);
    }

    [Fact]
    public async Task Keys_pressed_in_the_body_are_not_handled()
    {
        // Arrange
        var renderedComponent = RenderFrame();

        // Act
        var keyDown = () => renderedComponent.Find(".body-container")
            .KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        await keyDown.Should().ThrowAsync<MissingEventHandlerException>();
    }

    private static List<string> ButtonTexts(IRenderedComponent<FilterEditorFrame> renderedComponent)
        => [.. renderedComponent.FindAll(".button-container button").Select(button => button.TextContent.Trim())];

    private IRenderedComponent<FilterEditorFrame> RenderFrame(Action? onApply = null, Action? onCancel = null)
        => _testContext.Render<FilterEditorFrame>(b => b
            .Add(p => p.Title, "Contains")
            .AddChildContent("<div class=\"editor-body\">Body</div>")
            .Add(p => p.OnApply, () => onApply?.Invoke())
            .Add(p => p.OnCancel, () => onCancel?.Invoke()));
}
