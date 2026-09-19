using AvoidCrowds.Api.Geocoding;
using AvoidCrowds.Api.Journeys;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";
var frontendOrigin = builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173";

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(frontendOrigin).AllowAnyHeader().AllowAnyMethod());
});

// transitous usage policy requires a User-Agent identifying the client.
var transitousUserAgent = builder.Configuration["Transitous:UserAgent"]
    ?? "avoid-crowds/0.1 (non-commercial; https://github.com/avoid-crowds)";

builder.Services.AddHttpClient<IGeocodeClient, TransitousGeocodeClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Transitous:BaseUrl"] ?? "https://api.transitous.org/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(transitousUserAgent);
});

builder.Services.AddHttpClient<IJourneyPlanClient, TransitousJourneyPlanClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Transitous:BaseUrl"] ?? "https://api.transitous.org/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd(transitousUserAgent);
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(FrontendCorsPolicy);

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/stations", async (string? query, IGeocodeClient geocodeClient, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(query))
    {
        return Results.Ok(Array.Empty<StationSuggestion>());
    }

    var matches = await geocodeClient.SearchAsync(query, cancellationToken);
    return Results.Ok(GermanStationFilter.Filter(matches));
});

app.MapControllers();

app.Run();

public partial class Program;
