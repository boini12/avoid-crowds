# 03: Connection search

**What to build:** Complete the search form (date, time, arrival/departure) and the matching engine that turns a station pair + time into up to 5 real train connections, so the user can search a journey and pick one to continue with.

**Blocked by:** 01 (Project scaffolding + health-check tracer bullet), 02 (Station typeahead)

**Status:** ready-for-agent

- [ ] Search page collects origin station, destination station (both via ticket 02's typeahead), a single date+time value, and a mutually-exclusive arrival/departure toggle, with basic required-field validation before submit
- [ ] `POST /api/connections/search` accepts the origin/destination stations, date, time, and mode (arrival|departure), and returns up to 5 connection candidates
- [ ] Departure-mode: fetches the origin station's board across an hour-window scan of ±4 hours from the requested time; a departure event is a candidate if its planned path (`ppth`) contains the destination station name
- [ ] Arrival-mode: fetches the destination station's board across the same ±4-hour scan; an arrival event is a candidate if its preceding path contains the origin station name, with its departure time resolved via a second lookup (matching trip/train number) against the origin station's board
- [ ] Candidates are sorted by proximity to the requested time and capped at 5; each candidate carries its full intermediate route (ordered list of stops from `ppth`) for use by ticket 04
- [ ] Connections page renders the candidates (origin, destination, departure date/time); selecting one proceeds to the Matches page carrying the full candidate payload as client-held state (no backend session)
- [ ] If zero candidates are found within the scan window, the Connections page shows a clear "no trains found" message with a "back to start" button returning to the Search page
- [ ] Unit tests cover the pure matching logic directly — path/`ppth` string matching, the hour-window candidate scan, the arrival-mode two-lookup resolution, and the sort/cap-at-5 — with `IDbTimetablesClient` faked; no live external calls in tests
- [ ] All timetable times are treated as Europe/Berlin local time throughout; no per-user timezone conversion
