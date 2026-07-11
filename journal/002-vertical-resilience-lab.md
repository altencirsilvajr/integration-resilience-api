# 002 — Implement the vertical resilience laboratory

## Change

Created the .NET 9 solution, local fake provider, Minimal API, typed HTTP boundary,
Polly resilience pipeline, explicit fallback semantics, local Blazor dashboard,
tests, Docker Compose, and study documentation.

## Rationale

The vertical path demonstrates a real, reproducible integration failure rather
than simulating business behavior in the UI.

## ADR traceability

- ADR applied: ADR-0001 through ADR-0005.
- Local decision: in-memory fallback is sufficient because persistence is outside
  this laboratory's non-goals.

## Verification

`dotnet build IntegrationResilience.sln --no-restore` succeeds. Full runtime and
browser evidence are recorded in a later validation journal.
