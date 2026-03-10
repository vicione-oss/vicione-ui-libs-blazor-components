using ViciOne.Ui.Blazor.Components.Models;

namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

internal interface IToolbarChild
{
    DomRect? DomRect { get; }
    CssStyleDeclaration? Style { get; }
    IReadOnlyList<IToolbarChild> Children { get; }
    bool IsHidden();
    void SetHidden(bool hidden);
    void Refresh();
}
