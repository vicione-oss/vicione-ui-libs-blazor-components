using Microsoft.Extensions.DependencyInjection;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Extensions;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Builders;

internal sealed class PropertyGridBuilder<TContext>(IServiceCollection services) : IPropertyGridBuilder<TContext>
{
    public IPropertyGridBuilder<TContext> WithPropertyDescriptorProvider<TPropertyDescriptorProvider>()
        where TPropertyDescriptorProvider : class, IPropertyDescriptorProvider<TContext>
    {
        services.AddPropertyDescriptorProvider<TContext, TPropertyDescriptorProvider>();

        return this;
    }

    public IPropertyGridBuilder<TContext> WithPropertyValueEqualityComparer<TPropertyValue, TPropertyValueEqualityComparer>()
        where TPropertyValueEqualityComparer : class, IPropertyValueEqualityComparer<TPropertyValue>
    {
        services.AddPropertyValueEqualityComparer<TContext, TPropertyValue, TPropertyValueEqualityComparer>();

        return this;
    }
}
