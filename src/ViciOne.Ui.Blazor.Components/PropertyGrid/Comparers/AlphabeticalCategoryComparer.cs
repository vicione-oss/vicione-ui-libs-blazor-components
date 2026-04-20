namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;

internal sealed class AlphabeticalCategoryComparer : IAlphabeticalCategoryComparer
{
    public int Compare(string? x, string? y)
        => StringComparer.OrdinalIgnoreCase.Compare(x, y);
}
