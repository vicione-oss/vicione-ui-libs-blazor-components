using ViciOne.Ui.Blazor.Components.Attributes;

namespace ViciOne.Ui.Blazor.Components.Enums;

/// <summary>
/// Keys that can be pressed on the keyboard to modify the type of interaction executed.
/// </summary>
/// <remarks>
/// Enum values match with https://developer.mozilla.org/en-US/docs/Web/API/UI_Events/Keyboard_event_key_values.
/// </remarks>
[GenerateTypeScriptEnum]
public enum ModifierKey
{
    /// <summary>
    /// ALT key
    /// </summary>
    Alt
}
