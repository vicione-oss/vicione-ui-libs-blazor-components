using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Components;

public sealed partial class SimpleTableTests
{
    [Fact]
    public void Template_column_min_width_is_emitted_as_data_min_width()
    {
        // Arrange
        const int MinWidth = 80;

        // Act
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "0");
                builder.AddComponentParameter(
                    2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.MinimumWidth), MinWidth);
                builder.CloseComponent();

                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(4);
                builder.AddComponentParameter(5, nameof(SimpleTableTemplateColumn<>.Id), "1");
                builder.AddComponentParameter(
                    6, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(7, item.TestKey)));
                builder.CloseComponent();
            }));

        // Assert
        rendered.FindAll("col")[0].GetAttribute("data-min-width").Should().Be("80");
    }

    [Fact]
    public void Template_column_flex_marker_survives_the_wrapper()
    {
        // Arrange & Act
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "0");
                builder.AddComponentParameter(
                    2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.CloseComponent();

                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(3);
                builder.AddComponentParameter(4, nameof(SimpleTableTemplateColumn<>.Id), "1");
                builder.AddComponentParameter(
                    5, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(6, item.TestKey)));
                builder.AddComponentParameter(7, nameof(SimpleTableTemplateColumn<>.Width), 200);
                builder.CloseComponent();
            }));

        // Assert
        var cols = rendered.FindAll("col");
        cols[0].HasAttribute("data-flex").Should().BeTrue();
        cols[0].GetAttribute("data-column-id").Should().Be("0");
        cols[1].HasAttribute("data-flex").Should().BeFalse();
    }

    [Fact]
    public void Template_column_resizable_false_has_no_resize_handle()
    {
        // Arrange & Act
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "0");
                builder.AddComponentParameter(
                    2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));

                builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.Resizeable), false);
                builder.CloseComponent();

                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(4);
                builder.AddComponentParameter(5, nameof(SimpleTableTemplateColumn<>.Id), "1");
                builder.AddComponentParameter(
                    6, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(7, item.TestKey)));

                builder.CloseComponent();
            }));

        // Assert
        rendered.FindAll("th")[0].QuerySelector(".resize-handle").Should().BeNull();
    }

    [Fact]
    public void Template_column_renders_handle_on_the_last_column()
    {
        // Arrange & Act
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "0");
                builder.AddComponentParameter(
                    2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.Resizeable), true);
                builder.CloseComponent();

                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(4);
                builder.AddComponentParameter(5, nameof(SimpleTableTemplateColumn<>.Id), "1");
                builder.AddComponentParameter(
                    6, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(7, item.TestKey)));
                builder.AddComponentParameter(8, nameof(SimpleTableTemplateColumn<>.Resizeable), true);
                builder.CloseComponent();
            }));

        // Assert
        rendered.FindAll("th")[1].QuerySelector(".resize-handle").Should().NotBeNull();
    }

    [Fact]
    public void Template_column_width_seed_renders_as_col_width_clamped_to_min()
    {
        // Arrange & Act — Width 30 below MinWidth 50 must render at 50
        var rendered = _testContext.Render<SimpleTable<TableTestItem>>(b => b
            .Add(p => p.Items, [])
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "0");
                builder.AddComponentParameter(
                    2, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(0, item.TestValue)));
                builder.AddComponentParameter(3, nameof(SimpleTableTemplateColumn<>.Width), 30);
                builder.CloseComponent();

                builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(4);
                builder.AddComponentParameter(5, nameof(SimpleTableTemplateColumn<>.Id), "1");
                builder.AddComponentParameter(
                    6, nameof(SimpleTableTemplateColumn<>.CellContent),
                    (RenderFragment<TableTestItem>)(item => b => b.AddContent(7, item.TestKey)));
                builder.AddComponentParameter(8, nameof(SimpleTableTemplateColumn<>.Width), 200);
                builder.CloseComponent();
            }));

        // Assert
        var cols = rendered.FindAll("col");
        cols[0].GetAttribute("style").Should().Be("width: 50px");
        cols[1].GetAttribute("style").Should().Be("width: 200px");
    }
}
