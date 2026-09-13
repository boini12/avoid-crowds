# 01: Project scaffolding + health-check tracer bullet

**What to build:** Stand up the actual solution — the ASP.NET Core minimal-API backend, its test project, and the Vue 3 + TypeScript + Vite frontend — and prove the whole toolchain works with one trivial end-to-end round trip: the frontend loads, calls a backend health endpoint, and displays confirmation that the API is reachable.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] Solution ties together the API project and its test project; both build via `dotnet build`
- [ ] API project (.NET 9, minimal APIs) exposes `GET /api/health` returning a simple OK payload
- [ ] Frontend is a new Vue 3 + TypeScript + Vite app (npm) under `src/AvoidCrowds.Web`, with Vue Router installed and an empty route shell for the three pages (Search, Connections, Matches)
- [ ] CORS is configured on the API to allow the Vite dev server's origin
- [ ] Running the API (`dotnet run`) and the frontend (`npm run dev`) in two terminals, the Search page calls `/api/health` on load and visibly confirms the API is reachable
- [ ] Local dev workflow (two-terminal run) is documented (README or equivalent)
- [ ] The test project runs via `dotnet test` (a placeholder passing test is sufficient at this stage)
