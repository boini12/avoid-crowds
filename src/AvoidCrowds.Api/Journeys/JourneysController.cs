using AvoidCrowds.Api.SoccerCrowdCheck;
using Microsoft.AspNetCore.Mvc;

namespace AvoidCrowds.Api.Journeys;

[ApiController]
[Route("api/journeys")]
public class JourneysController(IJourneyPlanClient journeyPlanClient, ICrowdCheckService crowdCheckService) : ControllerBase
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

    [HttpGet("trip")]
    public async Task<IActionResult> GetTrip(string? tripId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tripId))
        {
            return BadRequest();
        }

        var trip = await journeyPlanClient.GetTripAsync(tripId, cancellationToken);
        if (trip is null)
        {
            return NotFound();
        }

        var detail = TrainDetailBuilder.Build(trip);
        var crowdWarnings = await crowdCheckService.CheckAsync(detail.Stops, cancellationToken);

        // TrainDetail itself stays a pure Journeys-domain mapping (like the rest of
        // this file's DTOs); the crowd-check result is folded in only at the response
        // boundary, where this endpoint is already the one place both domains meet.
        return Ok(new
        {
            detail.Origin,
            detail.Destination,
            detail.DepartureTime,
            detail.DepartureTimeZone,
            detail.ArrivalTime,
            detail.ArrivalTimeZone,
            detail.Stops,
            CrowdWarnings = crowdWarnings,
        });
    }
}
