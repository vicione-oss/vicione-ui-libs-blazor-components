using System.Linq.Expressions;
using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ViciOne.Ui.Blazor.Components.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.Tests.Enums;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;
using ViciOne.Ui.Blazor.Components.Tooltip.Components;
using Xunit;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Components;

public sealed partial class PropertyEntryTests
{
    [Fact]
    public void Should_render_component()
    {
        // Arrange
        using var testContext = new Bunit.TestContext();
        testContext.Services.AddScoped(_ => Substitute.For<IPropertyEditorComponentRegistry>());

        var propertyGridController = Substitute.For<IPropertyGridController>();
        var propertyGridItem = Substitute.For<IPropertyGridItem<int>>();

        // Act
        var renderedComponent = testContext.RenderComponent<PropertyEntry<int>>(b => b
            .AddCascadingValue(propertyGridController)
            .Add(p => p.PropertyGridItem, propertyGridItem));

        // Assert
        renderedComponent.Should().NotBeNull();
    }

    [Fact]
    public void Should_render_tooltip_and_disabled_selection_editor_because_selectable_values_are_mutually_exclusive()
    {
        // Arrange
        var instance1 = new Foo<AOrB> { Bar = AOrB.A };
        var instance2 = new Foo<AOrB> { Bar = AOrB.B };

        // Act / Assert
        TestTooltipAndSelectionEditorWithTheAssumptionOfSelectableValuesBeingMutuallyExclusive(
            instance => instance.Bar, instance1, instance2);
    }

    [Fact]
    public void Should_render_tooltip_and_disabled_selection_editor_because_nullable_selectable_values_are_mutually_exclusive()
    {
        // Arrange
        var instance1 = new Foo<AOrB?> { Bar = AOrB.A };
        var instance2 = new Foo<AOrB?> { Bar = AOrB.B };

        // Act / Assert
        TestTooltipAndSelectionEditorWithTheAssumptionOfSelectableValuesBeingMutuallyExclusive(
            instance => instance.Bar, instance1, instance2);
    }

    private static void TestTooltipAndSelectionEditorWithTheAssumptionOfSelectableValuesBeingMutuallyExclusive<TPropertyValue>(
        Expression<Func<Foo<TPropertyValue>, TPropertyValue>> propertySelector, params Foo<TPropertyValue>[] instances)
    {
        // Arrange
        using var testContext = new Bunit.TestContext();
        testContext.JSInterop.SetupModule("./_content/ViciOne.Ui.Blazor.Components/tooltip/components/tooltip-display.js");
        testContext.JSInterop.Mode = JSRuntimeMode.Loose;

        testContext.Services.AddScoped(_ =>
            new FooPropertySelectorProvider<TPropertyValue> { PropertySelector = propertySelector });

        testContext.Services.AddPropertyGrid<object>()
            .WithPropertyDescriptorProvider<FooPropertyDescriptorProvider<TPropertyValue>>();

        var propertyGridController = testContext.Services.GetRequiredService<IPropertyGridController<object>>();
        var propertyGridItemCollectionBuilder = testContext.Services.GetRequiredService<IPropertyGridItemCollectionBuilder<object>>();
        var propertyGridMessageStore = testContext.Services.GetRequiredService<IPropertyGridMessageStore<object>>();

        var context = new object();

        var propertyGridItem = propertyGridItemCollectionBuilder.Build(instances, context, propertyGridMessageStore)
            .First();

        // Act
        var renderedComponent = testContext.RenderComponent<TestPage<TPropertyValue>>(b => b
            .Add(p => p.PropertyGridController, propertyGridController)
            .Add(p => p.PropertyGridItem, propertyGridItem));

        var informationIconContainer = renderedComponent.Find(".information-icon-container");
        informationIconContainer.PointerEnter();

        renderedComponent.Render();

        // Assert
        var informationTooltip = renderedComponent.Find(".information-tooltip");

        informationTooltip.TextContent.Should()
            .BeEquivalentTo("The selection is disabled because the selected items have selectable values that are mutually exclusive.");

        var nullablePropertyValueType = typeof(TPropertyValue).MakeNullableType();

        var assertSelectionPropertyEditorDisabledDelegate = AssertSelectionPropertyEditorDisabled<TPropertyValue>;
        var assertSelectionPropertyEditorDisabledMethod = assertSelectionPropertyEditorDisabledDelegate
            .Method.GetGenericMethodDefinition().MakeGenericMethod(nullablePropertyValueType);

        assertSelectionPropertyEditorDisabledMethod.Invoke(null, [renderedComponent]);
    }

    private static void AssertSelectionPropertyEditorDisabled<TPropertyValue>(IRenderedFragment renderedComponent)
    {
        var propertyEditor = renderedComponent.FindComponent<SelectionPropertyEditor<TPropertyValue>>();
        propertyEditor.Instance.Enabled.Should().BeFalse();
        propertyEditor.Instance.NoOptionSelected.Should().BeTrue();
    }

    private sealed class TestPage<TPropertyValue> : ComponentBase
    {
        [Parameter, EditorRequired]
        public IPropertyGridController PropertyGridController { get; set; } = default!;

        [Parameter, EditorRequired]
        public IPropertyGridItem<TPropertyValue> PropertyGridItem { get; set; } = default!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(1, "div");
            {
                builder.OpenComponent<CascadingValue<IPropertyGridController>>(2);
                {
                    builder.AddAttribute(3, "Value", PropertyGridController);
                    builder.AddAttribute(4, "ChildContent", (RenderFragment)(builder2 =>
                    {
                        builder2.OpenComponent<PropertyEntry<TPropertyValue>>(1);
                        {
                            builder2.AddComponentParameter(2, nameof(PropertyEntry<>.PropertyGridItem), PropertyGridItem);
                        }
                        builder2.CloseComponent();
                    }));
                }
                builder.CloseComponent();

                builder.OpenComponent<TooltipDisplay>(5);
                builder.CloseComponent();
            }
            builder.CloseElement();
        }
    }

    private sealed class Foo<TPropertyValue>
    {
        public required TPropertyValue Bar { get; set; }
    }

    private sealed class FooPropertySelectorProvider<TPropertyValue>
    {
        public required Expression<Func<Foo<TPropertyValue>, TPropertyValue>> PropertySelector { get; init; }
    }

    private sealed class FooPropertyDescriptorProvider<TPropertyValue>(
        FooPropertySelectorProvider<TPropertyValue> propertySelectorProvider)
            : IPropertyDescriptorProvider<object, Foo<TPropertyValue>>
    {
        private readonly string _propertyName =
            propertySelectorProvider.PropertySelector.GetPropertyName();

        private readonly Func<Foo<TPropertyValue>, TPropertyValue> _getPropertyValue =
            propertySelectorProvider.PropertySelector.Compile();

        public IEnumerable<IPropertyDescriptor<Foo<TPropertyValue>>> GetPropertyDescriptors(object _)
        {
            yield return new SelectionPropertyDescriptor<Foo<TPropertyValue>, TPropertyValue>
            {
                Name = _propertyName,
                GetValue = instance => _getPropertyValue(instance),
                GetSelectableValues = instance =>
                [
                    new SelectableValue<TPropertyValue>
                    {
                        Value = _getPropertyValue(instance),
                        Text = $"{_getPropertyValue(instance)}"
                    }
                ],
                InformationTooltip = "Lorem ipsum"
            };
        }
    }
}
