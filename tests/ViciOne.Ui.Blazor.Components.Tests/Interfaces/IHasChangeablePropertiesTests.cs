using System.Linq.Expressions;
using ViciOne.Ui.Blazor.Components.Interfaces;
using ViciOne.Ui.Blazor.Components.Tests.Extensions;

namespace ViciOne.Ui.Blazor.Components.Tests.Interfaces;

/// <summary>
/// Tests for types of <typeparamref name="T"/> which implement <see cref="IHasChangeableProperties"/>
/// </summary>
public class IHasChangeablePropertiesTests<T>
    where T : IHasChangeableProperties, new()
{
    public void AssertChangedEventHandlingWhenPropertyIsSet<TProperty>(Expression<Func<T, TProperty>> propertySelector,
        TProperty initialValue, TProperty value, bool shouldTriggerChangedEvent)
    {
        // Arrange
        var propertyInfo = propertySelector.GetPropertyInfo();
        var propertyName = propertyInfo.Name;

        var propertySetter = propertyInfo.GetSetMethod() ??
            throw new ArgumentException("Property must have a set accessor", nameof(propertySelector));

        var changedTriggered = false;

        var state = new T();
        propertySetter.Invoke(state, [initialValue]);

        state.PropertiesChanged += args => changedTriggered = args.PropertyNames.Contains(propertyName);

        // Act
        propertySetter.Invoke(state, [value]);

        // Assert
        changedTriggered.Should().Be(shouldTriggerChangedEvent);
    }
}
