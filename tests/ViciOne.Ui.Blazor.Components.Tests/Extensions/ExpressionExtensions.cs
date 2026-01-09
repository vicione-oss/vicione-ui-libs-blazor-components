using System.Linq.Expressions;
using System.Reflection;

namespace ViciOne.Ui.Blazor.Components.Tests.Extensions;

internal static class ExpressionExtensions
{
    public static PropertyInfo GetPropertyInfo<T, TProperty>(this Expression<Func<T, TProperty>> propertySelector)
    {
        if (propertySelector.Body is not MemberExpression memberExpression)
            throw new ArgumentException($"Expression is not a {nameof(MemberExpression)}", nameof(propertySelector));

        if (memberExpression.Member is not PropertyInfo propertyInfo)
            throw new ArgumentException("Expression must select a property", nameof(propertySelector));

        return propertyInfo;
    }

    public static string GetPropertyName<T, TProperty>(this Expression<Func<T, TProperty>> propertySelector)
    {
        var propertyInfo = propertySelector.GetPropertyInfo();

        return propertyInfo.Name;
    }
}
