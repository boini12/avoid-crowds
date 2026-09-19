using System.Net;
using System.Net.Http.Json;
using AvoidCrowds.Api.Journeys;
using AvoidCrowds.Api.SoccerCrowdCheck;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace AvoidCrowds.Api.IntegrationTests;

[TestFixture]
public class CrowdCheckEndpointTests
{
    private static readonly DateTimeOffset Departure = new(2026, 9, 20, 10, 0, 0, TimeSpan.FromHours(2));
    private static readonly DateTimeOffset Kickoff = new(2026, 9, 20, 12, 30, 0, TimeSpan.FromHours(2));
    private static readonly DateTimeOffset Arrival = new(2026, 9, 20, 14, 0, 0, TimeSpan.FromHours(2));
    private const string TripIdWithMatch = "trip-with-match";
    private const string TripIdWithoutMatch = "trip-without-match";

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var trips = new Dictionary<string, TripDetails>
        {
            [TripIdWithMatch] = new(
                Departure,
                Arrival,
                new TripStop("Frankfurt (Main) Hbf", null, Departure, "Europe/Berlin"),
                new TripStop("Hamburg Hbf", Arrival, null, "Europe/Berlin"),
                []),
            [TripIdWithoutMatch] = new(
                Departure,
                Arrival,
                new TripStop("Hannover Hbf", null, Departure, "Europe/Berlin"),
                new TripStop("Bielefeld Hbf", Arrival, null, "Europe/Berlin"),
                []),
        };

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IJourneyPlanClient>();
                services.AddSingleton<IJourneyPlanClient>(new FakeJourneyPlanClient(trips));

                services.RemoveAll<IFixtureClient>();
                services.AddSingleton<IFixtureClient>(new FakeFixtureClient(
                    [new Fixture("Eintracht Frankfurt", "Bayern München", Kickoff, "Frankfurt am Main")]));
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
    public async Task GetTrip_StopNearMatchingFixture_ReturnsCrowdWarning()
    {
        var response = await _client.GetAsync($"/api/journeys/trip?tripId={TripIdWithMatch}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadFromJsonAsync<CrowdCheckResponse>();

        Assert.That(body!.CrowdWarnings, Has.Count.EqualTo(1));
        Assert.That(body.CrowdWarnings[0].StopName, Is.EqualTo("Frankfurt (Main) Hbf"));
        Assert.That(body.CrowdWarnings[0].HomeTeam, Is.EqualTo("Eintracht Frankfurt"));
        Assert.That(body.CrowdWarnings[0].AwayTeam, Is.EqualTo("Bayern München"));
    }

    [Test]
    public async Task GetTrip_NoStopNearMatchingFixture_ReturnsNoCrowdWarnings()
    {
        var response = await _client.GetAsync($"/api/journeys/trip?tripId={TripIdWithoutMatch}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var body = await response.Content.ReadFromJsonAsync<CrowdCheckResponse>();

        Assert.That(body!.CrowdWarnings, Is.Empty);
    }

    [Test]
    public async Task GetTrip_FixtureFetchFails_ReturnsServerErrorInsteadOfPartialDetail()
    {
        const string tripId = "trip-with-failing-fetch";
        var trip = new TripDetails(
            Departure,
            Arrival,
            new TripStop("Frankfurt (Main) Hbf", null, Departure, "Europe/Berlin"),
            new TripStop("Hamburg Hbf", Arrival, null, "Europe/Berlin"),
            []);

        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IJourneyPlanClient>();
                services.AddSingleton<IJourneyPlanClient>(new FakeJourneyPlanClient(
                    new Dictionary<string, TripDetails> { [tripId] = trip }));

                services.RemoveAll<IFixtureClient>();
                services.AddSingleton<IFixtureClient>(new AlwaysFailingFixtureClient());
            });
        });
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/journeys/trip?tripId={tripId}");

        Assert.That((int)response.StatusCode, Is.GreaterThanOrEqualTo(500));
    }

    private sealed class FakeJourneyPlanClient(IReadOnlyDictionary<string, TripDetails> trips) : IJourneyPlanClient
    {
        public Task<IReadOnlyList<JourneyItinerary>> PlanAsync(JourneyPlanQuery query, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<JourneyItinerary>>([]);

        public Task<TripDetails?> GetTripAsync(string tripId, CancellationToken cancellationToken) =>
            Task.FromResult(trips.GetValueOrDefault(tripId));
    }

    private sealed class FakeFixtureClient(IReadOnlyList<Fixture> fixtures) : IFixtureClient
    {
        // The crowd check fetches both bl1 and bl2; only answer for one league
        // so the fake fixture isn't double-counted as if it played in both.
        public Task<IReadOnlyList<Fixture>> GetSeasonFixturesAsync(string league, int season, CancellationToken cancellationToken) =>
            Task.FromResult(league == "bl1" ? fixtures : (IReadOnlyList<Fixture>)[]);
    }

    private sealed class AlwaysFailingFixtureClient : IFixtureClient
    {
        public Task<IReadOnlyList<Fixture>> GetSeasonFixturesAsync(string league, int season, CancellationToken cancellationToken) =>
            throw new HttpRequestException("OpenLigaDB unavailable");
    }

    // Mirrors only the field this test file cares about from the endpoint's
    // flat response shape (TrainDetail's fields plus crowdWarnings).
    private sealed record CrowdCheckResponse(IReadOnlyList<CrowdWarning> CrowdWarnings);
}
