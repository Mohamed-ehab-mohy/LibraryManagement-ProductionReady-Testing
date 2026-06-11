using NBomber.CSharp;
using Shouldly;

namespace LibraryManagement.LoadTests;

public class ReadScenariosTests
{
    private static readonly HttpClient Client = new() { BaseAddress = new Uri("http://localhost:5041") };

    private static async Task WarmUpAsync()
    {
        var requests = new[] { "/api/books", "/api/books/1", "/api/books?available=true" };
        foreach (var path in requests)
        {
            try { await Client.GetAsync(path); } catch { }
        }
    }

    [Fact]
    public async Task ReadScenarios_should_meet_latency_targets()
    {
        await WarmUpAsync();

        var listAllBooks = Scenario.Create("list_all_books", async ctx =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/books");
            var response = await Client.SendAsync(request);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
        })
        .WithLoadSimulations(
            Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
        );

        var getSingleBook = Scenario.Create("get_single_book", async ctx =>
        {
            var bookId = Random.Shared.Next(1, 11);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/books/{bookId}");
            var response = await Client.SendAsync(request);

            return response.IsSuccessStatusCode
                ? Response.Ok()
                : Response.Fail();
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
            scenario.Ok.Latency.Percent99.ShouldBeLessThan(2000);
        }
    }
}
