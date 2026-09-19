using AvoidCrowds.Api.Journeys;

namespace AvoidCrowds.Api.SoccerCrowdCheck;

public class CrowdCheckService(FixtureCache fixtureCache) : ICrowdCheckService
{
    public async Task<IReadOnlyList<CrowdWarning>> CheckAsync(IReadOnlyList<TripStop> stops, CancellationToken cancellationToken)
    {
        var seasons = stops
            .Select(CrowdMatcher.StopTime)
            .Where(time => time is not null)
            .Select(time => BundesligaSeason.ForDate(time!.Value))
            .Distinct()
            .ToList();

        var fixtureLists = await Task.WhenAll(
            seasons.SelectMany(season => Leagues.Bundesliga1And2
                .Select(league => fixtureCache.GetSeasonFixturesAsync(league, season, cancellationToken))));

        return CrowdMatcher.Match(stops, fixtureLists.SelectMany(fixtures => fixtures));
    }
}
