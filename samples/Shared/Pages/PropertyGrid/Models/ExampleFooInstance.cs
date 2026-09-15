using Shared.Pages.SectionRail.Enums;

namespace Shared.Pages.PropertyGrid.Models;

public sealed class ExampleFooInstance : IHasName
{
    public static readonly bool BooleanValueDefaultValue = true;
    public static readonly bool BooleanValueReversedDefaultValue;

    public string Name { get; set; } = "ABCD";
    public string? Brief { get; set; } = "Dolor sit amet";
    public string? Description { get; set; } = "Foo instance";
    public string Abstract { get; set; } = "Lorem ipsum";
    public string? LongTitle { get; set; } = "Consetetur sadipscing";
    public string? LongDescription { get; set; } = "Sed diam nonumy";
    public bool BooleanValue { get; set; } = BooleanValueDefaultValue;
    public bool BooleanValueReversed { get; set; } = BooleanValueReversedDefaultValue;
    public bool BooleanValuesResettable { get; set; }
    public bool? NullableBooleanValue { get; set; }
    public SectionId? Section { get; set; }
    public SectionId AnotherSection { get; set; }
    public int IntegerValue1 { get; set; }
    public int IntegerValue2 { get; set; } = 10;
    public byte ByteValue { get; set; } = 15;
    public int? NullableIntegerValue { get; set; }
    public ulong UnsignedLong { get; set; } = 5;
    public long SignedLong { get; set; } = 30;
}
