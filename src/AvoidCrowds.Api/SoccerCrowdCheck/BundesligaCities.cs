namespace AvoidCrowds.Api.SoccerCrowdCheck;

// Curated Bundesliga 1 + 2 home cities, mapped to the name(s) OpenLigaDB
// reports for a club playing at home there, the city name(s) it reports as a
// fixture's location.city, and the alias(es) used in German station names.
// Aliases are deliberately specific (e.g. "Frankfurt (Main)", not bare
// "Frankfurt") so a like-named but distinct city - "Frankfurt (Oder)" has no
// Bundesliga club and is intentionally absent from this list - is never
// matched to a fixture in Frankfurt am Main.
public static class BundesligaCities
{
    public static readonly IReadOnlyList<CityDefinition> All =
    [
        new("München", ["München", "Munich"], ["FC Bayern München"], ["München"]),
        new("Dortmund", ["Dortmund"], ["Borussia Dortmund"], ["Dortmund"]),
        new("Leipzig", ["Leipzig"], ["RB Leipzig"], ["Leipzig"]),
        new("Leverkusen", ["Leverkusen"], ["Bayer 04 Leverkusen"], ["Leverkusen"]),
        new("Frankfurt am Main", ["Frankfurt am Main", "Frankfurt"], ["Eintracht Frankfurt"], ["Frankfurt (Main)", "Frankfurt(Main)"]),
        new("Stuttgart", ["Stuttgart"], ["VfB Stuttgart"], ["Stuttgart"]),
        new("Freiburg", ["Freiburg", "Freiburg im Breisgau"], ["SC Freiburg"], ["Freiburg"]),
        // TSG Hoffenheim plays in Sinsheim, not in Hoffenheim itself.
        new("Sinsheim", ["Sinsheim"], ["TSG Hoffenheim", "TSG 1899 Hoffenheim"], ["Sinsheim"]),
        new("Berlin", ["Berlin"], ["1. FC Union Berlin", "Hertha BSC"], ["Berlin"]),
        new("Bremen", ["Bremen"], ["SV Werder Bremen"], ["Bremen"]),
        new("Wolfsburg", ["Wolfsburg"], ["VfL Wolfsburg"], ["Wolfsburg"]),
        new("Mönchengladbach", ["Mönchengladbach"], ["Borussia Mönchengladbach"], ["Mönchengladbach"]),
        new("Mainz", ["Mainz"], ["1. FSV Mainz 05"], ["Mainz"]),
        new("Augsburg", ["Augsburg"], ["FC Augsburg"], ["Augsburg"]),
        new("Bochum", ["Bochum"], ["VfL Bochum"], ["Bochum"]),
        new("Hamburg", ["Hamburg"], ["Hamburger SV", "FC St. Pauli"], ["Hamburg"]),
        new("Köln", ["Köln", "Cologne"], ["1. FC Köln"], ["Köln"]),
        new("Kiel", ["Kiel"], ["Holstein Kiel"], ["Kiel"]),
        new("Darmstadt", ["Darmstadt"], ["SV Darmstadt 98"], ["Darmstadt"]),
        new("Heidenheim", ["Heidenheim"], ["1. FC Heidenheim 1846"], ["Heidenheim"]),
        new("Paderborn", ["Paderborn"], ["SC Paderborn 07"], ["Paderborn"]),
        new("Düsseldorf", ["Düsseldorf"], ["Fortuna Düsseldorf"], ["Düsseldorf"]),
        new("Karlsruhe", ["Karlsruhe"], ["Karlsruher SC"], ["Karlsruhe"]),
        new("Hannover", ["Hannover"], ["Hannover 96"], ["Hannover"]),
        new("Ulm", ["Ulm"], ["SSV Ulm 1846"], ["Ulm"]),
        new("Braunschweig", ["Braunschweig"], ["Eintracht Braunschweig"], ["Braunschweig"]),
        new("Fürth", ["Fürth"], ["SpVgg Greuther Fürth"], ["Fürth"]),
        new("Münster", ["Münster"], ["Preußen Münster"], ["Münster"]),
        // FC Schalke 04 plays in Gelsenkirchen; the club name names no city.
        new("Gelsenkirchen", ["Gelsenkirchen"], ["FC Schalke 04"], ["Gelsenkirchen"]),
        new("Regensburg", ["Regensburg"], ["Jahn Regensburg"], ["Regensburg"]),
        new("Dresden", ["Dresden"], ["Dynamo Dresden"], ["Dresden"]),
        new("Nürnberg", ["Nürnberg"], ["1. FC Nürnberg"], ["Nürnberg"]),
        new("Elversberg", ["Elversberg"], ["SV 07 Elversberg"], ["Elversberg"]),
        new("Magdeburg", ["Magdeburg"], ["1. FC Magdeburg"], ["Magdeburg"]),
        new("Kaiserslautern", ["Kaiserslautern"], ["1. FC Kaiserslautern"], ["Kaiserslautern"]),
        new("Bielefeld", ["Bielefeld"], ["DSC Arminia Bielefeld"], ["Bielefeld"]),
        new("Cottbus", ["Cottbus"], ["Energie Cottbus"], ["Cottbus"]),
        new("Osnabrück", ["Osnabrück"], ["VfL Osnabrück"], ["Osnabrück"]),
    ];

    // Resolves the city a fixture is played in. The home team is the primary
    // signal: OpenLigaDB's location object is null for every fixture of the
    // seasons this app queries, so relying on location.city alone would mean
    // never matching anything. location.city is still honoured when present,
    // since it is the more direct statement of where the match is played.
    public static CityDefinition? GetVenueCity(string? fixtureCity, string homeTeam)
    {
        var byLocation = fixtureCity is null
            ? null
            : All.FirstOrDefault(candidate =>
                candidate.FixtureCityNames.Contains(fixtureCity, StringComparer.OrdinalIgnoreCase));

        return byLocation
            ?? All.FirstOrDefault(candidate =>
                candidate.HomeTeamNames.Contains(homeTeam, StringComparer.OrdinalIgnoreCase));
    }
}
