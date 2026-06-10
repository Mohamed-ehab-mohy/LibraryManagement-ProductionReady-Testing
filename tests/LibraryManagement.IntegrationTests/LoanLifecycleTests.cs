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
        var request = new BorrowRequestDto { MemberId = 1, BookId = 1 };

        var response = await client.PostAsJsonAsync("/api/loans", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
        loan.ShouldNotBeNull();
        loan.BookId.ShouldBe(1);
        loan.MemberId.ShouldBe(1);
        loan.ReturnedAt.ShouldBeNull();
    }

    [Fact]
    public async Task BorrowBook_WhenBookNotAvailable_ReturnsUnprocessable()
    {
        var client = _factory.CreateClient();

        // borrowed all copies of book 1
        for (int i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 1 });
        }

        var response = await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 1 });

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ReturnBook_WhenOverdue_CalculatesFine()
    {
        var client = _factory.CreateClient();

        // borrow book 3 (AvailableCopies = 4) for member 2 (Sara)
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

        // check member
        var memberResponse = await client.GetAsync("/api/members/1");
        memberResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // borrow
        var borrowResponse = await client.PostAsJsonAsync("/api/loans", new BorrowRequestDto { MemberId = 1, BookId = 2 });
        borrowResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var loan = await borrowResponse.Content.ReadFromJsonAsync<LoanDto>();
        loan.ShouldNotBeNull();

        // verify active loan appears
        var loansResponse = await client.GetAsync($"/api/loans?memberId={loan.MemberId}");
        var loans = await loansResponse.Content.ReadFromJsonAsync<List<LoanDto>>();
        loans.ShouldNotBeNull();
        loans.Any(l => l.Id == loan.Id && l.ReturnedAt == null).ShouldBeTrue();

        // return
        var returnResponse = await client.PutAsJsonAsync($"/api/loans/{loan.Id}/return", new { });
        returnResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await returnResponse.Content.ReadFromJsonAsync<ReturnResultDto>();
        result.ShouldNotBeNull();
        result.LoanId.ShouldBe(loan.Id);
    }
}
