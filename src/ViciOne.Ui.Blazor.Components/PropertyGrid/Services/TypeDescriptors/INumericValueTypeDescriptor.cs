using ViciOne.Ui.Blazor.Components.PropertyGrid.Attributes;

namespace ViciOne.Ui.Blazor.Components.PropertyGrid.Services.TypeDescriptors;

/// <summary>
/// Describes a numeric value type.
/// </summary>
public interface INumericValueTypeDescriptor
{
    /// <summary>
    /// Gets the underlying type.
    /// </summary>
    Type UnderlyingType { get; }
}

/// <summary>
/// Describes <typeparamref name="TNumericValueType"/>
/// </summary>
[GenerateNumericValueTypeDescriptor(Type = typeof(byte), Alias = "Byte")]
[GenerateNumericValueTypeDescriptor(Type = typeof(sbyte), Alias = "SignedByte")]
[GenerateNumericValueTypeDescriptor(Type = typeof(ushort), Alias = "UnsignedShort")]
[GenerateNumericValueTypeDescriptor(Type = typeof(uint), Alias = "UnsignedInt")]
[GenerateNumericValueTypeDescriptor(Type = typeof(ulong), Alias = "UnsignedLong")]
[GenerateNumericValueTypeDescriptor(Type = typeof(short), Alias = "Short")]
[GenerateNumericValueTypeDescriptor(Type = typeof(int), Alias = "Int")]
[GenerateNumericValueTypeDescriptor(Type = typeof(long), Alias = "Long")]
[GenerateNumericValueTypeDescriptor(Type = typeof(decimal), Alias = "Decimal")]
[GenerateNumericValueTypeDescriptor(Type = typeof(double), Alias = "Double")]
[GenerateNumericValueTypeDescriptor(Type = typeof(float), Alias = "Float")]
public interface INumericValueTypeDescriptor<TNumericValueType> : INumericValueTypeDescriptor
{
    /// <summary>
    /// The minium value that TPropertyValue may represent.
    /// </summary>
    TNumericValueType Minimum { get; }

    /// <summary>
    /// The maximum value that TPropertyValue may represent.
    /// </summary>
    TNumericValueType Maximum { get; }

    /// <summary>
    /// Represents the number one (1).
    /// </summary>
    TNumericValueType One { get; }
}
