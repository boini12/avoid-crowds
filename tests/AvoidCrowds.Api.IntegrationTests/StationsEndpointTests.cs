using System.Net;
using System.Net.Http.Json;
using AvoidCrowds.Api.Geocoding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace AvoidCrowds.Api.IntegrationTests;

[TestFixture]
public class StationsEndpointTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGeocodeClient>();
                services.AddSingleton<IGeocodeClient>(new FakeGeocodeClient(
                [
                    new GeocodeMatch("STOP", "de:11000:900003200", "Berlin Hbf", 52.525, 13.369),
                    new GeocodeMatch("STOP", "node/[260034778]", "Berlin", 43.451, -80.493),
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
    public async Task GetStations_ReturnsOnlyGermanMatches()
    {
        var response = await _client.GetAsync("/api/stations?query=Berlin");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var stations = await response.Content.ReadFromJsonAsync<List<StationSuggestion>>();

        Assert.That(stations, Has.Count.EqualTo(1));
        Assert.That(stations![0].Name, Is.EqualTo("Berlin Hbf"));
    }

    [Test]
    public async Task GetStations_MissingQuery_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/stations");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var stations = await response.Content.ReadFromJsonAsync<List<StationSuggestion>>();

        Assert.That(stations, Is.Empty);
    }

    private sealed class FakeGeocodeClient(IReadOnlyList<GeocodeMatch> matches) : IGeocodeClient
    {
        public Task<IReadOnlyList<GeocodeMatch>> SearchAsync(string query, CancellationToken cancellationToken) =>
            Task.FromResult(matches);
    }
}
