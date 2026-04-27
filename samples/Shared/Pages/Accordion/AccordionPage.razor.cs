using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Accordion.Models;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Pages.Accordion;

public sealed partial class AccordionPage : ComponentBase
{
    private readonly string _settingsIcon = MonochromeIconName.GearSolid.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();
    private readonly string _profileIcon = MonochromeIconName.Edit.GetCssClasses(MonochromeIconSize.SmallMedium).ToSpaceSeparated();

    private string? _expandedItem;
    private string? _expandedIconItem;
    private string? _expandedStyledItem;
    private string? _expandedCancellableItem;
    private bool _blockExpand;
    private string _lastEvent = "(none)";

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

    private void OnBeforeExpandCancellable(AccordionItemCancelEventArgs args)
    {
        if (_blockExpand)
        {
            args.Cancel = true;
            _lastEvent = $"Expand cancelled: {args.Sender.Text}";
        }
        else
        {
            _expandedCancellableItem = args.Sender.Text;
            _lastEvent = $"Expanded: {args.Sender.Text} (cancellable section)";
        }
    }

    private void OnBeforeCollapseCancellable(AccordionItemCancelEventArgs args)
    {
        _expandedCancellableItem = null;
        _lastEvent = $"Collapsed: {args.Sender.Text} (cancellable section)";
    }
}
