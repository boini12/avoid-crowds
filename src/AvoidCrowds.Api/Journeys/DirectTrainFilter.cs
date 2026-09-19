namespace AvoidCrowds.Api.Journeys;

public static class DirectTrainFilter
{
    private const int MaxResults = 5;

    public static IReadOnlyList<DirectTrain> Filter(
        IEnumerable<JourneyItinerary> itineraries,
        bool arriveBy,
        DateTimeOffset boundary)
    {
        var candidates = itineraries
            .Where(IsDirectRailJourney)
            .Select(itinerary => itinerary.Legs[0])
            .Where(leg => RespectsBoundary(leg, arriveBy, boundary));

        // Walk backward from the boundary for arrive-by (keep the latest arrivals),
        // forward for depart-after (keep the earliest departures) — then always
        // display chronologically.
        var closestToBoundary = arriveBy
            ? candidates.OrderByDescending(leg => leg.EndTime)
            : candidates.OrderBy(leg => leg.StartTime);

        return closestToBoundary
            .Take(MaxResults)
            .OrderBy(leg => leg.StartTime)
            .Select(leg => new DirectTrain(leg.FromName, leg.ToName, leg.StartTime))
            .ToList();
    }

    private static bool IsDirectRailJourney(JourneyItinerary itinerary) =>
        itinerary.Transfers == 0
        && itinerary.Legs.Count == 1
        && RailModes.Allowed.Contains(itinerary.Legs[0].Mode);

    private static bool RespectsBoundary(JourneyLeg leg, bool arriveBy, DateTimeOffset boundary) =>
        arriveBy ? leg.EndTime <= boundary : leg.StartTime >= boundary;
}
