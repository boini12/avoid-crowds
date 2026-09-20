namespace AvoidCrowds.Api.SoccerCrowdCheck;

public record CityDefinition(string Name, IReadOnlyList<string> FixtureCityNames, IReadOnlyList<string> StationNameAliases);

// Curated Bundesliga 1 + 2 home cities, mapped to the city name(s) OpenLigaDB
// reports as a fixture's location.city and the alias(es) used in German
// station names. Aliases are deliberately specific (e.g. "Frankfurt (Main)",
// not bare "Frankfurt") so a like-named but distinct city - "Frankfurt (Oder)"
// has no Bundesliga club and is intentionally absent from this list - is
// never matched to a fixture in Frankfurt am Main.
public static class BundesligaCities
{
    public static readonly IReadOnlyList<CityDefinition> All =
    [
        new("München", ["München", "Munich"], ["München"]),
        new("Dortmund", ["Dortmund"], ["Dortmund"]),
        new("Leipzig", ["Leipzig"], ["Leipzig"]),
        new("Leverkusen", ["Leverkusen"], ["Leverkusen"]),
        new("Frankfurt am Main", ["Frankfurt am Main", "Frankfurt"], ["Frankfurt (Main)", "Frankfurt(Main)"]),
        new("Stuttgart", ["Stuttgart"], ["Stuttgart"]),
        new("Freiburg", ["Freiburg", "Freiburg im Breisgau"], ["Freiburg"]),
        new("Sinsheim", ["Sinsheim"], ["Sinsheim"]),
        new("Berlin", ["Berlin"], ["Berlin"]),
        new("Bremen", ["Bremen"], ["Bremen"]),
        new("Wolfsburg", ["Wolfsburg"], ["Wolfsburg"]),
        new("Mönchengladbach", ["Mönchengladbach"], ["Mönchengladbach"]),
        new("Mainz", ["Mainz"], ["Mainz"]),
        new("Augsburg", ["Augsburg"], ["Augsburg"]),
        new("Bochum", ["Bochum"], ["Bochum"]),
        new("Hamburg", ["Hamburg"], ["Hamburg"]),
        new("Köln", ["Köln", "Cologne"], ["Köln"]),
        new("Kiel", ["Kiel"], ["Kiel"]),
        new("Darmstadt", ["Darmstadt"], ["Darmstadt"]),
        new("Heidenheim", ["Heidenheim"], ["Heidenheim"]),
        new("Paderborn", ["Paderborn"], ["Paderborn"]),
        new("Düsseldorf", ["Düsseldorf"], ["Düsseldorf"]),
        new("Karlsruhe", ["Karlsruhe"], ["Karlsruhe"]),
        new("Hannover", ["Hannover"], ["Hannover"]),
        new("Ulm", ["Ulm"], ["Ulm"]),
        new("Braunschweig", ["Braunschweig"], ["Braunschweig"]),
        new("Fürth", ["Fürth"], ["Fürth"]),
        new("Münster", ["Münster"], ["Münster"]),
        new("Gelsenkirchen", ["Gelsenkirchen"], ["Gelsenkirchen"]),
        new("Regensburg", ["Regensburg"], ["Regensburg"]),
        new("Dresden", ["Dresden"], ["Dresden"]),
        new("Nürnberg", ["Nürnberg"], ["Nürnberg"]),
        new("Elversberg", ["Elversberg"], ["Elversberg"]),
        new("Magdeburg", ["Magdeburg"], ["Magdeburg"]),
    ];

    public static bool MatchesStop(string stopName, string fixtureCity)
    {
        var city = All.FirstOrDefault(candidate =>
            candidate.FixtureCityNames.Contains(fixtureCity, StringComparer.OrdinalIgnoreCase));

        return city is not null
            && city.StationNameAliases.Any(alias => stopName.Contains(alias, StringComparison.OrdinalIgnoreCase));
    }
}
