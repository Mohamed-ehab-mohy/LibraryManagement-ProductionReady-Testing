using System.Net;
using Shouldly;
using LibraryManagement.IntegrationTests.Factories;

namespace LibraryManagement.IntegrationTests;

public class BookEndpointsTests : IClassFixture<LibraryWebAppFactory>
{
    private readonly LibraryWebAppFactory _factory;

    public BookEndpointsTests(LibraryWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetBooks_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/books");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
