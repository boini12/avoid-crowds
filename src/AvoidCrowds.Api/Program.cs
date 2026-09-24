using AvoidCrowds.Api;
using AvoidCrowds.Api.Geocoding;
using AvoidCrowds.Api.Journeys;
using AvoidCrowds.Api.SoccerCrowdCheck;

var builder = WebApplication.CreateBuilder(args);

var frontendOrigin = builder.Configuration[ApiConstants.ConfigKeys.FrontendOrigin]
    ?? ApiConstants.Defaults.FrontendOrigin;

builder.Services.AddCors(options =>
{
    options.AddPolicy(ApiConstants.FrontendCorsPolicy, policy =>
        policy.WithOrigins(frontendOrigin).AllowAnyHeader().AllowAnyMethod());
});

var transitousBaseUrl = builder.Configuration[ApiConstants.ConfigKeys.TransitousBaseUrl]
    ?? ApiConstants.Defaults.TransitousBaseUrl;

var transitousUserAgent = builder.Configuration[ApiConstants.ConfigKeys.TransitousUserAgent]
    ?? ApiConstants.Defaults.TransitousUserAgent;

builder.Services.AddHttpClient<IGeocodeClient, TransitousGeocodeClient>(client =>
{
    client.BaseAddress = new Uri(transitousBaseUrl);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(transitousUserAgent);
});

builder.Services.AddHttpClient<IJourneyPlanClient, TransitousJourneyPlanClient>(client =>
{
    client.BaseAddress = new Uri(transitousBaseUrl);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(transitousUserAgent);
});

builder.Services.AddHttpClient<IFixtureClient, OpenLigaDbFixtureClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration[ApiConstants.ConfigKeys.OpenLigaDbBaseUrl]
        ?? ApiConstants.Defaults.OpenLigaDbBaseUrl);
});

builder.Services.AddSingleton<FixtureCache>();
builder.Services.AddScoped<ICrowdCheckService, CrowdCheckService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(ApiConstants.FrontendCorsPolicy);

app.MapGet(ApiConstants.Routes.Health, () => Results.Ok(new { status = "ok" }));

app.MapGet(ApiConstants.Routes.Stations, async (string? query, IGeocodeClient geocodeClient, CancellationToken cancellationToken) =>
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
