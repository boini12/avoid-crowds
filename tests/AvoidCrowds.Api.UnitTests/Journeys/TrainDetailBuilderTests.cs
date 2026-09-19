using AvoidCrowds.Api.Journeys;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.Journeys;

[TestFixture]
public class TrainDetailBuilderTests
{
    private static readonly DateTimeOffset Departure = new(2026, 9, 20, 10, 0, 0, TimeSpan.FromHours(2));
    private static readonly DateTimeOffset Arrival = new(2026, 9, 20, 12, 0, 0, TimeSpan.FromHours(2));

    [Test]
    public void Build_WithIntermediateStops_ListsOriginThenIntermediatesThenDestination()
    {
        // Arrange
        var origin = new TripStop("Berlin Hbf", null, Departure, "Europe/Berlin");
        var intermediate = new TripStop("Wittenberge", Departure.AddMinutes(50), Departure.AddMinutes(52), "Europe/Berlin");
        var destination = new TripStop("Prague hl.n.", Arrival, null, "Europe/Prague");
        var trip = new TripDetails(Departure, Arrival, origin, destination, [intermediate]);

        // Act
        var result = TrainDetailBuilder.Build(trip);

        // Assert
        Assert.That(result.Origin, Is.EqualTo("Berlin Hbf"));
        Assert.That(result.Destination, Is.EqualTo("Prague hl.n."));
        Assert.That(result.DepartureTime, Is.EqualTo(Departure));
        Assert.That(result.DepartureTimeZone, Is.EqualTo("Europe/Berlin"));
        Assert.That(result.ArrivalTime, Is.EqualTo(Arrival));
        Assert.That(result.ArrivalTimeZone, Is.EqualTo("Europe/Prague"));
        Assert.That(result.Stops.Select(stop => stop.Name), Is.EqualTo(new[] { "Berlin Hbf", "Wittenberge", "Prague hl.n." }));
    }

    [Test]
    public void Build_WithNoIntermediateStops_ListsOnlyOriginAndDestination()
    {
        // Arrange
        var origin = new TripStop("Berlin Hbf", null, Departure, "Europe/Berlin");
        var destination = new TripStop("Hamburg Hbf", Arrival, null, "Europe/Berlin");
        var trip = new TripDetails(Departure, Arrival, origin, destination, []);

        // Act
        var result = TrainDetailBuilder.Build(trip);

        // Assert
        Assert.That(result.Stops.Select(stop => stop.Name), Is.EqualTo(new[] { "Berlin Hbf", "Hamburg Hbf" }));
    }

    [Test]
    public void Build_EachStopKeepsItsOwnArrivalDepartureAndTimeZone()
    {
        // Arrange
        var origin = new TripStop("Berlin Hbf", null, Departure, "Europe/Berlin");
        var intermediate = new TripStop("Prague hl.n.", Departure.AddHours(1).ToOffset(TimeSpan.FromHours(1)), Departure.AddHours(1).AddMinutes(5).ToOffset(TimeSpan.FromHours(1)), "Europe/Prague");
        var destination = new TripStop("Hamburg Hbf", Arrival, null, "Europe/Berlin");
        var trip = new TripDetails(Departure, Arrival, origin, destination, [intermediate]);

        // Act
        var result = TrainDetailBuilder.Build(trip);

        // Assert
        var stop = result.Stops[1];
        Assert.That(stop.TimeZone, Is.EqualTo("Europe/Prague"));
        Assert.That(stop.Arrival, Is.EqualTo(intermediate.Arrival));
        Assert.That(stop.Departure, Is.EqualTo(intermediate.Departure));
    }

    [Test]
    public void Build_OriginHasNoArrival_DestinationHasNoDeparture()
    {
        // Arrange
        var origin = new TripStop("Berlin Hbf", null, Departure, "Europe/Berlin");
        var destination = new TripStop("Hamburg Hbf", Arrival, null, "Europe/Berlin");
        var trip = new TripDetails(Departure, Arrival, origin, destination, []);

        // Act
        var result = TrainDetailBuilder.Build(trip);

        // Assert
        Assert.That(result.Stops[0].Arrival, Is.Null);
        Assert.That(result.Stops[^1].Departure, Is.Null);
    }
}
