using System.Net.Http.Json;
using NBomber.CSharp;
using Shouldly;

namespace LibraryManagement.LoadTests;

public class BorrowScenarioTests
{
    private static readonly HttpClient Client = new() { BaseAddress = new Uri("http://localhost:5041") };

    [Fact]
    public async Task BorrowScenario_should_have_zero_500_errors()
    {
        var borrowBook = Scenario.Create("borrow_book", async ctx =>
        {
            var memberId = Random.Shared.Next(1, 201);
            var bookId = Random.Shared.Next(1, 51);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/loans")
            {
                Content = JsonContent.Create(new { memberId, bookId })
            };

            var response = await Client.SendAsync(request);
            var statusCode = (int)response.StatusCode;

            if (statusCode >= 500)
                return Response.Fail(statusCode: statusCode.ToString(), message: "Server error");

            return Response.Ok(statusCode: statusCode.ToString());
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(60))
        );

        var stats = NBomberRunner
            .RegisterScenarios(borrowBook)
            .WithReportFileName("borrow-scenario-report")
            .WithReportFolder("reports")
            .Run();

        stats.ScenarioStats[0].Fail.Request.Count.ShouldBe(0);
    }
}
