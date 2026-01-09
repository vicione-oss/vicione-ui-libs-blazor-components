namespace ViciOne.Ui.Blazor.Components.Interfaces;

/// <summary>
/// Describes an instance that supports a batched update cycle via an update lock.
/// </summary>
public interface IHasUpdateLock
{
    /// <summary>
    /// Gets the number of times <see cref="BeginUpdate"/> was called without a corresponding call to
    /// <see cref="EndUpdate"/>.
    /// </summary>
    /// <remarks>
    /// When <see cref="UpdateLock"/> is greater than 0, the instance must not raise any events or
    /// notify observers; notifications should be deferred until the update cycle is completed.
    /// </remarks>
    int UpdateLock { get; }

    /// <summary>
    /// Begins an update cycle.
    /// Increments <see cref="UpdateLock"/>.
    /// </summary>
    /// <remarks>
    /// Each call to <see cref="BeginUpdate"/> must be paired with a corresponding call to
    /// <see cref="EndUpdate"/> to complete the cycle.
    /// </remarks>
    void BeginUpdate();

    /// <summary>
    /// Ends an update cycle.
    /// Decrements <see cref="UpdateLock"/>.
    /// </summary>
    void EndUpdate();
}

