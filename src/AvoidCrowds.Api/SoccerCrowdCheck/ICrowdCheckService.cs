using AvoidCrowds.Api.Journeys;

namespace AvoidCrowds.Api.SoccerCrowdCheck;

public interface ICrowdCheckService
{
    Task<IReadOnlyList<CrowdWarning>> CheckAsync(IReadOnlyList<TripStop> stops, CancellationToken cancellationToken);
}
