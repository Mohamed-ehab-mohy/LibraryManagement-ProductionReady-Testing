using NBomber.CSharp;
using NBomber.Http.CSharp;
using Shouldly;

namespace LibraryManagement.LoadTests;

public class ReadScenariosTests
{
    [Fact]
    public async Task ReadScenarios_should_meet_latency_targets()
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5041") };

        var listAllBooks = Scenario.Create("list_all_books", async ctx =>
        {
            using var request = Http.CreateRequest("GET", "/api/books");
            var response = await Http.Send(httpClient, request);

            return response.IsError
                ? Response.Fail()
                : Response.Ok();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var getSingleBook = Scenario.Create("get_single_book", async ctx =>
        {
            var bookId = Random.Shared.Next(1, 51);
            using var request = Http.CreateRequest("GET", $"/api/books/{bookId}");
            var response = await Http.Send(httpClient, request);

            return response.IsError
                ? Response.Fail()
                : Response.Ok();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var stats = NBomberRunner
            .RegisterScenarios(listAllBooks, getSingleBook)
            .WithReportFileName("read-scenarios-report")
            .WithReportFolder("reports")
            .Run();

        foreach (var scenario in stats.ScenarioStats)
        {
            scenario.Fail.Request.Count.ShouldBe(0);
            scenario.Ok.Latency.Percent99.ShouldBeLessThan(300);
        }
    }
}
