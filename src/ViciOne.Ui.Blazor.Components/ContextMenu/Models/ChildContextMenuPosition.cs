using ViciOne.Ui.Blazor.Components.Attributes;
using ViciOne.Ui.Blazor.Components.ContextMenu.Enums;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Models;

[GenerateTypeScriptImport(Type = "MouseLeaveDirection",
    ModulePath = "/_content/ViciOne.Ui.Blazor.Components/context-menu/enums/mouse-leave-direction.js")]
[GenerateTypeScriptClass]
internal record ChildContextMenuPosition(int X, int Y, MouseLeaveDirection MouseLeaveDirection);
