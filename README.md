# avoid-crowds

Plan a train journey and check whether it'll be crowded with football fans.

## Solution layout

- `src/AvoidCrowds.Api` — ASP.NET Core minimal API backend
- `src/AvoidCrowds.Web` — Vue 3 + TypeScript + Vite frontend
- `tests/AvoidCrowds.Api.Tests` — backend test project (xUnit)

## Local development

Run the API and the frontend in two separate terminals.

**Terminal 1 — API** (defaults to `http://localhost:5027`):

```sh
dotnet run --project src/AvoidCrowds.Api
```

**Terminal 2 — frontend** (defaults to `http://localhost:5173`):

```sh
cd src/AvoidCrowds.Web
npm install
npm run dev
```

Open the frontend URL in a browser — the Search page calls the API's
`GET /api/health` endpoint on load and shows whether the API is reachable.

If the API runs on a different URL, point the frontend at it via a
`VITE_API_BASE_URL` environment variable (e.g. in `src/AvoidCrowds.Web/.env.local`).

## Tests

```sh
dotnet test
```
