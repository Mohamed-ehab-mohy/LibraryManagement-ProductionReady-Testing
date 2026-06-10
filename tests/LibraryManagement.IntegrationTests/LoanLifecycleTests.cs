using System.Net;
using System.Net.Http.Json;
using Shouldly;
using LibraryManagement.IntegrationTests.Factories;
using LibraryManagement.Services.DTOs;

namespace LibraryManagement.IntegrationTests;

public class LoanLifecycleTests : IClassFixture<LibraryWebAppFactory>
{
    private readonly LibraryWebAppFactory _factory;

    public LoanLifecycleTests(LibraryWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BorrowBook_WithValidRequest_CreatesLoan()
    {
        var client = _factory.CreateClient();

        var book = await client.GetFromJsonAsync<List<BookDto>>("/api/books");
        var startingCopies = book!.First(b => b.Id == 3).AvailableCopies;

        var response = await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 3 });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
        loan.ShouldNotBeNull();
        loan.BookId.ShouldBe(3);
        loan.MemberId.ShouldBe(1);
        loan.ReturnedAt.ShouldBeNull();

        var updated = await client.GetFromJsonAsync<BookDto>($"/api/books/3");
        updated!.AvailableCopies.ShouldBe(startingCopies - 1);
    }

    [Fact]
    public async Task BorrowBook_WhenBookNotAvailable_ReturnsUnprocessable()
    {
        var client = _factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/books", new CreateBookDto
        {
            Title = "Single Copy Book",
            Author = "Author",
            ISBN = "1000000000001",
            TotalCopies = 1
        });
        var book = await created.Content.ReadFromJsonAsync<BookDto>();

        var firstBorrow = await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = book!.Id });
        firstBorrow.StatusCode.ShouldBe(HttpStatusCode.Created);

        var secondBorrow = await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = book.Id });

        secondBorrow.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ReturnBook_WhenOverdue_CalculatesFine()
    {
        var client = _factory.CreateClient();

        var borrowResponse = await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 2, BookId = 3 });
        var loan = await borrowResponse.Content.ReadFromJsonAsync<LoanDto>();
        loan.ShouldNotBeNull();

        var returnResponse = await client.PutAsJsonAsync($"/api/loans/{loan.Id}/return", new { });

        returnResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await returnResponse.Content.ReadFromJsonAsync<ReturnResultDto>();
        result.ShouldNotBeNull();
        result.LoanId.ShouldBe(loan.Id);
        result.IsOverdue.ShouldBeFalse();
        result.FineAmount.ShouldBe(0);
    }

    [Fact]
    public async Task ReturnBook_WhenNotBorrowed_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/loans/999/return", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMemberLoans_ReturnsLoanHistory()
    {
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 2 });

        var response = await client.GetAsync("/api/loans?memberId=1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var loans = await response.Content.ReadFromJsonAsync<List<LoanDto>>();
        loans.ShouldNotBeNull();
        loans.Count.ShouldBeGreaterThanOrEqualTo(1);
        loans.All(l => l.MemberId == 1).ShouldBeTrue();
    }

    [Fact]
    public async Task FullLoanLifecycle_BorrowThenReturn_Succeeds()
    {
        var client = _factory.CreateClient();

        var memberResponse = await client.GetAsync("/api/members/1");
        memberResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var book = await client.GetFromJsonAsync<BookDto>($"/api/books/2");
        var copiesBeforeBorrow = book!.AvailableCopies;

        var borrowResponse = await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 2 });
        borrowResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var loan = await borrowResponse.Content.ReadFromJsonAsync<LoanDto>();
        loan.ShouldNotBeNull();

        var afterBorrow = await client.GetFromJsonAsync<BookDto>($"/api/books/2");
        afterBorrow!.AvailableCopies.ShouldBe(copiesBeforeBorrow - 1);

        var loansResponse = await client.GetAsync($"/api/loans?memberId={loan.MemberId}");
        var loans = await loansResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
        loans.ShouldNotBeNull();
        loans.Any(l => l.Id == loan.Id && l.ReturnedAt == null).ShouldBeTrue();

        var returnResponse = await client.PutAsJsonAsync($"/api/loans/{loan.Id}/return", new { });
        returnResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await returnResponse.Content.ReadFromJsonAsync<ReturnResultDto>();
        result.ShouldNotBeNull();
        result.LoanId.ShouldBe(loan.Id);

        var afterReturn = await client.GetFromJsonAsync<BookDto>($"/api/books/2");
        afterReturn!.AvailableCopies.ShouldBe(copiesBeforeBorrow);
    }
}
