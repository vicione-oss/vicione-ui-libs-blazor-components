using ViciOne.Ui.Testing.Playwright.Attributes;

namespace Server.Tests.Infrastructure;

[CollectionDefinition("Server web application test collection")]
[PlaywrightTest]
public class ServerTestCollection : ICollectionFixture<ServerFixture>;
