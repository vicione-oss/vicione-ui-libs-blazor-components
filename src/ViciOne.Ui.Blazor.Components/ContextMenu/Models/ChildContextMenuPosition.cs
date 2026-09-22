using ViciOne.Ui.Blazor.Components.Attributes;
using ViciOne.Ui.Blazor.Components.ContextMenu.Enums;

namespace ViciOne.Ui.Blazor.Components.ContextMenu.Models;

[GenerateTypeScriptImport(Type = "MouseLeaveDirection", ModulePath = "../Enums/MouseLeaveDirection.cs.ts")]
[GenerateTypeScriptClass]
internal record ChildContextMenuPosition(int X, int Y, MouseLeaveDirection MouseLeaveDirection);
