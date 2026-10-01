using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components.Filters;

// The smallest editor the table tests need: one button that commits a filter for whichever column cascaded the
// context. Built with a render tree rather than markup because the test project does not use the Razor SDK.
internal sealed class TestApplyingColumnFilterEditor : ColumnFilterEditorBase
{
    protected override IColumnFilter? BuildFilter()
        => new TestColumnFilter(Context.ColumnId);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "button");
        {
            builder.AddAttribute(1, "class", "apply-test-filter");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, ApplyAsync));
        }
        builder.CloseElement();
    }
}
