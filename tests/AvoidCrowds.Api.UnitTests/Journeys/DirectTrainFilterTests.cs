using AvoidCrowds.Api.Journeys;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.Journeys;

[TestFixture]
public class DirectTrainFilterTests
{
    private static readonly DateTimeOffset Boundary = new(2026, 9, 20, 10, 0, 0, TimeSpan.FromHours(2));

    [Test]
    public void Filter_DirectRailItineraryAtBoundary_IsIncludedAndMapped()
    {
        // Arrange
        var itineraries = new[]
        {
            new JourneyItinerary(
                Transfers: 0,
                Legs:
                [
                    new JourneyLeg(
                        "REGIONAL_RAIL",
                        "Berlin Hbf",
                        "Hamburg Hbf",
                        Boundary,
                        Boundary.AddHours(2),
                        "trip-1"),
                ]),
        };

        // Act
        var result = DirectTrainFilter.Filter(itineraries, arriveBy: false, Boundary);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Origin, Is.EqualTo("Berlin Hbf"));
        Assert.That(result[0].Destination, Is.EqualTo("Hamburg Hbf"));
        Assert.That(result[0].DepartureTime, Is.EqualTo(Boundary));
        Assert.That(result[0].TripId, Is.EqualTo("trip-1"));
    }

    [Test]
    public void Filter_ItineraryWithTransfer_IsExcluded()
    {
        // Arrange
        var itineraries = new[]
        {
            new JourneyItinerary(
                Transfers: 1,
                Legs:
                [
                    new JourneyLeg("REGIONAL_RAIL", "Berlin Hbf", "Hannover Hbf", Boundary, Boundary.AddHours(1)),
                    new JourneyLeg("REGIONAL_RAIL", "Hannover Hbf", "Hamburg Hbf", Boundary.AddHours(1), Boundary.AddHours(2)),
                ]),
        };

        // Act
        var result = DirectTrainFilter.Filter(itineraries, arriveBy: false, Boundary);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [TestCase("SUBURBAN")]
    [TestCase("SUBWAY")]
    public void Filter_NonRailMode_IsExcluded(string mode)
    {
        // Arrange
        var itineraries = new[]
        {
            new JourneyItinerary(
                Transfers: 0,
                Legs: [new JourneyLeg(mode, "Berlin Hbf", "Hamburg Hbf", Boundary, Boundary.AddHours(2))]),
        };

        // Act
        var result = DirectTrainFilter.Filter(itineraries, arriveBy: false, Boundary);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_DepartAfter_DeparturesBeforeBoundaryAreExcluded()
    {
        // Arrange
        var itineraries = new[]
        {
            new JourneyItinerary(
                Transfers: 0,
                Legs: [new JourneyLeg("REGIONAL_RAIL", "Berlin Hbf", "Hamburg Hbf", Boundary.AddMinutes(-1), Boundary.AddHours(2))]),
        };

        // Act
        var result = DirectTrainFilter.Filter(itineraries, arriveBy: false, Boundary);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_ArriveBy_ArrivalsAfterBoundaryAreExcluded()
    {
        // Arrange
        var itineraries = new[]
        {
            new JourneyItinerary(
                Transfers: 0,
                Legs: [new JourneyLeg("REGIONAL_RAIL", "Berlin Hbf", "Hamburg Hbf", Boundary.AddHours(-2), Boundary.AddMinutes(1))]),
        };

        // Act
        var result = DirectTrainFilter.Filter(itineraries, arriveBy: true, Boundary);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Filter_ArriveBy_ArrivalAtBoundaryIsIncluded()
    {
        // Arrange
        var itineraries = new[]
        {
            new JourneyItinerary(
                Transfers: 0,
                Legs: [new JourneyLeg("REGIONAL_RAIL", "Berlin Hbf", "Hamburg Hbf", Boundary.AddHours(-2), Boundary)]),
        };

        // Act
        var result = DirectTrainFilter.Filter(itineraries, arriveBy: true, Boundary);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public void Filter_MoreThanFiveDirectTrains_ReturnsFirstFiveOrderedByDeparture()
    {
        // Arrange
        var itineraries = Enumerable.Range(0, 7)
            .Select(offset => new JourneyItinerary(
                Transfers: 0,
                Legs:
                [
                    new JourneyLeg(
                        "REGIONAL_RAIL",
                        "Berlin Hbf",
                        "Hamburg Hbf",
                        // Descending departure order in the input, to prove the filter sorts rather than passing input order through.
                        Boundary.AddHours(6 - offset),
                        Boundary.AddHours(8 - offset)),
                ]))
            .ToList();

        // Act
        var result = DirectTrainFilter.Filter(itineraries, arriveBy: false, Boundary);

        // Assert
        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result.Select(train => train.DepartureTime), Is.Ordered.Ascending);
        Assert.That(result[0].DepartureTime, Is.EqualTo(Boundary));
    }

    [Test]
    public void Filter_ArriveBy_MoreThanFiveQualify_ReturnsTheFiveClosestToBoundary()
    {
        // Arrange: 7 candidates, all arriving before the boundary, spaced an hour apart.
        // Arrive-by must keep the 5 *closest* to the boundary (the latest arrivals),
        // not the 5 earliest.
        var itineraries = Enumerable.Range(0, 7)
            .Select(offset => new JourneyItinerary(
                Transfers: 0,
                Legs:
                [
                    new JourneyLeg(
                        "REGIONAL_RAIL",
                        "Berlin Hbf",
                        "Hamburg Hbf",
                        Boundary.AddHours(-offset - 2),
                        Boundary.AddHours(-offset)),
                ]))
            .ToList();

        // Act
        var result = DirectTrainFilter.Filter(itineraries, arriveBy: true, Boundary);

        // Assert
        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result.Select(train => train.DepartureTime), Is.Ordered.Ascending);
        Assert.That(result[^1].DepartureTime, Is.EqualTo(Boundary.AddHours(-2)));
    }
}
