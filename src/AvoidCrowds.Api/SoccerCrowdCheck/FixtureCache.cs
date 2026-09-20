using System.Collections.Concurrent;

namespace AvoidCrowds.Api.SoccerCrowdCheck;

// Holds fetched season fixtures in memory, keyed by league+season, so the
// (expensive, whole-season) OpenLigaDB fetch doesn't repeat on every journey
// lookup. Lazy<> ensures concurrent callers for the same key share one fetch
// rather than firing it multiple times.
public class FixtureCache(IFixtureClient fixtureClient)
{
    private readonly ConcurrentDictionary<(string League, int Season), Lazy<Task<IReadOnlyList<Fixture>>>> _cache = new();

    public async Task<IReadOnlyList<Fixture>> GetSeasonFixturesAsync(string league, int season, CancellationToken cancellationToken)
    {
        var key = (league, season);
        var entry = _cache.GetOrAdd(
            key,
            k => new Lazy<Task<IReadOnlyList<Fixture>>>(() => fixtureClient.GetSeasonFixturesAsync(k.League, k.Season, cancellationToken)));

        try
        {
            return await entry.Value;
        }
        catch
        {
            // Evict a failed fetch so a later retry (e.g. the error banner's
            // retry button) re-fetches instead of staying poisoned.
            _cache.TryRemove(key, out _);
            throw;
        }
    }
}
