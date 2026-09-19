# Glossary

Definitions confirmed during grill-me sessions. Referenced by specs instead of
being redefined per-spec.

## Direct train

A train journey with zero transfers (`maxTransfers=0` in the transitous
`/api/v6/plan` request) — a single leg from origin to destination on one
vehicle/trip. Multi-leg itineraries with transfers are explicitly out of
scope for v1.

## En route (stop)

Any stop the selected direct train calls at between origin and destination,
inclusive of the origin and destination stops themselves. Used as the set of
locations checked for soccer fixtures. Sourced from transitous's
`intermediateStops` array on a leg (each with its own `lat`, `lon`,
`arrival`/`departure`, and `tz`).

## Depart-after / Arrive-by search

The two search modes for the train-search form, corresponding to transitous's
`arriveBy` flag:
- **Depart-after**: `arriveBy=false` — the entered date/time is a lower bound
  on departure; search walks forward in time.
- **Arrive-by**: `arriveBy=true` — the entered date/time is an upper bound on
  arrival; search walks backward in time.

Both modes roll into adjacent calendar days (up to a 2-day cap) if fewer than
5 direct trains are found on the anchor day.

## Soccer-fan match

A condition triggered when an en-route stop's arrival/departure time falls
within **±3 hours of kickoff** for a Bundesliga 1 or Bundesliga 2 fixture
whose home venue is in the same city as that stop. When true, the UI shows
"Soccer fans might be travelling with you." plus a detail line (teams,
city/stop, kickoff time). Shown once per matching stop/fixture — a journey
can have multiple matches.

## City match (stop ↔ fixture)

The mechanism used to decide whether an en-route stop is "in" a fixture's
venue city: a curated static list mapping each Bundesliga 1/2 club's home
city to the canonical city name(s)/aliases as they appear in German station
names (e.g. "Frankfurt" → matches "Frankfurt (Main) Hbf" but not
"Frankfurt (Oder)"). Chosen over generic station-name-suffix parsing (fragile
on compound/disambiguated city names) and over geo-radius matching
(OpenLigaDB doesn't provide venue coordinates, only a city name string).

## German-only station scope

Origin/destination station autocomplete (via transitous `/api/v1/geocode`) is
filtered to coordinates inside Germany's bounding box (~lat 47.3–55.1, lon
5.9–15.0), since transitous's geocode results carry no country field to
filter on directly. Non-German stations simply never appear as selectable
suggestions.
