namespace AvoidCrowds.Api.SoccerCrowdCheck;

public record CityDefinition(
    string Name,
    IReadOnlyList<string> FixtureCityNames,
    IReadOnlyList<string> HomeTeamNames,
    IReadOnlyList<string> StationNameAliases);
