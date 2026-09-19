using System.Text.Json.Serialization;

namespace AvoidCrowds.Api.Geocoding;

public class TransitousGeocodeClient(HttpClient httpClient) : IGeocodeClient
{
    public async Task<IReadOnlyList<GeocodeMatch>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var matches = await httpClient.GetFromJsonAsync<List<TransitousMatch>>(
            $"api/v1/geocode?text={Uri.EscapeDataString(query)}",
            cancellationToken);

        return matches?
            .Select(match => new GeocodeMatch(match.Type, match.Id, match.Name, match.Lat, match.Lon))
            .ToList()
            ?? [];
    }

    private sealed record TransitousMatch(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("lat")] double Lat,
        [property: JsonPropertyName("lon")] double Lon);
}
