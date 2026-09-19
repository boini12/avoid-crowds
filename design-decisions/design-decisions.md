# Design Decisions

Rationale behind choices made during grill-me sessions, kept separate from
specs so it isn't re-litigated or re-explained each time.

## 2026-09-19 — Train Journey Soccer-Crowd Check

### Search rolls forward/backward in time, not just within one day
"Depart-after" walks forward from the entered time; "arrive-by" walks
backward — standard journey-planner semantics — rather than always searching
forward regardless of mode. See [[glossary#depart-after--arrive-by-search]].

### Search is bounded to the anchor calendar day only (v1)
An earlier version of this decision had the search roll into adjacent days
(capped at 2) when fewer than 5 direct trains were found on the anchor day.
Revisited during implementation: for v1, the search window is bounded to
exactly the anchor calendar day (in the query time's own offset) and does not
roll into adjacent days. Rolling into adjacent days is deferred as a separate
concern for a later version. See [[glossary#depart-after--arrive-by-search]].

### Trains only, direct connections only, no S-Bahn (v1)
transitous's `RAIL` mode family bundles highspeed/long-distance/night/
regional rail with `SUBURBAN` (S-Bahn); `SUBWAY` is separate. v1 excludes
both `SUBURBAN` and `SUBWAY` — S-Bahn was judged overkill for v1 and can be
added later. Only direct (zero-transfer) trains are shown — simpler to
reason about and to display as a single selectable list item; multi-leg
itineraries are a future extension. See [[glossary#direct-train]].

### Soccer data source: OpenLigaDB, not TheSportsDB
TheSportsDB's free tier caps `eventsday.php` to ~3 events/day globally (1 if
sport-filtered) — a hard ceiling unrelated to its separate 30 req/min rate
limit, confirmed by live testing. This makes the "soccer game happening
en route" feature nearly non-functional on the free key. OpenLigaDB has no
API key/auth at all, covers both Bundesliga 1 and 2 (football-data.org's
free tier was also considered but excludes Bundesliga 2 entirely), and has
generous informal rate headroom (~1000 req/hr). Trade-off accepted:
OpenLigaDB has no venue geo-coordinates, only a `locationCity` string (see
the city-matching decision below), and no date-query endpoint (see the
caching decision below).

### City matching via curated static list, not geo-radius or generic parsing
OpenLigaDB provides no venue coordinates, ruling out radius-based matching.
Generic parsing of German station names (stripping "Hauptbahnhof"/"Hbf"
suffixes) was rejected as fragile against compound/disambiguated city names
(e.g. "Frankfurt (Main)" vs "Frankfurt (Oder)"). Since Bundesliga 1+2 is a
small, bounded set (~36 clubs), a curated static city list is more reliable
and easy to maintain. See [[glossary#city-match-stop--fixture]].

### Soccer-fan match window compares kickoff to the stop's own time, not the journey's overall start/end
The original spec sentence describing this was circular/ambiguous ("checks
if any soccer games are happening in a range of 3 hours after and before a
soccer game that is happening en route"). Reconstructed and confirmed with
the user: for each en-route stop, compare that stop's own arrival/departure
time (not the train's overall departure or arrival) against a candidate
fixture's kickoff ± 3 hours. Every intermediate stop counts as "en route,"
not just origin/destination, since a train can pass through a match city
without the match being at either endpoint.

### Message shown once per matching stop/fixture, with detail
The spec's literal message ("Soccer fans might be travelling with you.") is
shown as-is, but with a supporting detail line (teams, city/stop, kickoff
time) so it isn't an unexplained non-sequitur. If multiple stops/fixtures
match on one journey, the message+detail repeats per match rather than
collapsing into one generic notice — judged more informative for the user.

### German-only train search, enforced via geocode bounding-box filtering
transitous itself has no country filter and is pan-European by default. The
user asked for German-only trains "if possible." transitous's geocode
`Match` object has no country field, so this is enforced by filtering
autocomplete suggestions to coordinates within Germany's bounding box —
confirmed feasible, so implemented rather than left unenforced. See
[[glossary#german-only-station-scope]].

### OpenLigaDB fixtures cached season-wide, not per-date
OpenLigaDB has no date-query endpoint at all (confirmed against its full
OpenAPI spec — 22 paths, all keyed by league/season/matchday/team/matchId,
none by date). The standard workaround, confirmed via live testing, is a
single season-wide fetch (`getmatchdata/{league}/{season}`, ~306 matches for
Bundesliga 1 in one call) filtered client-side by `matchDateTime` — cheap
and practical. This is cached in-memory per league/season rather than
per-date. Matchday groupings don't map to single calendar dates (a matchday
can span Friday–Monday), so date filtering must happen per-match, never by
matchday.

### No authentication, no persistence/database (v1)
Confirmed explicitly rather than left as a silent assumption: this is a
stateless, single-shared-use lookup tool. No accounts, no per-user data, no
database — the backend is a thin business-logic layer over transitous and
OpenLigaDB plus an in-memory cache (which is ephemeral, not real
persistence).

### API endpoints follow the MVC controller pattern, not minimal-API lambdas
The journeys endpoint was reworked from a `MapGet` lambda in `Program.cs` into
a `JourneysController` (ASP.NET Core MVC), at the user's request, to follow
the MVC pattern for this repository going forward. Existing endpoints
(`/api/health`, `/api/stations`) were left as minimal-API lambdas for now
rather than migrated in the same change, to keep this change scoped to the
journeys endpoint.

### Stack (carried over from existing README, not re-litigated)
ASP.NET Core minimal API backend (`src/AvoidCrowds.Api`), Vue 3 + TypeScript
+ Vite frontend (`src/AvoidCrowds.Web`), xUnit backend tests
(`tests/AvoidCrowds.Api.Tests`). Styling: Tailwind, chosen over plain CSS or
a component library as the fastest fit for a small app's scope. No
CI/Docker/deployment planned for v1 — local dev only.
