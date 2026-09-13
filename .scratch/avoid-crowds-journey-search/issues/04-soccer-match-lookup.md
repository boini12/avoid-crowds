# 04: Soccer match lookup

**What to build:** After picking a connection, tell the user whether a Bundesliga or 2. Bundesliga match is happening that day anywhere on their route, so they know whether to expect crowds — closing the loop back to the start either way.

**Blocked by:** 03 (Connection search)

**Status:** ready-for-agent

- [ ] `ISportsDbClient` interface wraps TheSportsDB v1 free-tier endpoints (schedule by league/season), parsing responses into a minimal fixture type (home/away club, date, venue)
- [ ] TheSportsDB free API key is read from configuration (user-secrets locally, environment variables elsewhere) — never hardcoded
- [ ] A static club→home-city lookup table is built, covering the current season's 1. Bundesliga and 2. Bundesliga clubs
- [ ] `POST /api/matches` accepts the selected connection's payload (origin, destination, and the full ordered list of intermediate stops) and returns any 1./2. Bundesliga fixtures on that exact calendar date whose home club's city matches any station on the route — origin, destination, or an intermediate stop — matched by stripping common suffixes ("Hbf", "Central Station", etc.) from station names
- [ ] Matches page calls `/api/matches` on arrival and, if fixtures are found, displays them (club names, venue, kickoff time)
- [ ] If no fixtures match, the Matches page shows a "this train won't be crowded with fans" message instead
- [ ] A "back to start" button is always present on the Matches page, in both outcomes, and returns the user to the Search page with all held state cleared
- [ ] Unit tests cover the pure club-city/route matching logic directly, with `ISportsDbClient` faked; no live external calls in tests
