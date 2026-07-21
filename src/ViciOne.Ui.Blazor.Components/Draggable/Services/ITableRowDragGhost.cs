using ViciOne.Ui.Blazor.Components.Draggable.Abstractions;

namespace ViciOne.Ui.Blazor.Components.Draggable.Services;

/// <summary>
/// A default drag ghost specific to table rows, fixing the width collapse of a cloned <c>&lt;tr&gt;</c>.
/// </summary>
public interface ITableRowDragGhost : IDragGhost;
