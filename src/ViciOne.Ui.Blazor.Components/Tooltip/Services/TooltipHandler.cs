using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Tooltip.Models;

namespace ViciOne.Ui.Blazor.Components.Tooltip.Services;

/// <summary>
/// A context to provide necessary functionality to tooltipped components.
/// </summary>
public sealed class TooltipHandler
{
    private readonly TooltipInfo _info;
    private readonly TooltipService _service;

    /// <summary>
    /// Creates a new instance of <see cref="TooltipHandler"/>.
    /// </summary>
    ///
    /// <param name="service">
    /// An instance of <see cref="TooltipService"/> to use.
    /// </param>
    ///
    /// <param name="info">
    /// The <see cref="TooltipInfo"/> to process events for.
    /// </param>
    internal TooltipHandler(TooltipService service, TooltipInfo info)
    {
        _info = info;
        _service = service;
    }

    /// <summary>
    /// Provides functionality for the onpointerenter event.
    /// </summary>
    public void OnPointerEnter()
        => _service.OnPointerEnter(_info);

    /// <summary>
    /// Provides functionality for the onpointerleave event.
    /// </summary>
    public void OnPointerLeave()
        => _service.OnPointerLeave(_info);

    /// <summary>
    /// Provides functionality for the onpointermove event.
    /// </summary>
    ///
    /// <param name="e">
    /// The event arguments of the original pointer event.
    /// </param>
    public void OnPointerMove(MouseEventArgs e)
        => TooltipService.OnPointerMove(_info, e);
}
