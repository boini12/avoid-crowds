namespace AvoidCrowds.Api.Journeys;

public static class TrainDetailBuilder
{
    public static TrainDetail Build(TripDetails trip)
    {
        var stops = new List<TripStop>(trip.IntermediateStops.Count + 2) { trip.From };
        stops.AddRange(trip.IntermediateStops);
        stops.Add(trip.To);

        return new TrainDetail(
            trip.From.Name,
            trip.To.Name,
            trip.DepartureTime,
            trip.From.TimeZone,
            trip.ArrivalTime,
            trip.To.TimeZone,
            stops);
    }
}
