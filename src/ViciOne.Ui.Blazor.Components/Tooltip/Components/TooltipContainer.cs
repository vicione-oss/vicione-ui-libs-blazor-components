using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ViciOne.Ui.Blazor.Components.Tooltip.Models;
using ViciOne.Ui.Blazor.Components.Tooltip.Services;

namespace ViciOne.Ui.Blazor.Components.Tooltip.Components;

/// <summary>
/// Provides tooltips to the provided <see cref="ChildContent"/> components.
/// </summary>
public sealed class TooltipContainer : ComponentBase, IDisposable
{
    private TooltipHandler _handler = null!;
    private readonly TooltipInfo _info = new();
#pragma warning disable CA2213 // Disposable fields should be disposed - we cannot know if the service is still used by something else
    private TooltipService _tooltipService = default!;
#pragma warning restore CA2213 // Disposable fields should be disposed

    [CascadingParameter] private TooltipService? CascadedTooltipService { get; set; }

    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <summary>
    /// The content/components to be tooltipped.
    /// </summary>
    [Parameter]
    public RenderFragment<TooltipHandler>? ChildContent { get; set; }

    /// <summary>
    /// A condition that decides if the tooltip will be displayed or not.
    /// </summary>
    [Parameter]
    public Func<bool>? DisplayCondition { get; set; }

    /// <summary>
    /// The content of the tooltip.
    /// </summary>
    [Parameter]
    public RenderFragment? TooltipContent { get; set; }

    /// <inheritdoc/>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
        => builder.AddContent(0, ChildContent, _handler);

    /// <inheritdoc/>
    public void Dispose()
    {
        _info.Disposed = true;
        _tooltipService.RemoveTooltip(_info.Id);
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        var injectedService = ServiceProvider.GetService(typeof(TooltipService));
        if (injectedService is not null)
            _tooltipService = (TooltipService)injectedService;
        else if (CascadedTooltipService is not null)
            _tooltipService = CascadedTooltipService;
        else
            throw new ArgumentNullException(nameof(_tooltipService), $"Either a cascading value or a dependency injected service of type {nameof(TooltipService)} have to be available.");

        _handler = new(_tooltipService, _info);
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (TooltipContent != _info.Content)
            _info.Content = TooltipContent;

        if (DisplayCondition is not null && DisplayCondition != _info.DisplayCondition)
            _info.DisplayCondition = DisplayCondition;
    }
}
