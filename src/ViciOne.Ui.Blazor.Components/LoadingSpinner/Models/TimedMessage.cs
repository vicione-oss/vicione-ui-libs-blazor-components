namespace ViciOne.Ui.Blazor.Components.LoadingSpinner.Models;

/// <summary>
/// A message object used with the <see cref="LoadingSpinner"/>.
/// </summary>
public sealed record TimedMessage
{
    private int? _remainingDisplayDuration;

    /// <summary>
    /// Gets or sets the amount of seconds the message shall be visible for.
    ///
    /// <para>
    ///     A value smaller than zero is interpreted as "infinite". If set
    ///     to "infinite" messages queued after this message can only be
    ///     displayed by manual intervention.
    ///  </para>
    /// </summary>
    public int DisplayDuration { get; init; } = -1;

    /// <summary>
    ///     Gets or sets the message to display.
    /// </summary>
    public required string Message { get; set; }

    internal int RemainingDisplayDuration
    {
        get => _remainingDisplayDuration ??= DisplayDuration;
        set
        {
            if (value == _remainingDisplayDuration)
                return;

            _remainingDisplayDuration = value;
        }
    }

    internal void ResetRemainingDuration()
        => _remainingDisplayDuration = null;
}
