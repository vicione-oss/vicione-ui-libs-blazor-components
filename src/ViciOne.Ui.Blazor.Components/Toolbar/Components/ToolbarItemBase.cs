using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Services;

namespace ViciOne.Ui.Blazor.Components.Toolbar.Components;

/// <summary>
/// Base class for toolbar items, handling basics required for handling resizing logic.
/// </summary>
/// <remarks>
/// Every instance of this must be inside a <see cref="IToolbarItemParent"/>.
/// </remarks>
public abstract class ToolbarItemBase : ComponentBase, IToolbarChild, IAsyncDisposable
{
    private bool _hidden;
    private bool _initialized;
    private bool? _previousVisible;
    private ElementSizeChangedEventArgs? _previousElementSizeChangedEventArgs;

    /// <summary>
    /// This parameter is set only when <see cref="InMenu"/> is <see langword="true" />, otherwise it is <see langword="null" />.
    /// </summary>
    [CascadingParameter]
    private IToolbarItemParent Parent { get; set; } = default!;

    /// <summary>
    /// Whether the item is inside the context menu rather than the primary toolbar.
    /// </summary>
    [CascadingParameter(Name = "InMenu")]
    protected bool InMenu { get; set; }

    /// <summary>
    /// The text additionally rendered into <see href="https://html.spec.whatwg.org/#classes">class</see> attribute.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Callback that is executed when the item is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnClick { get; set; }

    /// <summary>
    /// The text displayed in a tooltip when the user hovers over the item.
    /// </summary>
    [Parameter]
    public string? Tooltip { get; set; }

    /// <summary>
    /// Whether the item can be interacted with. If <c>false</c>, the item is visually marked and the <see cref="OnClick"/> event can not trigger.
    /// </summary>
    [Parameter]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether the item is rendered in the UI.
    /// </summary>
    /// <remarks>
    /// If this is <c>true</c>, the item will take no space.
    /// </remarks>
    [Parameter]
    public bool Visible { get; set; } = true;

    [Inject] private IResizeObserver ResizeObserver { get; init; } = default!;

    /// <summary>
    /// The HTML element reference for the component's outer container.
    /// </summary>
    protected abstract ElementReference Container { get; }

    DomRect? IToolbarChild.DomRect => _previousElementSizeChangedEventArgs?.DomRect;
    CssStyleDeclaration? IToolbarChild.Style => _previousElementSizeChangedEventArgs?.Style;
    IReadOnlyList<IToolbarChild> IToolbarChild.Children => [];

    void IToolbarChild.Refresh()
        => InvokeAsync(StateHasChanged);

    bool IToolbarChild.IsHidden()
        => _hidden;

    void IToolbarChild.SetHidden(bool hidden)
        => _hidden = hidden;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (InMenu)
            return;

        ResizeObserver.ElementSizeChanged += OnElementSizeChanged;

        if (Parent == null)
            throw new InvalidOperationException($"{GetType().FullName} must be placed inside a {typeof(Toolbar).FullName}.");

        Parent.AddChild(this);
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        // Special case where an item may be changed from outside the toolbar,
        // includes handling for case where a single item is in the menu and that item
        // is changed to be invisible, so the menu should no longer be shown.
        if (_previousVisible != null && _previousVisible != Visible)
        {
            Parent?.ChildSizeChanged();

            _previousVisible = Visible;
        }
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || InMenu)
            return;

        await ResizeObserver.ObserveAsync(Container, true);

        _initialized = true;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        ResizeObserver.ElementSizeChanged -= OnElementSizeChanged;
        await ResizeObserver.UnobserveAsync(Container);

        Parent?.RemoveChild(this);

        GC.SuppressFinalize(this);
    }

    private void OnElementSizeChanged(ElementSizeChangedEventArgs args)
    {
        // When element is not rendered, it's size can be smaller -> ignore
        if (_hidden || args.ElementReference.Id != Container.Id)
            return;

        var rect = args.DomRect;
        var style = args.Style;
        if (style is null)
            return;

        // When element gets rendered again, need to suppress false updates
        var noChange = _previousElementSizeChangedEventArgs?.DomRect.Width.NearlyEquals(rect.Width) == true &&
            _previousElementSizeChangedEventArgs.DomRect.Height.NearlyEquals(rect.Height) &&
            _previousElementSizeChangedEventArgs.Style?.NearlyEquals(style) == true;

        if (noChange)
            return;

        _previousElementSizeChangedEventArgs = args;
        Parent?.ChildSizeChanged();
    }

    /// <summary>
    /// Handles keyboard events and triggers the <see cref="OnClick"/> callback when the Enter key is pressed.
    /// </summary>
    protected async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && OnClick.HasDelegate)
            await OnClick.InvokeAsync();
    }

    /// <summary>
    /// Generates the list of CSS classes to be applied to the component based on its current state.
    /// </summary>
    protected IEnumerable<string> GetBaseCssClasses()
    {
        if (!string.IsNullOrWhiteSpace(CssClass))
        {
            foreach (var cssClass in CssClass.Split(" "))
                yield return cssClass;
        }

        if (!Enabled)
            yield return "disabled";

        if (!Visible || _hidden)
            yield return "hidden";

        if (!_initialized && !InMenu)
            yield return "invisible";

        if (InMenu)
            yield return "in-menu";
    }
}
