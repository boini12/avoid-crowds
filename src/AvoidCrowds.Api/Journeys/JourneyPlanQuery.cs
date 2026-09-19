namespace AvoidCrowds.Api.Journeys;

public record JourneyPlanQuery(string FromStationId, string ToStationId, DateTimeOffset Time, bool ArriveBy);
