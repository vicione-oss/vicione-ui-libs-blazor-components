using Bunit;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Columns.TableNavigationColumn;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Footers;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Components.Headers;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Components.Panes;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Extensions;

internal static class ComponentParameterCollectionBuilderExtensions
{
    extension(ComponentParameterCollectionBuilder<AdvancedTable<TableTestItem>> b)
    {
        internal ComponentParameterCollectionBuilder<AdvancedTable<TableTestItem>> AddHeader()
            => b.Add(
                p => p.Header, headerBuilder =>
                {
                    headerBuilder.OpenComponent<TableHeader>(0);
                    headerBuilder.CloseComponent();
                });

        internal ComponentParameterCollectionBuilder<AdvancedTable<TableTestItem>> AddLeftPane()
            => b.Add(
                p => p.LeftOutlet, paneBuilder =>
                {
                    paneBuilder.OpenComponent<LeftPane>(0);
                    paneBuilder.CloseComponent();
                });

        internal ComponentParameterCollectionBuilder<AdvancedTable<TableTestItem>> AddFooter()
            => b.Add(
                p => p.Footer, footerBuilder =>
                {
                    footerBuilder.OpenComponent<TableFooter<TableTestItem>>(0);
                    footerBuilder.CloseComponent();
                });

        internal ComponentParameterCollectionBuilder<AdvancedTable<TableTestItem>> AddColumns()
            => b.Add(
                p => p.Columns, builder =>
                {
                    builder.OpenComponent<AdvancedTableColumn<TableTestItem>>(2);
                    static RenderFragment CellContent(TableTestItem x)
                        => childBuilder => childBuilder.AddContent(0, $"CellContent {x.TestKey}");
                    builder.AddComponentParameter(
                        3,
                        nameof(AdvancedTableColumn<>.CellContent),
                        (RenderFragment<TableTestItem>)CellContent);

                    builder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), Guid.NewGuid().ToString());
                    builder.CloseComponent();

                    builder.OpenComponent<TableNavigationColumn<TableTestItem>>(5);
                    builder.CloseComponent();
                });

        internal ComponentParameterCollectionBuilder<AdvancedTable<TableTestItem>> AddSortableColumn(string id)
            => b.Add(
                p => p.Columns, colBuilder =>
                {
                    colBuilder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                    colBuilder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), id);
                    colBuilder.AddComponentParameter(2, nameof(AdvancedTableColumn<>.Sortable), true);
                    colBuilder.CloseComponent();
                });

        internal ComponentParameterCollectionBuilder<AdvancedTable<TableTestItem>> AddNonSortableColumn(string id)
            => b.Add(
                p => p.Columns, colBuilder =>
                {
                    colBuilder.OpenComponent<AdvancedTableColumn<TableTestItem>>(0);
                    colBuilder.AddComponentParameter(1, nameof(AdvancedTableColumn<>.Id), id);
                    colBuilder.CloseComponent();
                });
    }
}
