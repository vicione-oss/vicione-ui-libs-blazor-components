using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Filters;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components.Filters;

public sealed class SimpleTableContainsColumnFilterEditorTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public SimpleTableContainsColumnFilterEditorTests()
        => _testContext.JSInterop.Mode = JSRuntimeMode.Loose;

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Editor_chrome_renders_in_english_by_default()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        try
        {
            // Act
            var renderedComponent = RenderEditor();

            // Assert
            renderedComponent.Find("input").GetAttribute("placeholder").Should().Be("Filter value...");
            ButtonTexts(renderedComponent).Should().Equal("Apply", "Cancel");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public void Editor_chrome_renders_in_german_for_a_german_culture()
    {
        // Arrange
        var previousCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("de");

        try
        {
            // Act
            var renderedComponent = RenderEditor();

            // Assert
            renderedComponent.Find("input").GetAttribute("placeholder").Should().Be("Filterwert...");
            ButtonTexts(renderedComponent).Should().Equal("Anwenden", "Abbrechen");
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public void Enter_in_the_search_box_applies_the_filter()
    {
        // Arrange
        IColumnFilter? appliedFilter = null;
        var closeRequestCount = 0;
        var renderedComponent = RenderEditor(
            EventCallback.Factory.Create<IColumnFilter?>(this, filter => appliedFilter = filter),
            EventCallback.Factory.Create(this, () => closeRequestCount++));

        // Act
        var input = renderedComponent.Find("input");
        input.Input("Alpha");
        input.KeyUp(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        appliedFilter.Should().BeOfType<SimpleTableContainsColumnFilter<TableTestItem>>();
        appliedFilter.As<SimpleTableContainsColumnFilter<TableTestItem>>().Value.Should().Be("Alpha");
        closeRequestCount.Should().Be(1);
    }

    private static List<string> ButtonTexts(
        IRenderedComponent<SimpleTableContainsColumnFilterEditor<TableTestItem>> renderedComponent)
            => [.. renderedComponent.FindAll(".button-container button")
                .Select(button => button.TextContent.Trim())];

    private IRenderedComponent<SimpleTableContainsColumnFilterEditor<TableTestItem>> RenderEditor(
        EventCallback<IColumnFilter?>? filterChanged = null, EventCallback? closeRequested = null)
            => _testContext.Render<SimpleTableContainsColumnFilterEditor<TableTestItem>>(b => b
                .AddCascadingValue(
                    new ColumnFilterEditorContext("TestValue", null,
                        filterChanged ?? EventCallback<IColumnFilter?>.Empty,
                        closeRequested ?? EventCallback.Empty))
                .Add(p => p.ValueSelector, item => item.TestValue));
}
