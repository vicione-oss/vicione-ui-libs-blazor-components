namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

internal interface IToolbarItemParent
{
    IReadOnlyList<IToolbarChild> Children { get; }

    void AddChild(IToolbarChild child);
    void RemoveChild(IToolbarChild child);
    void ChildSizeChanged();
    void MenuChildrenChanged();
}
