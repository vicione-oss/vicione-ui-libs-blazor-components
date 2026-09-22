using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Attributes;
using ViciOne.Ui.Blazor.Components.Resizeable.Enums;

namespace ViciOne.Ui.Blazor.Components.Resizeable.Models;

[GenerateTypeScriptImport(Type = "ResizeHandlePosition", ModulePath = "../Enums/ResizeHandlePosition.cs.ts")]
[GenerateTypeScriptClass]
internal sealed class ResizeHandleInfo
{
    public required ElementReference Element { get; init; }
    public required ResizeHandlePosition Position { get; init; }
}
