using Bogus;
using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Accordion.Components;
using ViciOne.Ui.Blazor.Components.Accordion.Models;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Pages.Accordion;

public sealed partial class AccordionPage : ComponentBase
{
    private static readonly Faker s_faker = new();

    private readonly string _settingsIcon = MonochromeIconName.GearSolid.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();
    private readonly string _profileIcon = MonochromeIconName.Edit.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();

    private string? _expandedItem;
    private string? _expandedIconItem;
    private string? _expandedStyledItem;
    private string? _expandedCancelableItem;
    private bool _blockExpand;
    private string _lastEvent = "(none)";

    private readonly IReadOnlyList<string> _scrollableItemTexts =
        [.. Enumerable.Range(1, 12).Select(index => $"{s_faker.Commerce.Department()} {index}")];
    private readonly string _scrollableItemContent = s_faker.Lorem.Paragraph(3);
    private readonly Dictionary<string, AccordionItem> _scrollableAccordionItems = [];
    private string? _expandedScrollableItem;
    private bool _scrollIntoView = true;
    private bool _scrollIntoViewPending;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_scrollIntoViewPending || _expandedScrollableItem is null)
            return;

        _scrollIntoViewPending = false;

        await _scrollableAccordionItems[_expandedScrollableItem].ScrollIntoViewAsync();
    }

    private void OnBeforeExpand(AccordionItemCancelEventArgs args)
    {
        _expandedItem = args.Sender.Text;
        _lastEvent = $"Expanded: {args.Sender.Text}";
    }

    private void OnBeforeCollapse(AccordionItemCancelEventArgs args)
    {
        _expandedItem = null;
        _lastEvent = $"Collapsed: {args.Sender.Text}";
    }

    private void OnBeforeExpandIcons(AccordionItemCancelEventArgs args)
        => _expandedIconItem = args.Sender.Text;

    private void OnBeforeCollapseIcons(AccordionItemCancelEventArgs _)
        => _expandedIconItem = null;

    private void OnBeforeExpandStyled(AccordionItemCancelEventArgs args)
        => _expandedStyledItem = args.Sender.Text;

    private void OnBeforeCollapseStyled(AccordionItemCancelEventArgs _)
        => _expandedStyledItem = null;

    private void OnBeforeExpandCancelable(AccordionItemCancelEventArgs args)
    {
        if (_blockExpand)
        {
            args.Cancel = true;
            _lastEvent = $"Expand canceled: {args.Sender.Text}";
        }
        else
        {
            _expandedCancelableItem = args.Sender.Text;
            _lastEvent = $"Expanded: {args.Sender.Text} (cancelable section)";
        }
    }

    private void OnBeforeCollapseCancelable(AccordionItemCancelEventArgs args)
    {
        _expandedCancelableItem = null;
        _lastEvent = $"Collapsed: {args.Sender.Text} (cancelable section)";
    }

    private void ScrollableItemBeforeExpand(AccordionItemCancelEventArgs args)
        => _expandedScrollableItem = args.Sender.Text;

    private void ScrollableItemBeforeCollapse(AccordionItemCancelEventArgs _)
        => _expandedScrollableItem = null;

    private void ExpandFirstItemButtonClick()
        => ExpandScrollableItem(_scrollableItemTexts[0]);

    private void ExpandLastItemButtonClick()
        => ExpandScrollableItem(_scrollableItemTexts[^1]);

    private void ExpandScrollableItem(string itemText)
    {
        _expandedScrollableItem = itemText;
        _scrollIntoViewPending = _scrollIntoView;
    }
}
