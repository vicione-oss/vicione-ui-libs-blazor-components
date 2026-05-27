using Microsoft.AspNetCore.Components;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Services;

namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

/// <summary>
/// Allows to group instances of <see cref="IToolbarChild"/> together.
/// </summary>
/// <remarks>
/// Every instance of this must be inside a <see cref="IToolbarItemParent"/>.
/// </remarks>
public sealed partial class ToolbarGroup : ComponentBase, IToolbarItemParent, IToolbarChild, IAsyncDisposable
{
    private readonly List<IToolbarChild> _children = [];
    private ElementReference _container;

    private bool _hidden;
    private bool _initialized;
    private bool _withSeparator;

    // needs to be a distinct variable from AlignRight since on rerender, the value will get overwritten
    private bool _suppressAlignRight;

    private ElementSizeChangedEventArgs? _previousElementSizeChangedEventArgs;

    [CascadingParameter] private IToolbarItemParent Parent { get; set; } = default!;
    [CascadingParameter(Name = "InMenu")] private bool InMenu { get; set; }

    /// <summary>
    /// If set to <c>true</c>, the group will be pushed to the far right side of the available space.
    /// </summary>
    /// <remarks>
    /// When multiple groups within a toolbar have this setting, it is only applied to the first group.
    /// Starting from this first group, everything will be right aligned.
    /// </remarks>
    [Parameter]
    public bool AlignRight { get; set; }

    /// <summary>
    /// Content to be rendered inside the group.
    /// </summary>
    /// <remarks>
    /// Use component <see cref="ToolbarButton"/> and / or
    /// <see cref="ToolbarContent"/> to render well-defined sections of content
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Inject] private IResizeObserver ResizeObserver { get; init; } = default!;

    DomRect? IToolbarChild.DomRect => _previousElementSizeChangedEventArgs?.DomRect;
    CssStyleDeclaration? IToolbarChild.Style => _previousElementSizeChangedEventArgs?.Style;
    IReadOnlyList<IToolbarChild> IToolbarChild.Children => _children;
    IReadOnlyList<IToolbarChild> IToolbarItemParent.Children => _children;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (InMenu)
            return;

        ResizeObserver.ElementSizeChanged += OnElementSizeChanged;

        if (Parent is null)
            throw new InvalidOperationException($"{GetType().FullName} must be placed inside a {typeof(Toolbar).FullName}.");

        _withSeparator = Parent.Children.Count > 0;

        Parent.AddChild(this);
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await ResizeObserver.ObserveAsync(_container, true);

        _initialized = true;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        ResizeObserver.ElementSizeChanged -= OnElementSizeChanged;
        await ResizeObserver.UnobserveAsync(_container);

        Parent?.RemoveChild(this);

        GC.SuppressFinalize(this);
    }

    private void OnElementSizeChanged(ElementSizeChangedEventArgs args)
    {
        // when element is not rendered, its size can be smaller -> ignore
        if (_hidden || args.ElementReference.Id != _container.Id)
            return;

        var style = args.Style;
        if (style is null)
            return;

        // when element gets rendered again, need to suppress false updates
        // Note: height & width are missing on purpose for groups
        var noChange = _previousElementSizeChangedEventArgs?.Style?.NearlyEquals(style) == true;
        if (noChange)
            return;

        _previousElementSizeChangedEventArgs = args;
        Parent.ChildSizeChanged();
    }

    internal void SuppressAlignRight()
        => _suppressAlignRight = true;

    void IToolbarItemParent.AddChild(IToolbarChild child)
    {
        if (_children.Any(x => x == child))
            return;

        _children.Add(child);
    }

    void IToolbarItemParent.RemoveChild(IToolbarChild child)
        => _children.Remove(child);

    void IToolbarChild.Refresh()
        => InvokeAsync(StateHasChanged);

    void IToolbarItemParent.ChildSizeChanged()
        => Parent.ChildSizeChanged();

    bool IToolbarChild.IsHidden()
        => _hidden;

    void IToolbarChild.SetHidden(bool hidden)
        => _hidden = hidden;
}
