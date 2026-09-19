using System.Net;
using System.Net.Http.Json;
using AvoidCrowds.Api.Journeys;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace AvoidCrowds.Api.IntegrationTests;

[TestFixture]
public class JourneyTripEndpointTests
{
    private static readonly DateTimeOffset Departure = new(2026, 9, 20, 10, 0, 0, TimeSpan.FromHours(2));
    private static readonly DateTimeOffset Arrival = new(2026, 9, 20, 12, 0, 0, TimeSpan.FromHours(2));
    private const string KnownTripId = "known-trip-id";

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
                    new TripDetails(
                        Departure,
                        Arrival,
                        new TripStop("Berlin Hbf", null, Departure, "Europe/Berlin"),
                        new TripStop("Hamburg Hbf", Arrival, null, "Europe/Berlin"),
                        [new TripStop("Wittenberge", Departure.AddMinutes(50), Departure.AddMinutes(52), "Europe/Berlin")])));
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
    public async Task GetTrip_KnownTripId_ReturnsTrainDetailWithAllStops()
    {
        var response = await _client.GetAsync($"/api/journeys/trip?tripId={KnownTripId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var detail = await response.Content.ReadFromJsonAsync<TrainDetail>();

        Assert.That(detail!.Origin, Is.EqualTo("Berlin Hbf"));
        Assert.That(detail.Destination, Is.EqualTo("Hamburg Hbf"));
        Assert.That(detail.Stops.Select(stop => stop.Name), Is.EqualTo(new[] { "Berlin Hbf", "Wittenberge", "Hamburg Hbf" }));
    }

    [Test]
    public async Task GetTrip_UnknownTripId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/journeys/trip?tripId=unknown-trip-id");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetTrip_MissingTripId_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/journeys/trip");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    private sealed class FakeJourneyPlanClient(TripDetails knownTrip) : IJourneyPlanClient
    {
        public Task<IReadOnlyList<JourneyItinerary>> PlanAsync(JourneyPlanQuery query, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<JourneyItinerary>>([]);

        public Task<TripDetails?> GetTripAsync(string tripId, CancellationToken cancellationToken) =>
            Task.FromResult(tripId == KnownTripId ? knownTrip : null);
    }
}
