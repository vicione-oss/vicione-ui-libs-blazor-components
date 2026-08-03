using ViciOne.Ui.Blazor.Components.SpinEdit.Attributes;

namespace ViciOne.Ui.Blazor.Components.SpinEdit.Services.Behaviors;

/// <summary>
/// Provides value arithmetic and validation rules for spin-edit controls.
/// Implementations define how to increment/decrement, clamp to limits and align values to a raster,
/// for a specific value, interval and limit types.
/// </summary>
/// <typeparam name="TValue">The numeric value type handled by the behavior (e.g. int, decimal).</typeparam>
/// <typeparam name="TInterval">The type used for step intervals.</typeparam>
/// <typeparam name="TLimit">The type used for minimum/maximum limits.</typeparam>
[GenerateNumericSpinBehavior(Type = typeof(byte), Alias = "Byte")]
[GenerateNumericSpinBehavior(Type = typeof(sbyte), Alias = "SignedByte")]
[GenerateNumericSpinBehavior(Type = typeof(ushort), Alias = "UnsignedShort")]
[GenerateNumericSpinBehavior(Type = typeof(uint), Alias = "UnsignedInt")]
[GenerateNumericSpinBehavior(Type = typeof(ulong), Alias = "UnsignedLong")]
[GenerateNumericSpinBehavior(Type = typeof(short), Alias = "Short")]
[GenerateNumericSpinBehavior(Type = typeof(int), Alias = "Int")]
[GenerateNumericSpinBehavior(Type = typeof(long), Alias = "Long")]
[GenerateNumericSpinBehavior(Type = typeof(decimal), Alias = "Decimal")]
[GenerateNumericSpinBehavior(Type = typeof(double), Alias = "Double")]
[GenerateNumericSpinBehavior(Type = typeof(float), Alias = "Float")]
public interface ISpinBehavior<TValue, TInterval, TLimit>
{
    /// <summary>
    /// Returns the value produced by adding <paramref name="interval"/> to <paramref name="value"/>,
    /// then constraining the result to the inclusive range defined by <paramref name="minimum"/> and <paramref name="maximum"/>.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="interval">The step to add to <paramref name="value"/>.</param>
    /// <param name="minimum">Inclusive lower bound for the result.</param>
    /// <param name="maximum">Inclusive upper bound for the result.</param>
    /// <returns>The incremented value, clamped to the provided limits.</returns>
    TValue Increment(TValue value, TInterval interval, TLimit minimum, TLimit maximum);

    /// <summary>
    /// Returns the value produced by subtracting <paramref name="interval"/> from <paramref name="value"/>,
    /// then constraining the result to the inclusive range defined by <paramref name="minimum"/> and <paramref name="maximum"/>.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="interval">The step to subtract from <paramref name="value"/>.</param>
    /// <param name="minimum">Inclusive lower bound for the result.</param>
    /// <param name="maximum">Inclusive upper bound for the result.</param>
    /// <returns>The decremented value, clamped to the provided limits.</returns>
    TValue Decrement(TValue value, TInterval interval, TLimit minimum, TLimit maximum);

    /// <summary>
    /// Ensures <paramref name="value"/> lies within the inclusive range defined by <paramref name="minimum"/> and <paramref name="maximum"/>.
    /// If <paramref name="value"/> is outside the range, the returned value is the nearest bound.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="minimum">Inclusive lower bound.</param>
    /// <param name="maximum">Inclusive upper bound.</param>
    /// <returns><paramref name="value"/> if within range; otherwise the nearest bound.</returns>
    TValue EnsureRange(TValue value, TLimit minimum, TLimit maximum);

    /// <summary>
    /// Aligns <paramref name="value"/> to the nearest valid raster (multiple) of <paramref name="interval"/>.
    /// Implementations decide rounding semantics (e.g. round, floor, or ceil) when <paramref name="value"/> is not an exact multiple.
    /// </summary>
    /// <param name="value">The value to align.</param>
    /// <param name="interval">The raster interval to align to.</param>
    /// <returns>The aligned value that is a multiple of <paramref name="interval"/> according to the implementation's policy.</returns>
    TValue EnsureRaster(TValue value, TInterval interval);

    /// <summary>
    /// Attempts to parse the string representation <paramref name="valueStr"/> into the typed value.
    /// Implementations should not throw on parse failure; instead return <c>false</c>.
    /// </summary>
    /// <param name="valueStr">The string to parse.</param>
    /// <param name="valueTyped">When this method returns, contains the parsed value if parsing succeeded; otherwise the default value of <typeparamref name="TValue"/>.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    bool TryParse(string valueStr, out TValue valueTyped);
}
