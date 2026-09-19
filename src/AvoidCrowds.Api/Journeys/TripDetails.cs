namespace AvoidCrowds.Api.Journeys;

public record TripDetails(
    DateTimeOffset DepartureTime,
    DateTimeOffset ArrivalTime,
    TripStop From,
    TripStop To,
    IReadOnlyList<TripStop> IntermediateStops);
