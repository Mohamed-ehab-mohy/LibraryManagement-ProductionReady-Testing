using LibraryManagement.IntegrationTests.Factories;

namespace LibraryManagement.IntegrationTests;

public class IntegrationTestBase : IClassFixture<LibraryWebAppFactory>, IAsyncLifetime
{
    protected readonly LibraryWebAppFactory Factory;
    protected readonly HttpClient Client;

    public IntegrationTestBase(LibraryWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
