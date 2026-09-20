namespace AvoidCrowds.Api.SoccerCrowdCheck;

public record CrowdWarning(string StopName, string HomeTeam, string AwayTeam, DateTimeOffset KickoffTime, string TimeZone);
