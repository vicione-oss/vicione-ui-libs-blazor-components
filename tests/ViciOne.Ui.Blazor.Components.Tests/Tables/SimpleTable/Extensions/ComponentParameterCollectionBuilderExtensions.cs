using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.SimpleTable.Components.Columns.SimpleTableSelectColumn;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.SimpleTable.Extensions;

internal static class ComponentParameterCollectionBuilderExtensions
{
    extension(ComponentParameterCollectionBuilder<SimpleTable<TableTestItem>> b)
    {
        internal ComponentParameterCollectionBuilder<SimpleTable<TableTestItem>> AddColumns()
            => b.Add(p => p.Columns, builder =>
                {
                    builder.OpenComponent<SimpleTableTemplateColumn<TableTestItem>>(0);
                    builder.AddComponentParameter(1, nameof(SimpleTableTemplateColumn<>.Id), "TestValue");
                    static RenderFragment CellContent(TableTestItem x)
                        => childBuilder => childBuilder.AddContent(0, $"CellContent {x.TestKey}");
                    builder.AddComponentParameter(
                        2,
                        nameof(SimpleTableTemplateColumn<>.CellContent),
                        (RenderFragment<TableTestItem>)CellContent);
                    builder.CloseComponent();

                    builder.OpenComponent<SimpleTableSelectColumn<TableTestItem>>(3);
                    builder.CloseComponent();
                });

        internal ComponentParameterCollectionBuilder<SimpleTable<TableTestItem>> AddVisibleSelectionRecording(
            List<string> raised,
            Action<IReadOnlyList<TableTestItem>> filteredItems,
            Action<IReadOnlyList<TableTestItem>> visibleSelection)
            => b.Add(p => p.FilteredItemsChanged, items =>
                {
                    raised.Add("filtered");
                    filteredItems(items);
                })
                .Add(p => p.VisibleSelectionChanged, items =>
                {
                    raised.Add("visible");
                    visibleSelection(items);
                });

        internal ComponentParameterCollectionBuilder<SimpleTable<TableTestItem>> AddFooter(int? selectedItemCount = null)
            => b.Add(
                p => p.Footer, footerBuilder =>
                {
                    footerBuilder.OpenComponent<TableFooter<TableTestItem>>(0);
                    footerBuilder.AddComponentParameter(1, nameof(TableFooter<>.SelectedItemCount), selectedItemCount);
                    footerBuilder.CloseComponent();
                });
    }
}
