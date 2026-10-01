using ViciOne.Ui.Blazor.Components.Tables.Shared.Services;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Services;

internal sealed class TestDragPayloadProvider(
    Func<TableTestItem, IReadOnlyList<TableTestItem>, IReadOnlyList<TableTestItem>> getPayload)
        : ITableDragPayloadProvider<TableTestItem>
{
    public IReadOnlyList<TableTestItem> GetPayload(TableTestItem draggedRow,
        IReadOnlyList<TableTestItem> resolvedPayload)
            => getPayload(draggedRow, resolvedPayload);
}
