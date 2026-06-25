using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.TabStrip.Enums;

namespace ViciOne.Ui.Blazor.Components.TabStrip.Components;

internal interface ITabStrip
{
    int ActiveTabIndex { get; }
    TabSize TabSize { get; }

    void AddTab(ITab tab);
    void RemoveTab(ITab tab);
    int GetTabIndex(ITab tab);
    void RegisterTabElement(ITab tab, ElementReference elementReference);
    Task ChangeActiveTabIndexAsync(int previousIndex, int newIndex);
}
