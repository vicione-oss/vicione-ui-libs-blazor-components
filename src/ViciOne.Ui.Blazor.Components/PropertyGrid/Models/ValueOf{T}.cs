namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Models;

/// <summary>
/// This class will be instantiated when <paramref name="value"/> needs to be passed around
/// to communicate "that is an actual value". The actual value can then be of
/// type <typeparamref name="T"/> which includes <see langword="null"/> when
/// <typeparamref name="T"/> is a nullable type.
/// In cases where <see langword="null"/> represents "no actual value" (e.g. when a
/// uniform value across multiple instances cannot be determined), then
/// <see langword="null"/> is passed around instead of an instance of <see cref="ValueOf{T}"/>
/// to avoid confusing "that is an actual value, in this case null" with "no actual value".
/// </summary>
internal sealed class ValueOf<T>(T value)
{
    public T Value => value;
}
