using System.Text.Json.Serialization;

namespace AvoidCrowds.Api.SoccerCrowdCheck;

public class OpenLigaDbFixtureClient(HttpClient httpClient) : IFixtureClient
{
    public async Task<IReadOnlyList<Fixture>> GetSeasonFixturesAsync(string league, int season, CancellationToken cancellationToken)
    {
        var matches = await httpClient.GetFromJsonAsync<List<OpenLigaDbMatch>>(
            $"getmatchdata/{league}/{season}",
            cancellationToken);

        return matches?
            .Where(match => match.MatchDateTimeUtc is not null)
            .Select(match => new Fixture(
                match.Team1?.TeamName ?? "",
                match.Team2?.TeamName ?? "",
                match.MatchDateTimeUtc!.Value,
                match.Location?.LocationCity))
            .ToList()
            ?? [];
    }

    private sealed record OpenLigaDbMatch(
        [property: JsonPropertyName("matchDateTimeUTC")] DateTimeOffset? MatchDateTimeUtc,
        [property: JsonPropertyName("team1")] OpenLigaDbTeam? Team1,
        [property: JsonPropertyName("team2")] OpenLigaDbTeam? Team2,
        [property: JsonPropertyName("location")] OpenLigaDbLocation? Location);

    private sealed record OpenLigaDbTeam([property: JsonPropertyName("teamName")] string? TeamName);

    private sealed record OpenLigaDbLocation([property: JsonPropertyName("locationCity")] string? LocationCity);
}
