using ViciOne.Ui.Blazor.Components.SpinEdit;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Components.Editors;

internal interface INumericPropertyEditor<TInterval, TLimit>
{
    /// <inheritdoc cref="SpinEdit{TValue, TInterval, TLimit}.Interval"/>
    TInterval Interval { get; set; }

    /// <inheritdoc cref="SpinEdit{TValue, TInterval, TLimit}.IsRastered"/>
    bool IsRastered { get; set; }

    /// <inheritdoc cref="SpinEdit{TValue, TInterval, TLimit}.Minimum"/>
    TLimit Minimum { get; set; }

    /// <inheritdoc cref="SpinEdit{TValue, TInterval, TLimit}.Maximum"/>
    TLimit Maximum { get; set; }
}
