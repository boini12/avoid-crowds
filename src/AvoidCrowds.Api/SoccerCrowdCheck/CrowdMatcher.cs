using AvoidCrowds.Api.Journeys;

namespace AvoidCrowds.Api.SoccerCrowdCheck;

public static class CrowdMatcher
{
    private static readonly TimeSpan Window = TimeSpan.FromHours(3);

    public static IReadOnlyList<CrowdWarning> Match(IReadOnlyList<TripStop> stops, IEnumerable<Fixture> fixtures)
    {
        // A fixture with no location city (a known OpenLigaDB data-quality gap)
        // can't be tied to a venue city, so it's excluded up front.
        var locatedFixtures = fixtures.Where(fixture => fixture.LocationCity is not null).ToList();

        var warnings = new List<CrowdWarning>();

        foreach (var stop in stops)
        {
            var stopTime = StopTime(stop);
            if (stopTime is null)
            {
                continue;
            }

            foreach (var fixture in locatedFixtures)
            {
                if (IsWithinWindow(stopTime.Value, fixture.KickoffTime)
                    && BundesligaCities.MatchesStop(stop.Name, fixture.LocationCity!))
                {
                    warnings.Add(new CrowdWarning(stop.Name, fixture.HomeTeam, fixture.AwayTeam, fixture.KickoffTime, stop.TimeZone));
                }
            }
        }

        return warnings;
    }

    // Origin has no arrival, destination has no departure; either time works
    // for an intermediate stop's brief dwell against a 3-hour window.
    public static DateTimeOffset? StopTime(TripStop stop) => stop.Arrival ?? stop.Departure;

    private static bool IsWithinWindow(DateTimeOffset stopTime, DateTimeOffset kickoff) => (stopTime - kickoff).Duration() <= Window;
}
