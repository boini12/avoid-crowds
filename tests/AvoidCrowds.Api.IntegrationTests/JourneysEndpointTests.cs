using System.Net;
using System.Net.Http.Json;
using AvoidCrowds.Api.Journeys;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace AvoidCrowds.Api.IntegrationTests;

[TestFixture]
public class JourneysEndpointTests
{
    private static readonly DateTimeOffset AnchorTime = new(2026, 9, 20, 10, 0, 0, TimeSpan.FromHours(2));

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IJourneyPlanClient>();
                services.AddSingleton<IJourneyPlanClient>(new FakeJourneyPlanClient(
                [
                    new JourneyItinerary(
                        Transfers: 0,
                        Legs: [new JourneyLeg("REGIONAL_RAIL", "Berlin Hbf", "Hamburg Hbf", AnchorTime, AnchorTime.AddHours(2))]),
                    new JourneyItinerary(
                        Transfers: 1,
                        Legs:
                        [
                            new JourneyLeg("REGIONAL_RAIL", "Berlin Hbf", "Hannover Hbf", AnchorTime, AnchorTime.AddHours(1)),
                            new JourneyLeg("REGIONAL_RAIL", "Hannover Hbf", "Hamburg Hbf", AnchorTime.AddHours(1), AnchorTime.AddHours(2)),
                        ]),
                ]));
            });
        });
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetJourneys_ReturnsOnlyDirectTrains()
    {
        var response = await _client.GetAsync(
            $"/api/journeys?fromStationId=de:11000:900003201&toStationId=de:02000:10513&time={Uri.EscapeDataString(AnchorTime.ToString("O"))}&arriveBy=false");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var trains = await response.Content.ReadFromJsonAsync<List<DirectTrain>>();

        Assert.That(trains, Has.Count.EqualTo(1));
        Assert.That(trains![0].Origin, Is.EqualTo("Berlin Hbf"));
        Assert.That(trains[0].Destination, Is.EqualTo("Hamburg Hbf"));
    }

    [Test]
    public async Task GetJourneys_MissingStation_ReturnsBadRequest()
    {
        var response = await _client.GetAsync(
            $"/api/journeys?toStationId=de:02000:10513&time={Uri.EscapeDataString(AnchorTime.ToString("O"))}&arriveBy=false");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private sealed class FakeJourneyPlanClient(IReadOnlyList<JourneyItinerary> itineraries) : IJourneyPlanClient
    {
        public Task<IReadOnlyList<JourneyItinerary>> PlanAsync(JourneyPlanQuery query, CancellationToken cancellationToken) =>
            Task.FromResult(itineraries);
    }
}
