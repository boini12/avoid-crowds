namespace AvoidCrowds.Api.SoccerCrowdCheck;

public record CityDefinition(
    string Name,
    IReadOnlyList<string> FixtureCityNames,
    IReadOnlyList<string> HomeTeamNames,
    IReadOnlyList<string> StationNameAliases)
{
    public bool MatchesStop(string stopName) =>
        StationNameAliases.Any(alias => stopName.Contains(alias, StringComparison.OrdinalIgnoreCase));
}
