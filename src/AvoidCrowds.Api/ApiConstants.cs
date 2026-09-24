namespace AvoidCrowds.Api;

/// <summary>
/// Single place for the string literals the application host wires up:
/// policy names, configuration keys, their fallbacks, and route templates.
/// </summary>
public static class ApiConstants
{
    public const string FrontendCorsPolicy = "FrontendCorsPolicy";

    public static class ConfigKeys
    {
        public const string FrontendOrigin = "FrontendOrigin";
        public const string TransitousBaseUrl = "Transitous:BaseUrl";
        public const string TransitousUserAgent = "Transitous:UserAgent";
        public const string OpenLigaDbBaseUrl = "OpenLigaDb:BaseUrl";
    }

    public static class Defaults
    {
        public const string FrontendOrigin = "http://localhost:5173";
        public const string TransitousBaseUrl = "https://api.transitous.org/";

        // transitous usage policy requires a User-Agent identifying the client.
        public const string TransitousUserAgent = "avoid-crowds/0.1 (non-commercial; https://github.com/avoid-crowds)";

        public const string OpenLigaDbBaseUrl = "https://api.openligadb.de/";
    }

    public static class Routes
    {
        public const string Health = "/api/health";
        public const string Stations = "/api/stations";
    }
}
