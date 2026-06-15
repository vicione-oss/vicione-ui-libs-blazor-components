using ViciOne.Ui.Blazor.Components.Moveable.Components;

namespace Shared.Pages.Moveable.Components;

public interface IMoveableShape
{
    bool Moveable { get; set; }

    internal void RegisterMoveHandle(IMoveHandle moveHandle);
    internal void UnregisterMoveHandle(IMoveHandle moveHandle);
}
