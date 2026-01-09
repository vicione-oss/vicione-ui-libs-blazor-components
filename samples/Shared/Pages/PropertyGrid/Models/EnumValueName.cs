namespace Shared.Pages.PropertyGrid.Models;

[StronglyTypedId(backingType: StronglyTypedIdBackingType.String, jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct EnumValueName
{
    public EnumValueName()
        : this(string.Empty) // assign empty to avoid possible null-reference exception in generated Equals()
    {
    }

    public static bool operator <(EnumValueName left, EnumValueName right)
        => left.CompareTo(right) < 0;

    public static bool operator <=(EnumValueName left, EnumValueName right)
        => left.CompareTo(right) <= 0;

    public static bool operator >(EnumValueName left, EnumValueName right)
        => left.CompareTo(right) > 0;

    public static bool operator >=(EnumValueName left, EnumValueName right)
        => left.CompareTo(right) >= 0;
}
