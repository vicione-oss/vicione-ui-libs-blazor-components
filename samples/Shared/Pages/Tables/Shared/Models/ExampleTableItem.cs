namespace Shared.Pages.Tables.Shared.Models;

public sealed class ExampleTableItem
{
    public required Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    public required int Quantity { get; set; }
}
