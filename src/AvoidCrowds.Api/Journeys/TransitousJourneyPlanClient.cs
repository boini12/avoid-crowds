using System.Text.Json.Serialization;

namespace AvoidCrowds.Api.Journeys;

public class TransitousJourneyPlanClient(HttpClient httpClient) : IJourneyPlanClient
{
    private const int NumItineraries = 5;

    public async Task<IReadOnlyList<JourneyItinerary>> PlanAsync(JourneyPlanQuery query, CancellationToken cancellationToken)
    {
        var modes = string.Join(',', RailModes.Allowed);
        var url = "api/v6/plan"
            + $"?fromPlace={Uri.EscapeDataString(query.FromStationId)}"
            + $"&toPlace={Uri.EscapeDataString(query.ToStationId)}"
            + $"&time={Uri.EscapeDataString(query.Time.ToString("O"))}"
            + $"&arriveBy={(query.ArriveBy ? "true" : "false")}"
            + "&maxTransfers=0"
            + $"&transitModes={modes}"
            + $"&numItineraries={NumItineraries}"
            + $"&searchWindow={AnchorDaySearchWindowSeconds(query)}";

        var response = await httpClient.GetFromJsonAsync<TransitousPlanResponse>(url, cancellationToken);

        return response?.Itineraries?
            .Select(itinerary => new JourneyItinerary(
                itinerary.Transfers,
                itinerary.Legs
                    .Select(leg => new JourneyLeg(leg.Mode, leg.From.Name, leg.To.Name, leg.StartTime, leg.EndTime))
                    .ToList()))
            .ToList()
            ?? [];
    }

    // transitous defaults searchWindow to 900s (15 min), which under-returns
    // same-day trains long before the 5-result target is reached. Bound the
    // window to exactly the anchor calendar day (in the query time's own
    // offset) instead. v1 deliberately does not roll into adjacent days, even
    // if fewer than 5 direct trains are found — see design-decisions.md.
    private static int AnchorDaySearchWindowSeconds(JourneyPlanQuery query)
    {
        var anchorDayStart = new DateTimeOffset(query.Time.Date, query.Time.Offset);
        var window = query.ArriveBy
            ? query.Time - anchorDayStart
            : anchorDayStart.AddDays(1) - query.Time;

        return (int)Math.Max(window.TotalSeconds, 60);
    }

    private sealed record TransitousPlanResponse(
        [property: JsonPropertyName("itineraries")] List<TransitousItinerary>? Itineraries);

    private sealed record TransitousItinerary(
        [property: JsonPropertyName("transfers")] int Transfers,
        [property: JsonPropertyName("legs")] List<TransitousLeg> Legs);

    private sealed record TransitousLeg(
        [property: JsonPropertyName("mode")] string Mode,
        [property: JsonPropertyName("startTime")] DateTimeOffset StartTime,
        [property: JsonPropertyName("endTime")] DateTimeOffset EndTime,
        [property: JsonPropertyName("from")] TransitousPlace From,
        [property: JsonPropertyName("to")] TransitousPlace To);

    private sealed record TransitousPlace([property: JsonPropertyName("name")] string Name);
}
