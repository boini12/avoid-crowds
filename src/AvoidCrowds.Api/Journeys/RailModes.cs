namespace AvoidCrowds.Api.Journeys;

public static class RailModes
{
    // transitous's RAIL mode family bundles SUBURBAN (S-Bahn) in with actual
    // trains; SUBWAY is separate. v1 excludes both, per design-decisions.md.
    public static readonly IReadOnlyList<string> Allowed =
    [
        "HIGHSPEED_RAIL",
        "LONG_DISTANCE",
        "NIGHT_RAIL",
        "REGIONAL_FAST_RAIL",
        "REGIONAL_RAIL",
    ];
}
