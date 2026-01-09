using Microsoft.AspNetCore.Components;
using Shared.Pages.SectionRail.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;

namespace Shared.Pages.SectionRail.Components;

public sealed partial class SectionRailPage : ComponentBase
{
    private readonly string _dataflowIconCssClass =
        MonochromeIconName.DataflowSolid.GetCssClasses().ToSpaceSeparated();

    private readonly string _infoOutlinedIconCssClass =
        MonochromeIconName.InfoOutlined.GetCssClasses().ToSpaceSeparated();

    private SectionId _activeSectionId;
    private bool _expanded;
}
