namespace AvoidCrowds.Api.Geocoding;

public interface IGeocodeClient
{
    Task<IReadOnlyList<GeocodeMatch>> SearchAsync(string query, CancellationToken cancellationToken);
}
