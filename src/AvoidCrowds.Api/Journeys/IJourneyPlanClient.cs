namespace AvoidCrowds.Api.Journeys;

public interface IJourneyPlanClient
{
    Task<IReadOnlyList<JourneyItinerary>> PlanAsync(JourneyPlanQuery query, CancellationToken cancellationToken);

    Task<TripDetails?> GetTripAsync(string tripId, CancellationToken cancellationToken);
}
