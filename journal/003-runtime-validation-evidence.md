# 003 — Record real dashboard validation

## Change

Recorded Playwright-driven dashboard evidence and a screenshot of the real
success/fallback/circuit flow.

## ADR traceability

- ADR applied: ADR-0003 (observable resilience transition), ADR-0004 (marked
  fallback), ADR-0005 (local learning dashboard).

## Verification

- Full .NET test suite: 4 passed.
- Real local provider, API, and dashboard: verified through HTTP and browser flow.
- Docker Compose: attempted; Docker CLI did not return a container status in the
  shared desktop session. The evidence records this limitation without claiming a
  container validation that did not complete.
