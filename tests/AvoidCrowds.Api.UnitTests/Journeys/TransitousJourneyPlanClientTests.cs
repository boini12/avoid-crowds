using System.Net;
using System.Text;
using AvoidCrowds.Api.Journeys;
using NUnit.Framework;

namespace AvoidCrowds.Api.UnitTests.Journeys;

[TestFixture]
public class TransitousJourneyPlanClientTests
{
    [Test]
    public async Task PlanAsync_SendsTimeAsUtcWithoutFractionalSeconds()
    {
        // Arrange: transitous misreads the round-trip "O" format
        // (2026-10-09T19:00:00.0000000+02:00) as 19:00 UTC, silently skipping
        // two hours of trains - the time must go out as plain UTC seconds.
        var handler = new CapturingHandler();
        var client = new TransitousJourneyPlanClient(new HttpClient(handler) { BaseAddress = new Uri("https://transitous.test/") });
        var query = new JourneyPlanQuery("from", "to", new DateTimeOffset(2026, 10, 9, 19, 0, 0, TimeSpan.FromHours(2)), ArriveBy: false);

        // Act
        await client.PlanAsync(query, CancellationToken.None);

        // Assert
        var time = System.Web.HttpUtility.ParseQueryString(handler.LastRequestUri!.Query)["time"];
        Assert.That(time, Is.EqualTo("2026-10-09T17:00:00Z"));
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"itineraries":[]}""", Encoding.UTF8, "application/json"),
            });
        }
    }
}
