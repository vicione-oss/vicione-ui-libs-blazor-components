using System.Globalization;
using System.Text;
using ViciOne.Ui.Blazor.Components.Helpers;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Messages;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;
using ViciOne.Ui.Blazor.Components.Resources.PropertyGrid.Localization;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Items;

internal class PropertyGridItem<TPropertyValue>(ILookup<Type, object> instancesByType,
    IEnumerable<IPropertyDescriptor> propertyDescriptors, IEqualityComparer<TPropertyValue> valueEqualityComparer,
    IPropertyGridMessageStore messageStore)
        : IPropertyGridItem<TPropertyValue>
{
    private static readonly Type s_valueType = typeof(TPropertyValue);

    private static readonly CompositeFormat s_propertyDoesNotHaveNullableValueTypeAlthoughValueIsNull =
        CompositeFormat.Parse(ValidationMessages.PropertyDoesNotHaveNullableValueTypeAlthoughValueIsNull);

    private readonly IPropertyDescriptor _firstPropertyDescriptor = propertyDescriptors.First();

    public string? Description { get; } = propertyDescriptors.Select(d => d.Description)
        .Distinct().Count() == 1 ? propertyDescriptors.First().Description : null;

    public string? InformationTooltipText { get; } = propertyDescriptors.Select(d => d.InformationTooltip)
        .Distinct().Count() == 1 ? propertyDescriptors.First().InformationTooltip : null;

    protected IReadOnlyList<PropertyOperationIteration> PropertyOperationIterations { get; } =
        CreatePropertyOperationIterations(instancesByType, propertyDescriptors, s_valueType);

    public IEqualityComparer<TPropertyValue> ValueEqualityComparer => valueEqualityComparer;

    public Type ValueType => s_valueType;

    public string Category => _firstPropertyDescriptor.Category;

    public string Name => _firstPropertyDescriptor.Name;

    public string DisplayName => _firstPropertyDescriptor.DisplayName ?? _firstPropertyDescriptor.Name;

    public bool CanBeSetToNull => propertyDescriptors.All(p => p.CanBeSetToNull) &&
        PropertyOperationIterations.ReadUnifiedCanSetValue();

    public bool Resettable => PropertyOperationIterations.ReadUnifiedResettable();
    public bool ReadOnly => PropertyOperationIterations.ReadUnifiedReadOnly() ||
        !PropertyOperationIterations.ReadUnifiedCanSetValue();
    public bool Enabled => PropertyOperationIterations.ReadUnifiedEnabled();
    public bool Visible => PropertyOperationIterations.ReadUnifiedVisible();

    public IPropertyGridMessageStore MessageStore => messageStore;

    public IEnumerable<IPropertyDescriptor> PropertyDescriptors => propertyDescriptors;

    public ValueOf<TPropertyValue>? ReadUnifiedValue()
        => PropertyOperationIterations.ReadUnifiedValue(valueEqualityComparer);

    public bool ResetValue()
    {
        messageStore.Remove(this);

        try
        {
            var result = PropertyOperationIterations.ResetValue<TPropertyValue>(out var exceptions);

            if (result == ResetValueResult.ResetFailedAndValuesRestored)
            {
                CreateValueRestoredInfoMessageFrom(exceptions, messageStore);

                return false;
            }

            return result == ResetValueResult.Success;
        }
        catch (Exception unexpectedException)
        {
            HandleUnexpectedException(unexpectedException, messageStore);

            return false;
        }
    }

    public bool SetNull()
        => SetValue(default!);

    public bool SetValue(TPropertyValue value)
    {
        messageStore.BeginUpdate();
        try
        {
            messageStore.Remove(this);

            try
            {
                var result = PropertyOperationIterations.SetValue(value, out var exceptions);

                if (result == SetValueResult.SaveFailedAndValuesRestored)
                {
                    CreateValueRestoredInfoMessageFrom(exceptions, messageStore);

                    return false;
                }

                return result == SetValueResult.Success;
            }
            catch (Exception unexpectedException)
            {
                HandleUnexpectedException(unexpectedException, messageStore);

                return false;
            }
        }
        finally
        {
            messageStore.EndUpdate();
        }
    }

    private readonly record struct ExceptionGroupingKey(Type ExceptionType, string Message);

    private void CreateValueRestoredInfoMessageFrom(List<Exception> exceptions, IPropertyGridMessageStore messageStore)
    {
        var exceptionLookUp = exceptions.GroupBy(exception =>
            new ExceptionGroupingKey { ExceptionType = exception.GetType(), Message = exception.Message });

        var innerMessages = new List<string>();

        foreach (var exceptionGrouping in exceptionLookUp)
        {
            var groupedExceptions = exceptionGrouping.ToList();

            var errorMessage = $"{groupedExceptions[0].Message}";

            if (groupedExceptions.Count > 1)
                errorMessage += $" ({groupedExceptions.Count})";

            innerMessages.Add(errorMessage);
        }

        var infoMessage = new ValueRestoredInfoMessage
        {
            Text = PropertyGridItem.TheLastValueUsedHasBeenRestored,
            Reasons = innerMessages
        };

        messageStore.Add(this, infoMessage);
    }

    private void HandleUnexpectedException(Exception unexpectedException, IPropertyGridMessageStore messageStore)
        => messageStore.Add(this, new ErrorMessage { Text = unexpectedException.Message });

    private static List<PropertyOperationIteration> CreatePropertyOperationIterations(ILookup<Type, object> instancesByType,
        IEnumerable<IPropertyDescriptor> propertyDescriptors, Type propertyValueType)
    {
        var iterations = new List<PropertyOperationIteration>();

        foreach (var g in instancesByType)
        {
            var instanceType = g.Key;
            var instances = g.Cast(instanceType).ToReadOnlyList(instanceType);

            var propertyDescriptorType = typeof(IPropertyDescriptor<,>).MakeGenericType(instanceType, propertyValueType);

            var propertyDescriptor = propertyDescriptors.OfType(propertyDescriptorType).FirstOrDefault();

            if (propertyDescriptor is not null)
            {
                iterations.Add(new PropertyOperationIteration
                {
                    InstanceType = instanceType,
                    PropertyValueType = propertyValueType,
                    PropertyDescriptor = propertyDescriptor,
                    Instances = instances
                });
            }
        }

        return iterations;
    }

    public bool Validate(TPropertyValue value)
    {
        messageStore.BeginUpdate();
        try
        {
            messageStore.Remove(this);

            if (value is null && !GenericParameterHelper.IsNullable<TPropertyValue>())
            {
                // this should never be the case
                var errorMessageText = string.Format(CultureInfo.InvariantCulture,
                    s_propertyDoesNotHaveNullableValueTypeAlthoughValueIsNull, DisplayName);

                messageStore.Add(this, new ErrorMessage { Text = errorMessageText });

                return false;
            }

            var validationMessages = new List<string>();

            PropertyOperationIterations.Validate(value, validationMessages);

            if (validationMessages.Count > 0)
            {
                foreach (var validationMessage in validationMessages)
                    messageStore.Add(this, new ErrorMessage { Text = validationMessage });

                return false;
            }

            return true;
        }
        finally
        {
            messageStore.EndUpdate();
        }
    }

    public bool IsDefaultValueDifferentFrom(ValueOf<TPropertyValue>? value)
    {
        if (PropertyOperationIterations.ReadUnifiedDefaultValue(valueEqualityComparer) is not ValueOf<TPropertyValue> defaultValue)
            return false; // no uniform default value, hence we should not indicate difference

        if (PropertyOperationIterations
            .ReadUnifiedHasValueDifferentFromDefaultValue(defaultValue.Value) is bool hasValueDifferentFromDefaultValue)
        {
            return hasValueDifferentFromDefaultValue;
        }

        if (value is null)
            return defaultValue is not null;
        else
            return !valueEqualityComparer.Equals(value.Value, defaultValue.Value);
    }
}
