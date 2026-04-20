namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Comparers;

internal sealed class ReverseAlphabeticalCategoryComparer : IReverseAlphabeticalCategoryComparer
{
    public int Compare(string? x, string? y)
        => string.CompareOrdinal(y, x);
}
