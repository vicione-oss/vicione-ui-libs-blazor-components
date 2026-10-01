namespace ViciOne.Ui.Blazor.Components.Tests.Tables.Shared.Models;

internal sealed record TestColumn(string Id)
{
    public string? Title { get; init; }

    public int? Width { get; init; }

    public int? MinimumWidth { get; init; }

    public bool? Resizeable { get; init; }
}
