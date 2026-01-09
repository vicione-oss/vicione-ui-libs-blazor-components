namespace Shared.Models;

public class ExampleGridItem
{
    public required Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public required DateTime Timestamp { get; set; }
}
