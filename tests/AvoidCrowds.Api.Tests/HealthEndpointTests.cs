using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AvoidCrowds.Api.Tests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_ReturnsOkStatusPayload()
    {
        var client = _factory.CreateClient();

        var payload = await client.GetFromJsonAsync<HealthResponse>("/api/health");

        Assert.NotNull(payload);
        Assert.Equal("ok", payload!.Status);
    }

    private sealed record HealthResponse(string Status);
}
