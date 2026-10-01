using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.Filters;
using ViciOne.Ui.Blazor.Components.Tables.Shared.Models;
using ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Models;

namespace ViciOne.Ui.Blazor.Components.Tests.Tables.AdvancedTable.Components.Filters;

// A two-value editor over the same base as the shipped text editors: a lower and an upper bound, both held here
// and neither of them "the" value. Built with a render tree rather than markup because the test project does not
// use the Razor SDK.
internal sealed class TestNumericRangeColumnFilterEditor : ColumnFilterEditorBase
{
    private int? _from;
    private int? _to;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Context.ColumnFilter is TestNumericRangeColumnFilter rangeFilter)
        {
            _from = rangeFilter.From;
            _to = rangeFilter.To;
        }
    }

    protected override IColumnFilter? BuildFilter()
    {
        if (_from is null && _to is null)
            return null;

        return new TestNumericRangeColumnFilter(Context.ColumnId, _from, _to);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "input");
        {
            builder.AddAttribute(1, "type", "number");
            builder.AddAttribute(2, "class", "range-from");
            builder.AddAttribute(3, "value", _from);
            builder.AddAttribute(4, "onchange",
                EventCallback.Factory.CreateBinder<int?>(this, value => _from = value, _from));
        }
        builder.CloseElement();

        builder.OpenElement(5, "input");
        {
            builder.AddAttribute(6, "type", "number");
            builder.AddAttribute(7, "class", "range-to");
            builder.AddAttribute(8, "value", _to);
            builder.AddAttribute(9, "onchange",
                EventCallback.Factory.CreateBinder<int?>(this, value => _to = value, _to));
        }
        builder.CloseElement();

        builder.OpenElement(10, "button");
        {
            builder.AddAttribute(11, "class", "apply");
            builder.AddAttribute(12, "onclick", EventCallback.Factory.Create(this, ApplyAsync));
        }
        builder.CloseElement();

        builder.OpenElement(13, "button");
        {
            builder.AddAttribute(14, "class", "cancel");
            builder.AddAttribute(15, "onclick", EventCallback.Factory.Create(this, CancelAsync));
        }
        builder.CloseElement();
    }
}
