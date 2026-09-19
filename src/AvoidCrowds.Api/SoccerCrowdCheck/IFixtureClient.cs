namespace AvoidCrowds.Api.SoccerCrowdCheck;

public interface IFixtureClient
{
    Task<IReadOnlyList<Fixture>> GetSeasonFixturesAsync(string league, int season, CancellationToken cancellationToken);
}
