namespace AvoidCrowds.Api.Journeys;

public record TripStop(string Name, DateTimeOffset? Arrival, DateTimeOffset? Departure, string TimeZone)
{
    public DateTimeOffset? StopTime => Arrival ?? Departure;
}
