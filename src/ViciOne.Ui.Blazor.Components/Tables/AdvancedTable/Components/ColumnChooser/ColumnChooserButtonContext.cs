using Microsoft.AspNetCore.Components;

namespace ViciOne.Ui.Blazor.Components.Tables.AdvancedTable.Components.ColumnChooser;

/// <summary>
/// The state and behavior a custom column chooser button needs to drive the popup it opens, handed to
/// <see cref="ColumnChooserToggle.Button"/>.
/// </summary>
/// <param name="PopupVisible">Whether the popup is currently open.</param>
/// <param name="PopupTitle">The title of the popup. It can be used to customize the button, such as its tooltip.</param>
/// <param name="Toggle">Invoked to open the popup if it is closed, or close it if it is open.</param>
public sealed record ColumnChooserButtonContext(bool PopupVisible, string PopupTitle, EventCallback Toggle);
