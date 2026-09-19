namespace AvoidCrowds.Api.Geocoding;

public static class GermanStationFilter
{
    // Germany's bounding box, per design-decisions.md — transitous's geocode
    // results carry no country field to filter on directly.
    private const double MinLat = 47.3;
    private const double MaxLat = 55.1;
    private const double MinLon = 5.9;
    private const double MaxLon = 15.0;

    public static IReadOnlyList<StationSuggestion> Filter(IEnumerable<GeocodeMatch> matches)
    {
        return matches
            .Where(match => match.Type == "STOP")
            .Where(IsInsideGermany)
            .Select(match => new StationSuggestion(match.Id, match.Name, match.Lat, match.Lon))
            .ToList();
    }

    private static bool IsInsideGermany(GeocodeMatch match) =>
        match.Lat is >= MinLat and <= MaxLat
        && match.Lon is >= MinLon and <= MaxLon;
}
