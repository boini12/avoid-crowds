# Train Journey Soccer-Crowd Check

_Spec captured: 2026-09-19_

Terminology used below is defined in `glossary/glossary.md`; rationale for
non-obvious choices is in `design-decisions/design-decisions.md`.

## Problem / motivation

A traveler planning a German train trip wants to know, before boarding,
whether they're likely to share the train (or a station along the route)
with a crowd of soccer fans heading to or from a Bundesliga match. The app
takes a journey request (from/to station, date/time, depart-after or
arrive-by), finds up to 5 matching direct trains, and — once the traveler
picks one — checks whether any Bundesliga 1/2 fixture is happening near an
en-route stop within a 3-hour window of the train's time there.

## Scope

**In scope:**
- Vue 3 + TypeScript + Vite frontend, ASP.NET Core minimal API backend
  (existing solution layout in `src/AvoidCrowds.Api` / `src/AvoidCrowds.Web`).
- Search form: from/to station (autocomplete, Germany-only), date+time,
  depart-after/arrive-by toggle.
- Backend queries transitous (`/api/v6/plan`, `/api/v1/geocode`) for direct,
  rail-only trains, rolling forward/backward in time (capped at 2 days) until
  5 results are found or the cap is hit.
- Results list view: up to 5 trains, showing origin, destination, and
  departure date/time.
- Train detail view: selected train's info plus soccer-fan crowd check
  results.
- Soccer-fan check: every en-route stop checked against Bundesliga 1+2
  fixtures (OpenLigaDB) within a ±3 hour window of the stop's own
  arrival/departure time, matched by a curated stop-city ↔ club-city list.
- "Back to start" button on the Results and Train Detail views.
- Inline error banner with a retry button for any external API failure.
- In-memory caching of OpenLigaDB season data per league.
- Backend xUnit tests for the day-rolling search logic and the 3-hour
  stop/kickoff matching logic.

**Out of scope (v1):**
- Multi-leg itineraries / transfers (direct trains only).
- S-Bahn (`SUBURBAN` mode) and subway/metro (`SUBWAY` mode).
- Non-German train search.
- Authentication/accounts.
- Persistence/database (including search history or favorites).
- CI, Docker, or any deployment target — local dev only.
- A map or visual route display — text/list views only.
- Leagues other than Bundesliga 1 and 2.

## Success criteria

- Submitting a valid from/to/date-time search returns up to 5 direct,
  rail-mode-only trains, each showing origin, destination, and departure
  date/time.
- "Depart-after" search results all depart at or after the entered time;
  "arrive-by" results all arrive at or before it (walking backward in time).
- If fewer than 5 direct trains exist on the anchor day, the search
  transparently rolls into the next (or previous) day(s), up to 2 days
  total, before showing fewer than 5 results.
- Only stations within Germany's bounding box appear in from/to autocomplete
  suggestions.
- Selecting a train shows a detail view; if any en-route stop (including
  origin/destination) has a Bundesliga 1 or 2 fixture within ±3 hours of the
  train's time at that stop, the view shows "Soccer fans might be travelling
  with you." plus a detail line (teams, city/stop, kickoff time) — once per
  matching stop/fixture.
- If no match is found, no soccer-fan message is shown.
- "Back to start" is present and functional on Results and Train Detail
  views.
- A failed transitous or OpenLigaDB call shows an inline error with a retry
  button that re-issues the same request.
- Backend unit tests cover: (a) the multi-day rolling search-until-5-results
  logic, and (b) the stop-time-vs-kickoff ±3-hour matching logic.

## Constraints

- transitous usage policy: must send a `User-Agent`/attribution per their
  terms; open-source/non-commercial use only; avoid excessive
  resource-intensive requests.
- OpenLigaDB has no date-query endpoint — must fetch season-wide per league
  and filter client-side by `matchDateTime` (see design decisions).
- No API keys required for either external API in v1.
- Times must be handled timezone-aware using transitous's per-stop `tz`
  field, not assumed to be a single fixed zone.

## Edge cases

- Same origin and destination station: blocked client-side with an inline
  validation message before search is attempted.
- Fewer than 5 direct trains exist even after rolling 2 days: show however
  many were found, no error.
- A station typed but never selected from autocomplete: search button stays
  disabled — free text alone cannot be submitted.
- Multiple soccer matches on one journey (different stops/fixtures each
  within their own ±3h window): show the message once per match, not
  collapsed into a single generic notice.
- OpenLigaDB or transitous request fails/times out: inline error banner +
  retry button, no partial/broken result rendering.
- A stop's city matches a Bundesliga club's curated city, but the fixture's
  `location` is null in OpenLigaDB (known data-quality gap) — that fixture
  cannot be matched to a venue city and is excluded from consideration for
  that stop.

## Integration points

- **transitous** (`https://api.transitous.org/api/`): `GET /api/v6/plan`
  (journey search — `fromPlace`, `toPlace`, `time`, `arriveBy`,
  `maxTransfers=0`, mode filter, `numItineraries`, `pageCursor`) and
  `GET /api/v1/geocode` (station autocomplete, `text` param, filtered to
  Germany's bounding box client/server-side).
- **OpenLigaDB** (`https://api.openligadb.de`): `GET /getmatchdata/bl1/{season}`
  and `GET /getmatchdata/bl2/{season}` (season-wide fixture fetch, cached
  in-memory, filtered by `matchDateTime`).
- Frontend never calls transitous or OpenLigaDB directly — all external API
  calls go through the ASP.NET Core backend, which owns the business logic
  (search rolling, mode filtering, stop/kickoff matching).

## Assumptions

- None outstanding — all dimensions below were explicitly asked and
  confirmed during the grill-me session rather than inferred:
  search-direction semantics, mode scope, direct-vs-multi-leg, en-route
  definition, match-window comparison basis, soccer data source,
  city-matching approach, German-only station scope, view flow, styling,
  caching, error UX, language/timezone, day-roll cap, station-input
  validation, same-station validation, message detail level, multi-match
  handling, authentication, and persistence.

## Open questions

- None — the design-tree frontier was closed and confirmed by the user
  before this spec was written.
