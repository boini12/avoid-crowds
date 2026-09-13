# 02: Station typeahead

**What to build:** Let the user find and pick real Deutsche Bahn stations by name on the search page, backed by a live lookup against DB's station-search endpoint, so later matching logic always has an exact, valid station name to work with.

**Blocked by:** 01 (Project scaffolding + health-check tracer bullet)

**Status:** ready-for-agent

- [ ] `IDbTimetablesClient` interface wraps DB's `GET /station/{pattern}`, parsing the XML response into a minimal station-suggestion type (name, EVA number)
- [ ] `DB-Client-Id` / `DB-Api-Key` are read from configuration (user-secrets locally, environment variables elsewhere) — never hardcoded
- [ ] `GET /api/stations?q=` uses `IDbTimetablesClient` to return matching station suggestions for a free-text query
- [ ] Origin and destination fields on the Search page call `/api/stations?q=` as the user types (debounced) and require picking one suggestion each — the app stores the chosen station's real DB name/EVA number, never arbitrary free text
- [ ] Unit tests cover the XML→DTO parsing logic against sample DB station-search responses, with the underlying HTTP call faked — no live external calls in tests
- [ ] If the DB API call fails, the typeahead shows a clear inline error rather than crashing the page
