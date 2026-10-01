using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Extensions;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components.Columns;

public sealed class SimpleTableTemplateColumnTests : IAsyncDisposable
{
    private readonly BunitContext _testContext = new();

    public SimpleTableTemplateColumnTests()
    {
        _testContext.Services.AddSimpleTable().AddLogging();
        _testContext.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public async ValueTask DisposeAsync()
        => await _testContext.DisposeAsync();

    [Fact]
    public void Shows_column_with_no_items()
    {
        // Arrange
        List<TableTestItem> items = [];

        const string Title = "my title";

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
                {
                    builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                    builder.AddAttribute(1, nameof(SimpleTableTemplateColumn<>.Id), "col");
                    builder.AddAttribute(2, nameof(SimpleTableTemplateColumn<>.Title), Title);
                    builder.AddAttribute(3, nameof(SimpleTableTemplateColumn<>.CellContent),
                        (RenderFragment<TableTestItem>)CellContent);
                    builder.CloseComponent();
                })
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<SimpleTableTemplateColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.Find(".header-content").TextContent.Should().Contain(Title);
    }

    [Fact]
    public void Shows_column_with_items_with_expected_content()
    {
        // Arrange
        var items = new List<TableTestItem>
        {
            new(1, "Content One"),
            new(2, "Content Two"),
            new(3, "Content Three")
        };

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "col");
                builder.AddComponentParameter(2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)CellContent);
                builder.CloseComponent();
            })
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<SimpleTableTemplateColumn<TableTestItem>>().Should().ContainSingle();

        items.ToList().ForEach(i =>
            renderedComponent.FindComponents<CellContainer>().Any(x
                => x.Markup.Contains(i.TestValue, StringComparison.InvariantCulture)).Should().BeTrue());
    }

    [Fact]
    public void Shows_header_template_when_given()
    {
        // Arrange
        List<TableTestItem> items = [];

        const string Title = "Custom Title";

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
                {
                    builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                    builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "col");
                    builder.AddComponentParameter(
                        2, nameof(SimpleTableTemplateColumn<>.HeaderContent),
                        (RenderFragment)(headerBuilder =>
                        {
                            headerBuilder.OpenElement(0, "div");
                            headerBuilder.AddContent(1, Title);
                            headerBuilder.CloseElement();
                        }));

                    builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.CellContent),
                        (RenderFragment<TableTestItem>)CellContent);
                    builder.CloseComponent();
                })
        );

        // Assert
        renderedComponent.Should().NotBeNull();
        renderedComponent.FindComponents<SimpleTableTemplateColumn<TableTestItem>>().Should().ContainSingle();
        renderedComponent.Find(".header-content").TextContent.Should().Contain(Title);
    }

    [Fact]
    public void Sort_comparer_parameter_is_exposed_through_the_sort_contract()
    {
        // Arrange
        var items = new List<TableTestItem>().ToList();

        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var renderedComponent = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, items)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "col");
                builder.AddComponentParameter(2, nameof(SimpleTableTemplateColumn<>.SortComparer), comparer);
                builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)CellContent);
                builder.CloseComponent();
            })
        );

        // Assert — SortComparer implicitly implements IHasSortExpression.SortComparer, the member the
        // provider consumes, so the parameter must surface unchanged
        var column = renderedComponent
            .FindComponents<SimpleTableTemplateColumn<TableTestItem>>()
            .Should().ContainSingle().Subject.Instance;

        column.SortComparer.Should().BeSameAs(comparer);
    }

    private static RenderFragment CellContent(TableTestItem x)
        => childBuilder => childBuilder.AddContent(0, x.TestValue);
}
