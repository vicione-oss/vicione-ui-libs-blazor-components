using Microsoft.AspNetCore.Components;
using ViciOne.Ui.MonochromeIcons.Core.Enums;
using ViciOne.Ui.MonochromeIcons.Core.Extensions;
using SearchBoxComponent = ViciOne.Ui.Blazor.Components.SearchBox.SearchBox;

namespace Shared.Pages.SearchBox;

public sealed partial class SearchBoxPage : ComponentBase
{
    private string? _demo1SearchTerm;
    private string _demo2SearchTerm = string.Empty;

    private readonly MarkupString _loremText1 = new(@"
        Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt
        ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo
        dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit
        amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor
        invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et
        justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum
        dolor sit amet.
    ");

    private readonly MarkupString _loremText2 = new(@"
        Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut
        aliquip ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit
        esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et
        iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi.
    ");

    private readonly string _customIconCssClass =
        MonochromeIconName.FilterLight.GetCssClasses(SearchBoxComponent.IconSize).ToSpaceSeparated();
}
