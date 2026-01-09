using Shared.Pages.PropertyGrid.Models;
using Shared.Pages.SectionRail.Enums;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Validators;

namespace Shared.Pages.PropertyGrid.Services;

internal sealed class ExampleBarInstancePropertyDescriptorProvider
    : IPropertyDescriptorProvider<ExamplePropertyGridContext, ExampleBarInstance>
{
    private readonly StringMustNotBeEmptyPropertyValueValidator _stringMustNotBeEmptyPropertyValueValidator = new();
    private readonly StringMustNotContainDigitPropertyValueValidator _stringMustNotContainDigitPropertyValueValidator = new();

    public IEnumerable<IPropertyDescriptor<ExampleBarInstance>> GetPropertyDescriptors(ExamplePropertyGridContext context)
    {
        yield return new PropertyDescriptor<ExampleBarInstance, string>()
        {
            ConsiderPredicate = (_) => true,
            Name = nameof(ExampleBarInstance.Name),
            Category = "String properties",
            DisplayName = "Name",
            GetValue = (instance) => instance.Name,
            SetValue = (instance, value) => instance.Name = value
        };

        yield return new PropertyDescriptor<ExampleBarInstance, string>()
        {
            Name = nameof(ExampleBarInstance.Brief),
            Category = "String properties",
            DisplayName = "Brief",
            GetValue = (instance) => instance.Brief,
            SetValue = (instance, value) => instance.Brief = value,
            ValueValidators = [_stringMustNotBeEmptyPropertyValueValidator, _stringMustNotContainDigitPropertyValueValidator]
        };

        yield return new PropertyDescriptor<ExampleBarInstance, string?>()
        {
            ConsiderPredicate = (_) => true,
            Name = nameof(ExampleBarInstance.Description),
            Category = "String properties",
            DisplayName = "Description",
            GetValue = (instance) => instance.Description,
            SetValue = (instance, value) => instance.Description = value,
            CanBeSetToNull = true
        };

        yield return new PropertyDescriptor<ExampleBarInstance, Uri>()
        {
            ConsiderPredicate = (_) => true,
            Name = nameof(ExampleBarInstance.Uri),
            Category = "String properties",
            DisplayName = "URI",
            GetDefaultValue = (_) => ExampleBarInstance.UriDefaultValue,
            GetValue = (instance) => instance.Uri,
            SetValue = (instance, value) => instance.Uri = value,
            CanBeSetToNull = false
        };

        yield return new PropertyDescriptor<ExampleBarInstance, Uri?>()
        {
            ConsiderPredicate = (_) => true,
            Name = nameof(ExampleBarInstance.NullableUri),
            Category = "String properties",
            DisplayName = "Nullable URI",
            GetValue = (instance) => instance.NullableUri,
            SetValue = (instance, value) => instance.NullableUri = value,
            CanBeSetToNull = true
        };

        yield return new PropertyDescriptor<ExampleBarInstance, bool>
        {
            Name = nameof(ExampleBarInstance.BooleanValue),
            Category = "Boolean properties",
            DisplayName = "Boolean",
            Description = "Basic boolean property",
            GetValue = (instance) => instance.BooleanValue,
            SetValue = (instance, value) => instance.BooleanValue = value,
            GetDefaultValue = (_) => ExampleBarInstance.BooleanValueDefaultValue,
            ResetValue = (instance) => instance.BooleanValue = ExampleBarInstance.BooleanValueDefaultValue,
            HasValueDifferentFromDefaultValue = (instance, defaultValue) => instance.BooleanValue != defaultValue
        };

        yield return new NumericPropertyDescriptor<ExampleBarInstance, byte, byte, byte>
        {
            Name = nameof(ExampleBarInstance.ByteValue),
            Category = "Numeric properties",
            DisplayName = "Rastered byte",
            Description = "Byte that supports increment by 10 and is rastered",
            GetValue = (instance) => instance.ByteValue,
            Minimum = 5,
            Maximum = 200,
            Interval = 10,
            IsRasteredValue = true,
            SetValue = (instance, value) => instance.ByteValue = value
        };

        yield return new NumericPropertyDescriptor<ExampleBarInstance, int?, int, int>
        {
            Name = nameof(ExampleBarInstance.NullableIntegerValue),
            Category = "Numeric properties",
            DisplayName = "Nullable integer",
            Description = "Numeric property that supports increment by 1 and is nullable",
            GetValue = (instance) => instance.NullableIntegerValue,
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Interval = 1,
            SetValue = (instance, value) => instance.NullableIntegerValue = value,
            CanBeSetToNull = true
        };

        yield return new NumericPropertyDescriptor<ExampleBarInstance, ulong, ulong, ulong>
        {
            Name = nameof(ExampleBarInstance.UnsignedLong),
            Category = "Numeric properties",
            DisplayName = "Unsigned long",
            Description = "Unsigned long",
            GetValue = (instance) => instance.UnsignedLong,
            Minimum = ulong.MinValue,
            Maximum = ulong.MaxValue,
            Interval = 1,
            SetValue = (instance, value) => instance.UnsignedLong = value
        };

        yield return new SelectionPropertyDescriptor<ExampleBarInstance, SectionId?>
        {
            Name = nameof(ExampleBarInstance.Section),
            Category = "Possible value properties",
            DisplayName = "Section",
            Description = "Properties with a predefined set of possible values to select the current property value from",
            GetSelectableValues = (_) => Enum.GetValues<SectionId>()
                .Skip(2)
                .Select(v => new SelectableValue<SectionId?> { Value = v, Text = v.ToString() }),
            GetValue = (instance) => instance.Section,
            SetValue = (instance, value) => instance.Section = value
        };

        yield return new SelectionPropertyDescriptor<ExampleBarInstance, EnumValueName>
        {
            Name = nameof(ExampleBarInstance.AnotherSection),
            Category = "Possible value properties",
            GetSelectableValues = (_) => Enum.GetValues<SectionId>()
                .Select(v => new SelectableValue<EnumValueName> { Value = new(v.ToString()), Text = v.ToString() }),
            GetValue = (instance) => new(instance.AnotherSection.ToString()),
            SetValue = (instance, value) => instance.AnotherSection = Enum.Parse<SectionId>(value.Value)
        };
    }
}
