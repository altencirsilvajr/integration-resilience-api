# Blazor flow evidence — 2026-07-11

## Environment

The local fake provider ran at `http://localhost:5307`, API at
`http://localhost:5306`, and dashboard at `http://localhost:5406`.

## Automated browser route

Playwright CLI drove the real dashboard (no mocked browser/API route):

1. Opened the dashboard and confirmed the study guide and five scenario buttons.
2. Clicked **1. Seed success**: the API card reported HTTP 200,
   `source:"provider"`, and a real correlation id.
3. Clicked **2. Trigger 503**: the API card reported HTTP 200 with
   `source:"fallback"`, `fallbackAgeSeconds`, and
   `providerFailureClass:"circuit-open"`.
4. The timeline card showed two HTTP 503 attempts, exponential retry delays of
   120 ms and 240 ms, and a circuit-open event.
5. Clicked **5. Wait and recover** in the same real flow: the following provider
   probe returned HTTP 200 and the circuit became `Closed`.

The browser console was checked after the final dashboard build: 0 errors and 0
warnings. Screenshot: [fallback and circuit timeline](images/2026-07-11-blazor-flow.png).

## Test evidence

`dotnet test IntegrationResilience.sln --no-build --no-restore --logger
"console;verbosity=minimal"` passed 4 tests: 2 domain/application behavior tests
and 2 API/governance tests, including `WebApplicationFactory` HTTP health coverage.

## Docker Compose validation (repeated successfully)

The retry on 2026-07-12 completed with Docker 29.5.3 / Compose v5.1.4:

- `docker compose up --build -d` built and started `fake-provider`, `api`, and
  `dashboard`; `docker compose ps` reported all three as `Up` on 5307, 5306, and
  5406 respectively.
- Containerized `GET /health` returned HTTP 200.
- A containerized provider lookup returned `source:"provider"`; a configured
  503 then returned an explicitly marked fallback with `circuit-open` and the
  timeline named the internal target `http://fake-provider:8080/...`.
- Playwright clicked Seed success, Trigger 503, and Wait and recover against the
  Docker-served dashboard. It observed provider → fallback/open circuit →
  provider/closed circuit using real API calls.

The sole browser console error was the browser's automatic request for a missing
`/favicon.ico`; it did not affect the dashboard flow.
