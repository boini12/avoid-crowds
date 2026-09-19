using Microsoft.AspNetCore.Mvc;

namespace AvoidCrowds.Api.Journeys;

[ApiController]
[Route("api/journeys")]
public class JourneysController(IJourneyPlanClient journeyPlanClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        string? fromStationId,
        string? toStationId,
        DateTimeOffset? time,
        bool? arriveBy,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fromStationId) || string.IsNullOrWhiteSpace(toStationId) || time is null)
        {
            return BadRequest();
        }

        var query = new JourneyPlanQuery(fromStationId, toStationId, time.Value, arriveBy ?? false);
        var itineraries = await journeyPlanClient.PlanAsync(query, cancellationToken);
        return Ok(DirectTrainFilter.Filter(itineraries, query.ArriveBy, query.Time));
    }
}
