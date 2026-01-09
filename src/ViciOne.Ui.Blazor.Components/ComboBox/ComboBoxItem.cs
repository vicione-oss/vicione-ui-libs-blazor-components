namespace ViciOne.Ui.Blazor.Components.ComboBox;

/// <summary>
/// Generic item implementation for use with <see cref="ComboBox{TItem, TValue}"/>.
/// </summary>
/// <typeparam name="TValue">Type of the <see cref="Value"/> property</typeparam>
/// <typeparam name="TText">Type of the <see cref="Text"/> property</typeparam>
public sealed class ComboBoxItem<TValue, TText>
{
    /// <summary>
    /// Value used to render the
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/option#value">value</see> attribute
    /// of the <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/option">option</see> element
    /// associated with the item
    /// </summary>
    public required TValue Value { get; init; }

    /// <summary>
    /// Text used to render the content of the
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/option">option</see> element
    /// associated with the item
    /// </summary>
    public required TText Text { get; init; }
}

