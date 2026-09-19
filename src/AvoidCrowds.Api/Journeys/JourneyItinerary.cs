namespace AvoidCrowds.Api.Journeys;

public record JourneyItinerary(int Transfers, IReadOnlyList<JourneyLeg> Legs);
