namespace AvoidCrowds.Api.SoccerCrowdCheck;

public record Fixture(string HomeTeam, string AwayTeam, DateTimeOffset KickoffTime, string? LocationCity);
