# Study guide

Start with `GET /health`, then call a successful score lookup. This seeds the
deliberately in-memory fallback. Next, send `scenario=server-error` or `timeout`:
the retry events and circuit transition are available from
`GET /api/resilience/state?correlationId=...`.

Read ADR-0001 for scope, ADR-0002 for the boundary, ADR-0003 for policy ordering,
ADR-0004 for the failure contract, and ADR-0005 before looking at the dashboard.

Interview prompts answered here:

- Why retry only transient faults, and why is every attempt time bounded?
- What is the difference between a fallback and a fresh provider score?
- Why is circuit state observable but not used as a hidden success path?
- Why does a local Blazor UI help learning without changing the backend-first scope?
