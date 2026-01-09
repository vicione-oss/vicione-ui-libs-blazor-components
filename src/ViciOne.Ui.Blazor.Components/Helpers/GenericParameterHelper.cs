namespace ViciOne.Ui.Blazor.Components.Helpers;

internal sealed class GenericParameterHelper
{
    // https://stackoverflow.com/a/23397792/3936440
    public static bool IsNullable<T>()
        => default(T) == null;
}
