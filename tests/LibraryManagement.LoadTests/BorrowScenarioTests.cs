using NBomber.CSharp;
using NBomber.Http.CSharp;
using Shouldly;

namespace LibraryManagement.LoadTests;

public class BorrowScenarioTests
{
    [Fact]
    public async Task BorrowScenario_should_have_zero_500_errors()
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5041") };

        var borrowBook = Scenario.Create("borrow_book", async ctx =>
        {
            var memberId = Random.Shared.Next(1, 201);
            var bookId = Random.Shared.Next(1, 51);

            var body = new { memberId, bookId };
            using var request = Http.CreateRequest("POST", "/api/loans")
                .WithJsonBody(body);

            var response = await Http.Send(httpClient, request);

            if (response.IsError)
                return Response.Fail(statusCode: response.StatusCode, message: response.Message);

            var httpResponse = response.Payload.Value;
            var statusCode = (int)httpResponse.StatusCode;

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
