using Shared.Pages.SectionRail.Enums;

namespace Shared.Pages.PropertyGrid.Models;

public sealed class ExampleBarInstance : IHasName
{
    public static readonly Uri UriDefaultValue = new("https://www.google.de");
    public static readonly bool BooleanValueDefaultValue;

    public string Name { get; set; } = "XYZ!";
    public string Brief { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Uri Uri { get; set; } = UriDefaultValue;
    public Uri? NullableUri { get; set; }
    public bool BooleanValue { get; set; } = BooleanValueDefaultValue;
    public byte ByteValue { get; set; } = 20;
    public int? NullableIntegerValue { get; set; } = 30;
    public ulong UnsignedLong { get; set; } = 10;
    public SectionId? Section { get; set; } = SectionId.Library;
    public SectionId AnotherSection { get; set; }
}
