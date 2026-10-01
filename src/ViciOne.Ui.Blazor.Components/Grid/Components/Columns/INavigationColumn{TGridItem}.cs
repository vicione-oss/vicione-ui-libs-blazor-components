using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Grid.Components.Columns;

[Obsolete(Constants.ObsoleteMessage)]
internal interface INavigationColumn<TGridItem>
{
    string? NavigationButtonTitle { get; }
    EventCallback<TGridItem> Navigate { get; }
}
