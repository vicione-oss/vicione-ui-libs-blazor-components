using Shared.Pages.Tables.Shared.Factories;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.SimpleTable.EmptyData.Components;

public sealed partial class SimpleTableEmptyDataPage
{
    private static readonly List<ExampleTableItem> s_items = ExampleTableItemFactory.CreateMany(20);

    // A stable empty instance: handing the table a fresh list every render would read as a new data source
    // and re-fetch on each one.
    private static readonly List<ExampleTableItem> s_noItems = [];

    private bool _dataProvided = true;

    private List<ExampleTableItem> Items
    {
        get
        {
            if (_dataProvided)
                return s_items;

            return s_noItems;
        }
    }
}
