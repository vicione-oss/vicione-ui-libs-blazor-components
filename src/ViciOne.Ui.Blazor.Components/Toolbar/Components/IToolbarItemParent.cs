namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

internal interface IToolbarItemParent
{
    void AddChild(IToolbarChild child);
    void RemoveChild(IToolbarChild child);
    void ChildSizeChanged();
}
