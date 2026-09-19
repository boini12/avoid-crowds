namespace AvoidCrowds.Api.Journeys;

public record TrainDetail(
    string Origin,
    string Destination,
    DateTimeOffset DepartureTime,
    string DepartureTimeZone,
    DateTimeOffset ArrivalTime,
    string ArrivalTimeZone,
    IReadOnlyList<TripStop> Stops);
