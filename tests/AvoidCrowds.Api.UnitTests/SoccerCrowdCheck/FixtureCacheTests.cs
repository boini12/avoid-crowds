using AvoidCrowds.Api.SoccerCrowdCheck;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.SoccerCrowdCheck;

[TestFixture]
public class FixtureCacheTests
{
    [Test]
    public async Task GetSeasonFixturesAsync_CalledTwiceForSameLeagueAndSeason_FetchesOnlyOnce()
    {
        // Arrange
        var client = new CountingFixtureClient();
        var cache = new FixtureCache(client);

        // Act
        await cache.GetSeasonFixturesAsync("bl1", 2025, CancellationToken.None);
        await cache.GetSeasonFixturesAsync("bl1", 2025, CancellationToken.None);

        // Assert
        Assert.That(client.CallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetSeasonFixturesAsync_DifferentLeaguesOrSeasons_FetchesSeparately()
    {
        // Arrange
        var client = new CountingFixtureClient();
        var cache = new FixtureCache(client);

        // Act
        await cache.GetSeasonFixturesAsync("bl1", 2025, CancellationToken.None);
        await cache.GetSeasonFixturesAsync("bl2", 2025, CancellationToken.None);
        await cache.GetSeasonFixturesAsync("bl1", 2024, CancellationToken.None);

        // Assert
        Assert.That(client.CallCount, Is.EqualTo(3));
    }

    [Test]
    public void GetSeasonFixturesAsync_ClientFails_ExceptionPropagatesAndIsNotCached()
    {
        // Arrange
        var client = new FailingFixtureClient();
        var cache = new FixtureCache(client);

        // Act / Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => cache.GetSeasonFixturesAsync("bl1", 2025, CancellationToken.None));
        Assert.That(client.CallCount, Is.EqualTo(1));

        // A retry after the failure should attempt the fetch again, not stay poisoned.
        Assert.ThrowsAsync<InvalidOperationException>(() => cache.GetSeasonFixturesAsync("bl1", 2025, CancellationToken.None));
        Assert.That(client.CallCount, Is.EqualTo(2));
    }

    private sealed class CountingFixtureClient : IFixtureClient
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<Fixture>> GetSeasonFixturesAsync(string league, int season, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult<IReadOnlyList<Fixture>>([]);
        }
    }

    private sealed class FailingFixtureClient : IFixtureClient
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<Fixture>> GetSeasonFixturesAsync(string league, int season, CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("OpenLigaDB unavailable");
        }
    }
}
