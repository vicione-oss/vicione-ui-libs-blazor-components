using System.Diagnostics.CodeAnalysis;
using ViciOne.Ui.Blazor.Components.PropertyGrid.Services;

namespace Shared.Pages.PropertyGrid.Services;

internal sealed class ExampleStringPropertyValueEqualityComparer : IPropertyValueEqualityComparer<string>
{
    private readonly EqualityComparer<string> _stringEqualityComparer = EqualityComparer<string>.Default;

    public bool Equals(string? x, string? y) => _stringEqualityComparer.Equals(x, y);
    public int GetHashCode([DisallowNull] string obj) => _stringEqualityComparer.GetHashCode(obj);
}
