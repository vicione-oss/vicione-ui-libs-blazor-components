using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Services;

/// <summary>
/// The table's connection to its JavaScript module: imports the module, attaches it to the rendered table
/// element and carries the calls that keep the two sides in step.
/// </summary>
/// <remarks>
/// <para>
/// Cascaded to the components rendered inside the table, so a column's filter panel can position itself
/// through the same module without the table exposing an interop surface of its own. That is why the
/// interface carries no type parameter: the filter panel is not generic and could not name one.
/// </para>
/// <para>
/// Every call is a no-op before <see cref="AttachAsync"/> has completed and after disposal, so callers do not
/// have to track which of the two they are in.
/// </para>
/// </remarks>
internal interface IAdvancedTableJsSession : IAsyncDisposable
{
    /// <summary>
    /// Whether the module is attached and therefore whether the calls below reach it.
    /// </summary>
    bool IsAttached { get; }

    /// <summary>
    /// Returns the table's JavaScript module.
    /// </summary>
    /// <remarks>
    /// The session owns and disposes it.
    /// </remarks>
    Task<IJSObjectReference> GetModuleAsync();

    /// <summary>
    /// Attaches the module to <paramref name="tableElement"/>, handing JavaScript the component the session was
    /// built for as the object its callbacks invoke. Calling it again once attached does nothing.
    /// </summary>
    Task AttachAsync(ElementReference tableElement);

    /// <summary>
    /// Recomputes the CSS custom properties that place the pinned columns.
    /// </summary>
    Task UpdatePinnedOffsetsAsync();

    /// <summary>
    /// Moves the keyboard position back to the default cell, because the rows it named have been replaced.
    /// </summary>
    Task ResetFocusedCellAsync();

    /// <summary>
    /// Moves focus into the content of the cell the keyboard sits on.
    /// </summary>
    Task EnterFocusedCellAsync();

    /// <summary>
    /// Reports that the set of rendered columns changed, so the module re-measures them.
    /// </summary>
    Task ColumnsChangedAsync();
}
