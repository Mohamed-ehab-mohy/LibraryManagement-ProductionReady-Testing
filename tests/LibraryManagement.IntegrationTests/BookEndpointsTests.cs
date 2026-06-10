using System.Net;
using System.Net.Http.Json;
using Shouldly;
using LibraryManagement.IntegrationTests.Factories;
using LibraryManagement.Services.DTOs;

namespace LibraryManagement.IntegrationTests;

public class BookEndpointsTests : IClassFixture<LibraryWebAppFactory>
{
    private readonly LibraryWebAppFactory _factory;

    public BookEndpointsTests(LibraryWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetBooks_ReturnsAllBooks()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/books");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        books.ShouldNotBeNull();
        books.Count.ShouldBe(3);
    }

    [Fact]
    public async Task GetBooks_WithAvailableFilter_ReturnsOnlyAvailable()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/books?available=true");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        books.ShouldNotBeNull();
        books.All(b => b.AvailableCopies > 0).ShouldBeTrue();
    }

    [Fact]
    public async Task GetBookById_WhenExists_ReturnsBook()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/books/1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        book.ShouldNotBeNull();
        book.Id.ShouldBe(1);
        book.Title.ShouldBe("Clean Code");
    }

    [Fact]
    public async Task GetBookById_WhenNotExists_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/books/999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBook_ReturnsCreated()
    {
        var client = _factory.CreateClient();
        var dto = new CreateBookDto
        {
            Title = "Test Driven Development",
            Author = "Kent Beck",
            ISBN = "9780321146533",
            TotalCopies = 2
        };

        var response = await client.PostAsJsonAsync("/api/books", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        book.ShouldNotBeNull();
        book.Title.ShouldBe("Test Driven Development");
        book.AvailableCopies.ShouldBe(2);
    }

    [Fact]
    public async Task CreateBook_WithDuplicateIsbn_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var dto = new CreateBookDto
        {
            Title = "Duplicate",
            Author = "Author",
            ISBN = "9780132350884",
            TotalCopies = 1
        };

        var response = await client.PostAsJsonAsync("/api/books", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateBook_WithInvalidTotalCopies_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var dto = new CreateBookDto
        {
            Title = "Bad Book",
            Author = "Author",
            ISBN = "1111111111111",
            TotalCopies = 0
        };

        var response = await client.PostAsJsonAsync("/api/books", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
}
