using System.Net;
using System.Net.Http.Json;
using Shouldly;
using LibraryManagement.IntegrationTests.Factories;
using LibraryManagement.Services.DTOs;

namespace LibraryManagement.IntegrationTests;

public class LoanLifecycleTests : IntegrationTestBase
{
    public LoanLifecycleTests(LibraryWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task BorrowBook_WithValidRequest_CreatesLoan()
    {
        var book = await Client.GetFromJsonAsync<List<BookDto>>("/api/books");
        var startingCopies = book!.First(b => b.Id == 3).AvailableCopies;

        var response = await Client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 3 });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
        loan.ShouldNotBeNull();
        loan.BookId.ShouldBe(3);
        loan.MemberId.ShouldBe(1);
        loan.ReturnedAt.ShouldBeNull();

        var updated = await Client.GetFromJsonAsync<BookDto>("/api/books/3");
        updated!.AvailableCopies.ShouldBe(startingCopies - 1);
    }

    [Fact]
    public async Task BorrowBook_WhenBookNotAvailable_ReturnsUnprocessable()
    {
        var created = await Client.PostAsJsonAsync("/api/books", new CreateBookDto
        {
            Title = "Single Copy Book",
            Author = "Author",
            ISBN = "1000000000001",
            TotalCopies = 1
        });
        var book = await created.Content.ReadFromJsonAsync<BookDto>();

        var firstBorrow = await Client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = book!.Id });
        firstBorrow.StatusCode.ShouldBe(HttpStatusCode.Created);

        var secondBorrow = await Client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = book.Id });

        secondBorrow.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ReturnBook_WhenOverdue_CalculatesFine()
    {
        var borrowResponse = await Client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 2, BookId = 3 });
        var loan = await borrowResponse.Content.ReadFromJsonAsync<LoanDto>();
        loan.ShouldNotBeNull();

        var returnResponse = await Client.PutAsJsonAsync($"/api/loans/{loan.Id}/return", new { });

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
        var response = await Client.PutAsJsonAsync("/api/loans/999/return", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMemberLoans_ReturnsLoanHistory()
    {
        await Client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 2 });

        var response = await Client.GetAsync("/api/loans?memberId=1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var loans = await response.Content.ReadFromJsonAsync<List<LoanDto>>();
        loans.ShouldNotBeNull();
        loans.Count.ShouldBeGreaterThanOrEqualTo(1);
        loans.All(l => l.MemberId == 1).ShouldBeTrue();
    }

    [Fact]
    public async Task FullLoanLifecycle_BorrowThenReturn_Succeeds()
    {
        var memberResponse = await Client.GetAsync("/api/members/1");
        memberResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var book = await Client.GetFromJsonAsync<BookDto>("/api/books/2");
        var copiesBeforeBorrow = book!.AvailableCopies;

        var borrowResponse = await Client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 2 });
        borrowResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var loan = await borrowResponse.Content.ReadFromJsonAsync<LoanDto>();
        loan.ShouldNotBeNull();

        var afterBorrow = await Client.GetFromJsonAsync<BookDto>("/api/books/2");
        afterBorrow!.AvailableCopies.ShouldBe(copiesBeforeBorrow - 1);

        var loansResponse = await Client.GetAsync($"/api/loans?memberId={loan.MemberId}");
        var loans = await loansResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
        loans.ShouldNotBeNull();
        loans.Any(l => l.Id == loan.Id && l.ReturnedAt == null).ShouldBeTrue();

        var returnResponse = await Client.PutAsJsonAsync($"/api/loans/{loan.Id}/return", new { });
        returnResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await returnResponse.Content.ReadFromJsonAsync<ReturnResultDto>();
        result.ShouldNotBeNull();
        result.LoanId.ShouldBe(loan.Id);

        var afterReturn = await Client.GetFromJsonAsync<BookDto>("/api/books/2");
        afterReturn!.AvailableCopies.ShouldBe(copiesBeforeBorrow);
    }
}
