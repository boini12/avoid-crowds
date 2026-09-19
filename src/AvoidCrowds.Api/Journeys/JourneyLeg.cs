namespace AvoidCrowds.Api.Journeys;

public record JourneyLeg(string Mode, string FromName, string ToName, DateTimeOffset StartTime, DateTimeOffset EndTime);
