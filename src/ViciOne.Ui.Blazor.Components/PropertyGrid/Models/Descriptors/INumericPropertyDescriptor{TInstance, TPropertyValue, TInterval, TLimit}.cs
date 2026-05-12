namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models.Descriptors;

/// <inheritdoc cref="INumericPropertyDescriptor{TInterval, TLimit}"/>
public interface INumericPropertyDescriptor<TInstance, TPropertyValue, TInterval, TLimit>
    : IPropertyDescriptor<TInstance, TPropertyValue>, INumericPropertyDescriptor<TInterval, TLimit>
        where TInterval : struct
        where TLimit : struct;
