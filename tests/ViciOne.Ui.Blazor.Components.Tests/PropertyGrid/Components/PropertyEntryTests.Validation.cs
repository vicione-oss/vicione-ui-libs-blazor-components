using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Components;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

namespace ViciOne.Ui.Blazor.Components.Tests.PropertyGrid.Components;

public sealed partial class PropertyEntryTests
{
    public sealed class Validation : IAsyncDisposable
    {
        private static readonly TimeSpan s_waitTimeout = TimeSpan.FromMilliseconds(5000);

        private static readonly IPropertyDescriptor<Foo> s_nameVisiblePropertyDescriptor = new PropertyDescriptor<Foo, bool>
        {
            Name = nameof(Foo.NameVisible),
            GetValue = instance => instance.NameVisible
        };

        private static readonly IPropertyDescriptor<Foo> s_namePropertyDescriptor = new PropertyDescriptor<Foo, string?>
        {
            Name = nameof(Foo.Name),
            GetValue = instance => instance.Name,
            DependsOn = [s_nameVisiblePropertyDescriptor],
            ValueValidators = [new StringMustNotBeEmptyPropertyValueValidator()],
            Visible = instance => instance.NameVisible
        };

        private readonly BunitContext _testContext = new();
        private readonly IPropertyGridController<object> _propertyGridController;
        private readonly IPropertyGridState<object> _propertyGridState;

        public Validation()
        {
            _testContext.JSInterop.Mode = JSRuntimeMode.Loose;

            _testContext.Services.AddPropertyGrid<object>()
                .WithPropertyDescriptorProvider<FooPropertyDescriptorProvider>();

            _propertyGridController = _testContext.Services.GetRequiredService<IPropertyGridController<object>>();
            _propertyGridState = _testContext.Services.GetRequiredService<IPropertyGridState<object>>();
        }

        public ValueTask DisposeAsync()
            => _testContext.DisposeAsync();

        [Fact]
        public async Task Should_not_add_error_message_for_hidden_property_with_invalid_value()
        {
            // Arrange
            await SetInstanceAsync(new Foo { Name = string.Empty, NameVisible = false });
            var namePropertyGridItem = GetPropertyGridItem<string?>(s_namePropertyDescriptor);

            // Act
            RenderPropertyEntry(namePropertyGridItem, groupExpanded: true);

            // Assert
            namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().BeEmpty();
        }

        [Fact]
        public async Task Should_add_error_message_for_visible_property_with_invalid_value()
        {
            // Arrange
            await SetInstanceAsync(new Foo { Name = string.Empty, NameVisible = true });
            var namePropertyGridItem = GetPropertyGridItem<string?>(s_namePropertyDescriptor);

            // Act
            RenderPropertyEntry(namePropertyGridItem, groupExpanded: true);

            // Assert
            namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().ContainSingle()
                .Which.Should().BeOfType<ErrorMessage>();
        }

        [Fact]
        public async Task Should_add_error_message_for_visible_property_with_invalid_value_in_collapsed_group()
        {
            // Arrange
            await SetInstanceAsync(new Foo { Name = string.Empty, NameVisible = true });
            var namePropertyGridItem = GetPropertyGridItem<string?>(s_namePropertyDescriptor);

            // Act
            RenderPropertyEntry(namePropertyGridItem, groupExpanded: false);

            // Assert
            namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().ContainSingle()
                .Which.Should().BeOfType<ErrorMessage>();
        }

        [Fact]
        public async Task Should_add_error_message_when_hidden_property_with_invalid_value_becomes_visible_by_dependency()
        {
            // Arrange
            var instance = new Foo { Name = string.Empty, NameVisible = false };
            await SetInstanceAsync(instance);
            var namePropertyGridItem = GetPropertyGridItem<string?>(s_namePropertyDescriptor);
            var nameVisiblePropertyGridItem = GetPropertyGridItem<bool>(s_nameVisiblePropertyDescriptor);

            RenderPropertyEntry(namePropertyGridItem, groupExpanded: true);
            namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().BeEmpty();

            instance.NameVisible = true;

            // Act
            _propertyGridController.UpdateDependents(nameVisiblePropertyGridItem);

            // Assert
            namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().ContainSingle()
                .Which.Should().BeOfType<ErrorMessage>();
        }

        [Fact]
        public async Task Should_add_error_message_when_hidden_property_with_invalid_value_becomes_visible_by_update_property()
        {
            // Arrange
            var instance = new Foo { Name = string.Empty, NameVisible = false };
            await SetInstanceAsync(instance);
            var namePropertyGridItem = GetPropertyGridItem<string?>(s_namePropertyDescriptor);

            var renderedComponent = RenderPropertyEntry(namePropertyGridItem, groupExpanded: true);
            namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().BeEmpty();

            instance.NameVisible = true;

            // Act
            _propertyGridController.UpdateProperty(nameof(Foo.Name));

            // Assert
            renderedComponent.WaitForAssertion(() =>
                namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().ContainSingle()
                    .Which.Should().BeOfType<ErrorMessage>(), s_waitTimeout);
        }

        [Fact]
        public async Task Should_remove_error_message_when_property_with_invalid_value_becomes_hidden()
        {
            // Arrange
            var instance = new Foo { Name = string.Empty, NameVisible = true };
            await SetInstanceAsync(instance);
            var namePropertyGridItem = GetPropertyGridItem<string?>(s_namePropertyDescriptor);
            var nameVisiblePropertyGridItem = GetPropertyGridItem<bool>(s_nameVisiblePropertyDescriptor);

            RenderPropertyEntry(namePropertyGridItem, groupExpanded: true);
            namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().ContainSingle();

            instance.NameVisible = false;

            // Act
            _propertyGridController.UpdateDependents(nameVisiblePropertyGridItem);

            // Assert
            namePropertyGridItem.MessageStore.Get(namePropertyGridItem).Should().BeEmpty();
        }

        private async Task SetInstanceAsync(Foo instance)
        {
            var propertyGridStateChangedTaskCompletionSource = new TaskCompletionSource<bool>();
            _propertyGridState.PropertiesChanged += _ => propertyGridStateChangedTaskCompletionSource.TrySetResult(true);

            _propertyGridController.SetInstances([instance], new object());

            await propertyGridStateChangedTaskCompletionSource.Task.WaitAsync(s_waitTimeout, CancellationToken.None);
        }

        private IPropertyGridItem<TPropertyValue> GetPropertyGridItem<TPropertyValue>(IPropertyDescriptor propertyDescriptor)
            => (IPropertyGridItem<TPropertyValue>)_propertyGridState.Items
                .Single(item => item.PropertyDescriptors.Contains(propertyDescriptor));

        private IRenderedComponent<PropertyEntry<TPropertyValue>> RenderPropertyEntry<TPropertyValue>(
            IPropertyGridItem<TPropertyValue> propertyGridItem, bool groupExpanded)
                => _testContext.Render<PropertyEntry<TPropertyValue>>(b => b
                    .AddCascadingValue<IPropertyGridController>(_propertyGridController)
                    .Add(p => p.PropertyGridItem, propertyGridItem)
                    .Add(p => p.Visible, groupExpanded));

        private sealed class Foo
        {
            public required string Name { get; set; }

            public required bool NameVisible { get; set; }
        }

        private sealed class FooPropertyDescriptorProvider : IPropertyDescriptorProvider<object, Foo>
        {
            public IEnumerable<IPropertyDescriptor<Foo>> GetPropertyDescriptors(object _)
            {
                yield return s_nameVisiblePropertyDescriptor;
                yield return s_namePropertyDescriptor;
            }
        }
    }
}
