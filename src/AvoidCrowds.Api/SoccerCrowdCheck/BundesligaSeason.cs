namespace AvoidCrowds.Api.SoccerCrowdCheck;

public static class BundesligaSeason
{
    // OpenLigaDB labels a season by its starting year (e.g. season "2025" is
    // the 2025/26 season, roughly July 2025 - June 2026).
    public static int ForDate(DateTimeOffset date) => date.Month >= 7 ? date.Year : date.Year - 1;
}
