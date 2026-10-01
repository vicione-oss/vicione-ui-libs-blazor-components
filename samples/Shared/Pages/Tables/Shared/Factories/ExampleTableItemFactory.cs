using System.Security.Cryptography;
using Bogus;
using Shared.Pages.Tables.Shared.Models;

namespace Shared.Pages.Tables.Shared.Factories;

internal static class ExampleTableItemFactory
{
    public static ExampleTableItem Create()
    {
        var faker = new Faker();

        return new ExampleTableItem
        {
            Id = Guid.NewGuid(),
            Key = faker.Random.String2(3),
            Value = faker.Random.String2(8),
            Timestamp = DateTimeOffset.Now.AddMinutes(RandomNumberGenerator.GetInt32((96 * 60) - (48 * 60))),
            Quantity = faker.Random.Int(1, 500)
        };
    }

    public static List<ExampleTableItem> CreateMany(int count)
        => [.. Enumerable.Repeat(0, count).Select(_ => Create())];
}
