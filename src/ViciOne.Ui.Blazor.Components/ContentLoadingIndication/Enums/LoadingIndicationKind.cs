using ViciOne.Ui.Blazor.Components.Enums;

namespace ViciOne.Ui.Blazor.Components.ContentLoadingIndication.Enums;

/// <summary>
/// Available loading indications implemented in <see cref="ContentLoadingIndication"/>
/// </summary>
public readonly record struct LoadingIndicationKind : ITypeSafeEnumImplemention<LoadingIndicationKind>
{
    /// <summary>
    /// Loading indication with a spinner animation on the left side
    /// </summary>
    public static readonly LoadingIndicationKind SpinnerLeft = new(nameof(SpinnerLeft));

    /// <summary>
    /// Loading indication with a spinner animation in the center
    /// </summary>
    public static readonly LoadingIndicationKind SpinnerCenter = new(nameof(SpinnerCenter));

    /// <summary>
    /// Loading indication with a spinner animation on the right side
    /// </summary>
    public static readonly LoadingIndicationKind SpinnerRight = new(nameof(SpinnerRight));

    /// <summary>
    /// Loading indication that adjusts opacity
    /// </summary>
    public static readonly LoadingIndicationKind Opacity = new(nameof(Opacity));

    private readonly string _name;

    internal LoadingIndicationKind(string name)
        => _name = name;

    /// <inheritdoc/>
    public static LoadingIndicationKind GetDefaultValue()
        => SpinnerCenter;

    /// <inheritdoc/>
    public string GetName()
        => _name;

    /// <inheritdoc cref="GetName"/>
    public override string ToString()
        => GetName();
}
