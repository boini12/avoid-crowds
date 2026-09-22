using System.Globalization;
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
            + $"&time={Uri.EscapeDataString(TransitousTime(query.Time))}"
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
                    .Select(leg => new JourneyLeg(leg.Mode, leg.From.Name, leg.To.Name, leg.StartTime, leg.EndTime, leg.TripId))
                    .ToList()))
            .ToList()
            ?? [];
    }

    public async Task<TripDetails?> GetTripAsync(string tripId, CancellationToken cancellationToken)
    {
        var url = $"api/v6/trip?tripId={Uri.EscapeDataString(tripId)}";
        var itinerary = await httpClient.GetFromJsonAsync<TransitousItinerary>(url, cancellationToken);
        var leg = itinerary?.Legs.FirstOrDefault();

        if (leg is null)
        {
            return null;
        }

        return new TripDetails(
            leg.StartTime,
            leg.EndTime,
            ToTripStop(leg.From),
            ToTripStop(leg.To),
            (leg.IntermediateStops ?? []).Select(ToTripStop).ToList());
    }

    // transitous misreads the round-trip "O" format's fractional seconds
    // (e.g. 19:00:00.0000000+02:00) and drops the offset, treating the time as
    // UTC - which silently skipped two hours of departures. Plain UTC seconds
    // are unambiguous.
    private static string TransitousTime(DateTimeOffset time) =>
        time.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    private static TripStop ToTripStop(TransitousPlace place) =>
        new(place.Name, place.Arrival, place.Departure, place.Tz ?? "");

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
        [property: JsonPropertyName("to")] TransitousPlace To,
        [property: JsonPropertyName("tripId")] string TripId = "",
        [property: JsonPropertyName("intermediateStops")] List<TransitousPlace>? IntermediateStops = null);

    private sealed record TransitousPlace(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("tz")] string? Tz = null,
        [property: JsonPropertyName("arrival")] DateTimeOffset? Arrival = null,
        [property: JsonPropertyName("departure")] DateTimeOffset? Departure = null);
}
