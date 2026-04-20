using NSubstitute;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Components;

public sealed partial class PropertyGridTests
{
    private static IPropertyGridItem<int> CreatePropertyGridItem(string displayName, string category = "")
    {
        var store = Substitute.For<IPropertyGridMessageStore>();
        var item = Substitute.For<IPropertyGridItem<int>>();
        item.DisplayName.Returns(displayName);
        item.Category.Returns(category);
        item.ValueType.Returns(typeof(int));
        item.Visible.Returns(true);
        item.MessageStore.Returns(store);
        return item;
    }
}
