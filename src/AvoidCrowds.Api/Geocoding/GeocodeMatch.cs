namespace AvoidCrowds.Api.Geocoding;

public record GeocodeMatch(string Type, string Id, string Name, double Lat, double Lon);
