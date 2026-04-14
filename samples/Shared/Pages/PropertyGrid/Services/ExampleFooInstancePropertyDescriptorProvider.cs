using Shared.Pages.PropertyGrid.Models;
using Shared.Pages.SectionRail.Enums;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Exceptions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

namespace Shared.Pages.PropertyGrid.Services;

internal sealed class ExampleFooInstancePropertyDescriptorProvider
    : IPropertyDescriptorProvider<ExamplePropertyGridContext, ExampleFooInstance>
{
    private readonly StringMustNotBeEmptyPropertyValueValidator _stringMustNotBeEmptyPropertyValueValidator = new();

    public IEnumerable<IPropertyDescriptor<ExampleFooInstance>> GetPropertyDescriptors(ExamplePropertyGridContext context)
    {
        yield return new PropertyDescriptor<ExampleFooInstance, string>()
        {
            ConsiderPredicate = (_) => true,
            CanBeSetToNull = false,
            Category = "String properties",
            GetDefaultValue = (_) => "default",
            ResetValue = (instance) => instance.Name = "default",
            Description = "A string property showing how validation can be done for properties.",
            GetValue = (instance) => instance.Name,
            InformationTooltip = "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, " +
                "sed diam nonumy eirmod tempor invidunt ut labore et dolore magna",
            Name = nameof(ExampleFooInstance.Name),
            SetValue = (instance, value) => instance.Name = value,
            ValueValidators = [_stringMustNotBeEmptyPropertyValueValidator]
        };

        yield return new PropertyDescriptor<ExampleFooInstance, string?>()
        {
            Category = "String properties",
            Name = nameof(ExampleFooInstance.Brief),
            GetValue = (instance) => instance.Brief,
            SetValue = (instance, value) => instance.Brief = value,
            ValueValidators = [_stringMustNotBeEmptyPropertyValueValidator]
        };

        yield return new PropertyDescriptor<ExampleFooInstance, string?>()
        {
            ConsiderPredicate = (_) => true,
            Category = "String properties",
            Name = nameof(ExampleFooInstance.Description),
            GetValue = (instance) => instance.Description,
            Enabled = (_) => false
        };

        yield return new PropertyDescriptor<ExampleFooInstance, string>()
        {
            Category = "String properties",
            Name = nameof(ExampleFooInstance.Abstract),
            GetValue = (instance) => instance.Abstract,
            SetValue = (instance, value) => instance.Abstract = value,
            ReadOnly = (_) => true,
            InformationTooltip = "Read-only property"
        };

        var booleanValueResettablePropertyDescriptor = new PropertyDescriptor<ExampleFooInstance, bool>
        {
            Category = "Boolean properties",
            Description = "Resetattable flag of boolean properties",
            DisplayName = "Booleans Resettable",
            Name = nameof(ExampleFooInstance.BooleanValuesResettable),
            GetValue = (instance) => instance.BooleanValuesResettable,
            SetValue = (instance, value) => instance.BooleanValuesResettable = value
        };

        yield return booleanValueResettablePropertyDescriptor;

        var booleanValue1PropertyDescriptor = new PropertyDescriptor<ExampleFooInstance, bool>
        {
            Name = nameof(ExampleFooInstance.BooleanValue),
            Category = "Boolean properties",
            DisplayName = "Boolean",
            Description = "Boolean property",
            GetValue = (instance) => instance.BooleanValue,
            SetValue = (instance, value) =>
            {
                instance.BooleanValue = value;
                instance.BooleanValueReversed = !value;
            },
            GetDefaultValue = (_) => ExampleFooInstance.BooleanValueDefaultValue,
            ResetValue = (instance) =>
            {
                instance.BooleanValue = ExampleFooInstance.BooleanValueDefaultValue;
                instance.BooleanValueReversed = ExampleFooInstance.BooleanValueReversedDefaultValue;
            },
            Resettable = (instance) => instance.BooleanValuesResettable
        };

        var booleanValue2PropertyDescriptor = new PropertyDescriptor<ExampleFooInstance, bool>
        {
            Name = nameof(ExampleFooInstance.BooleanValueReversed),
            Category = "Boolean properties",
            DisplayName = "Boolean reversed",
            Description = "Boolean property reversed",
            GetValue = (instance) => instance.BooleanValueReversed,
            SetValue = (instance, value) =>
            {
                instance.BooleanValueReversed = value;
                instance.BooleanValue = !value;
            },
            GetDefaultValue = (_) => ExampleFooInstance.BooleanValueReversedDefaultValue,
            ResetValue = (instance) =>
            {
                instance.BooleanValueReversed = ExampleFooInstance.BooleanValueReversedDefaultValue;
                instance.BooleanValue = ExampleFooInstance.BooleanValueDefaultValue;
            },
            Resettable = (instance) => instance.BooleanValuesResettable
        };

        booleanValue1PropertyDescriptor.DependsOn = [booleanValueResettablePropertyDescriptor, booleanValue2PropertyDescriptor];
        booleanValue2PropertyDescriptor.DependsOn = [booleanValueResettablePropertyDescriptor, booleanValue1PropertyDescriptor];

        yield return booleanValue1PropertyDescriptor;
        yield return booleanValue2PropertyDescriptor;

        var switchStatePropertyDescriptor = new PropertyDescriptor<ExampleFooInstance, bool?>
        {
            Name = nameof(ExampleFooInstance.NullableBooleanValue),
            Category = "Switch",
            DisplayName = "State",
            Description = "State with indeterminate support",
            GetValue = (instance) => instance.NullableBooleanValue,
            SetValue = (instance, value) => instance.NullableBooleanValue = value
        };

        yield return switchStatePropertyDescriptor;

        yield return new PropertyDescriptor<ExampleFooInstance, string>
        {
            Name = nameof(ExampleFooInstance.NullableBooleanValue),
            Category = "Switch",
            DisplayName = "Switched",
            GetValue = (_) => "Switch is on",
            Visible = (instance) => instance.NullableBooleanValue is true,
            DependsOn = [switchStatePropertyDescriptor]
        };

        yield return new SelectionPropertyDescriptor<ExampleFooInstance, SectionId?>
        {
            Name = nameof(ExampleFooInstance.Section),
            Category = "Possible value properties",
            DisplayName = "Section",
            Description = "Properties with a predefined set of possible values to select the current property value from",
            GetSelectableValues = (_) => Enum.GetValues<SectionId>()
                .Take(2)
                .Select(v => new SelectableValue<SectionId?> { Value = v, Text = v.ToString() })
                .Append(new SelectableValue<SectionId?> { Value = null, Text = "None" }),
            GetValue = (instance) => instance.Section,
            SetValue = (instance, value) => instance.Section = value
        };

        yield return new NumericPropertyDescriptor<ExampleFooInstance, int, int, int>
        {
            Name = nameof(ExampleFooInstance.IntegerValue1),
            Category = "Numeric properties",
            DisplayName = "Integer 1",
            Description = "Integer that is initially zero",
            Minimum = 0,
            Maximum = int.MaxValue,
            Interval = 1,
            GetValue = (instance) => instance.IntegerValue1,
            SetValue = (instance, value) => instance.IntegerValue1 = value
        };

        yield return new NumericPropertyDescriptor<ExampleFooInstance, int, int, int>
        {
            Name = nameof(ExampleFooInstance.IntegerValue2),
            Category = "Numeric properties",
            DisplayName = "Integer 2",
            Description = "Integer that supports increment by 10",
            Minimum = 5,
            Maximum = 200,
            Interval = 10,
            GetValue = (instance) => instance.IntegerValue2,
            SetValue = (instance, value) => instance.IntegerValue2 = value
        };

        yield return new NumericPropertyDescriptor<ExampleFooInstance, byte, byte, byte>
        {
            Name = nameof(ExampleFooInstance.ByteValue),
            Category = "Numeric properties",
            DisplayName = "Rastered byte",
            Description = "Byte that supports increment by 10 and is rastered",
            GetValue = (instance) => instance.ByteValue,
            Minimum = 5,
            Maximum = 200,
            Interval = 5,
            IsRasteredValue = true,
            SetValue = (instance, value) => instance.ByteValue = value
        };

        yield return new NumericPropertyDescriptor<ExampleFooInstance, int?, int, int>
        {
            Name = nameof(ExampleFooInstance.NullableIntegerValue),
            Category = "Numeric properties",
            DisplayName = "Nullable integer",
            Description = "Integer that supports increment by 1 and is nullable",
            GetValue = (instance) => instance.NullableIntegerValue,
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Interval = 1,
            SetValue = (instance, value) => instance.NullableIntegerValue = value,
            CanBeSetToNull = true
        };

        yield return new NumericPropertyDescriptor<ExampleFooInstance, ulong, ulong, ulong>
        {
            Name = nameof(ExampleFooInstance.UnsignedLong),
            Category = "Numeric properties",
            DisplayName = "Unsigned long",
            Description = "Unsigned long throwing exception when value is assigned",
            GetValue = (instance) => instance.UnsignedLong,
            Minimum = ulong.MinValue,
            Maximum = ulong.MaxValue,
            Interval = 1,
            SetValue = (instance, value) =>
            {
                if (value == 10)
                    throw new SetValueException("Lorem ipsum dolores error met", instance, value);

                instance.UnsignedLong = value;
            }
        };

        yield return new NumericPropertyDescriptor<ExampleFooInstance, long, long, long>
        {
            Name = nameof(ExampleFooInstance.SignedLong),
            Category = "Numeric properties",
            DisplayName = "Signed long",
            Description = "Signed long throwing exception when value is assigned",
            GetValue = (instance) => instance.SignedLong,
            Minimum = long.MinValue,
            Maximum = long.MaxValue,
            Interval = 10,
            IsRasteredValue = true,
            SetValue = (instance, value) => instance.SignedLong = value
        };

        yield return new SelectionPropertyDescriptor<ExampleFooInstance, EnumValueName>
        {
            Name = nameof(ExampleFooInstance.AnotherSection),
            Category = "Possible value properties",
            GetSelectableValues = (_) => Enum.GetValues<SectionId>()
                .Select(v => new SelectableValue<EnumValueName> { Value = new(v.ToString()), Text = v.ToString() }),
            GetValue = (instance) => new(instance.AnotherSection.ToString()),
            SetValue = (instance, value) => instance.AnotherSection = Enum.Parse<SectionId>(value.Value)
        };
    }
}
