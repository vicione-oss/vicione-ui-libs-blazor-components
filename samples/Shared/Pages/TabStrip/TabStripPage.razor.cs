using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.ComboBox;
using ViciOne.Ui.Blazor.Components.Factories;
using ViciOne.Ui.Blazor.Components.TabStrip.Enums;
using ViciOne.Ui.Blazor.Components.TabStrip.Models;

namespace Shared.Pages.TabStrip;

public sealed partial class TabStripPage : ComponentBase
{
    private TabSize _tabSize = TabSize.Large;

    private readonly List<ComboBoxItem<TabSize, string>> _tabSizeComboBoxItems = [..
        TypeSafeEnumFactory<TabSize>.CreateAll().Select(tabSize => new ComboBoxItem<TabSize, string>
        {
            Value = tabSize,
            Text = tabSize.GetName(),
        })
    ];

    private int _activeTabIndex;

    private int? _lastClickedTabIndex;

    private void OnTabSelected(TabSelectedEventArgs args)
        => _lastClickedTabIndex = args.TabIndex;
}
