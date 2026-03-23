using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Models;
using ViciOne.Ui.Blazor.Components.Resizing.Services;
using ViciOne.Ui.Blazor.Components.Toolbar.Components;
using Xunit;
using TestContext = Bunit.TestContext;

namespace ViciOne.Ui.Blazor.Components.Tests.Toolbar.Components;

public sealed class ToolbarGroupTests : IDisposable
{
    private readonly IResizeObserver _resizeObserver;
    private readonly TestContext _testContext;

    private readonly List<ElementReference> _elementReferences = [];

    public ToolbarGroupTests()
    {
        _resizeObserver = Substitute.For<IResizeObserver>();

        _testContext = new TestContext();
        _testContext.Services.AddScoped(_ => _resizeObserver);

        _resizeObserver.ObserveAsync(Arg.Do<ElementReference>(_elementReferences.Add), Arg.Any<bool>());
    }

    public void Dispose()
        => _testContext.Dispose();

    [Fact]
    public void Should_register_with_parent_on_startup()
    {
        // Arrange
        IToolbarChild? child = null;
        void HandleAddChild(IToolbarChild c) => child = c;

        // Act
        var renderedComponent = _testContext.RenderComponent<ToolbarGroupParent>(b => b
            .Add(x => x.HandleAddChild, HandleAddChild)
            .AddChildContent<ToolbarGroup>());

        renderedComponent.WaitForState(() => child != null);

        // Assert
        child.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_unregister_with_parent_on_dispose()
    {
        // Arrange
        IToolbarChild? child = null;
        IToolbarChild? removedChild = null;
        void HandleAddChild(IToolbarChild c) => child = c;
        void HandleRemoveChild(IToolbarChild c) => removedChild = c;

        // Act
        var renderedComponent = _testContext.RenderComponent<ToolbarGroupParent>(b => b
            .Add(x => x.HandleAddChild, HandleAddChild)
            .Add(x => x.HandleRemoveChild, HandleRemoveChild)
            .AddChildContent<ToolbarGroup>());

        renderedComponent.WaitForState(() => child != null);

        if (child is ToolbarGroup group)
            await group.DisposeAsync();

        renderedComponent.WaitForState(() => removedChild != null);

        // Assert
        removedChild.Should().NotBeNull();
    }

    [Fact]
    public void Should_notify_parent_on_resize_if_not_hidden()
    {
        // Arrange
        var childSizeChangedCount = 0;
        void HandleChildChanged() => childSizeChangedCount++;

        // Act
        var renderedComponent = _testContext.RenderComponent<ToolbarGroupParent>(b => b
            .Add(x => x.HandleChildChanged, HandleChildChanged)
            .AddChildContent<ToolbarGroup>());

        SetContainerSize(_elementReferences[0], marginX: 10);
        renderedComponent.WaitForState(() => childSizeChangedCount == 1);

        SetContainerSize(_elementReferences[0], marginX: 11);
        renderedComponent.WaitForState(() => childSizeChangedCount == 2);

        // Assert
        childSizeChangedCount.Should().Be(2);
    }

    [Fact]
    public async Task Should_not_notify_parent_on_resize_if_hidden()
    {
        // Arrange
        IToolbarChild? child = null;
        void HandleAddChild(IToolbarChild c) => child = c;

        var childSizeChangedCount = 0;
        void HandleChildChanged() => childSizeChangedCount++;

        // Act
        var renderedComponent = _testContext.RenderComponent<ToolbarGroupParent>(b => b
            .Add(x => x.HandleAddChild, HandleAddChild)
            .Add(x => x.HandleChildChanged, HandleChildChanged)
            .AddChildContent<ToolbarGroup>());

        SetContainerSize(_elementReferences[0], marginX: 10);
        renderedComponent.WaitForState(() => child != null);

        child!.SetHidden(true);

        SetContainerSize(_elementReferences[0], marginX: 11);
        await Task.Delay(50, CancellationToken.None);

        // Assert
        childSizeChangedCount.Should().Be(1);
    }

    [Fact]
    public async Task Should_not_notify_parent_on_resize_to_same_size()
    {
        // Arrange
        var childSizeChangedCount = 0;
        void HandleChildChanged() => childSizeChangedCount++;

        // Act
        var renderedComponent = _testContext.RenderComponent<ToolbarGroupParent>(b => b
            .Add(x => x.HandleChildChanged, HandleChildChanged)
            .AddChildContent<ToolbarGroup>());

        SetContainerSize(_elementReferences[0], marginX: 10);
        renderedComponent.WaitForState(() => childSizeChangedCount == 1);

        SetContainerSize(_elementReferences[0], marginX: 10);
        await Task.Delay(50, CancellationToken.None);

        // Assert
        childSizeChangedCount.Should().Be(1);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void Should_show_separator_only_when_group_is_not_only_child(int groupCount, bool alignRight)
    {
        // Arrange & Act
        var renderedComponent = _testContext.RenderComponent<ToolbarGroupParent>(b => b
            .AddChildContent(builder =>
            {
                var sequence = 1;

                for (var i = 0; i < groupCount; i++)
                {
                    builder.OpenComponent<ToolbarGroup>(sequence++);
                    builder.AddComponentParameter(sequence++, nameof(ToolbarGroup.AlignRight), alignRight);
                    builder.CloseComponent();
                }
            }));

        // Assert
        var groups = renderedComponent.FindAll(".toolbar-group");
        groups.Should().HaveCount(groupCount);

        groups[0].ClassList.Should().NotContain("with-separator");

        for (var i = 1; i < groupCount; i++)
            groups[i].ClassList.Should().Contain("with-separator");
    }

    private void SetContainerSize(ElementReference elementReference, int width = 0, int paddingX = 0, int marginX = 0, int borderX = 0)
        => _resizeObserver.ElementSizeChanged += Raise.Event<Action<ElementSizeChangedEventArgs>>(
            new ElementSizeChangedEventArgs
            {
                ElementReference = elementReference,
                DomRect = new DomRect { Width = width, Height = 50 },
                Style = new CssStyleDeclaration
                {
                    BorderLeft = borderX,
                    BorderRight = borderX,
                    MarginLeft = marginX,
                    MarginRight = marginX,
                    PaddingLeft = paddingX,
                    PaddingRight = paddingX
                }
            }
        );

    private sealed class ToolbarGroupParent : ComponentBase, IToolbarItemParent
    {
        private readonly List<IToolbarChild> _children = [];

        [Parameter]
        public Action<IToolbarChild>? HandleAddChild { get; set; }

        [Parameter]
        public Action<IToolbarChild>? HandleRemoveChild { get; set; }

        [Parameter]
        public Action? HandleChildChanged { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        public IReadOnlyList<IToolbarChild> Children => _children;

        public void AddChild(IToolbarChild child)
        {
            _children.Add(child);

            HandleAddChild?.Invoke(child);
        }

        public void RemoveChild(IToolbarChild child)
        {
            _children.Remove(child);

            HandleRemoveChild?.Invoke(child);
        }

        public void ChildSizeChanged()
            => HandleChildChanged?.Invoke();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingValue<IToolbarItemParent>>(0);
            builder.AddAttribute(2, "Value", this);

            if (ChildContent != null)
            {
                builder.AddAttribute(3, "ChildContent", (RenderFragment)(childBuilder =>
                    childBuilder.AddContent(4, ChildContent)));
            }

            builder.CloseComponent();
        }
    }
}
