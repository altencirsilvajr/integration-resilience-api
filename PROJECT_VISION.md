# Integration Resilience API — Project Vision

## Purpose

This is a deliberately small, vertical .NET 9 laboratory for studying an unstable
external HTTP dependency. It is portfolio material: every moving part exists to
make a senior-level integration decision visible, reproducible, and discussable.

## The pressure being explored

A customer-score provider can be slow, throttle callers, or fail. A user request
must remain bounded; resilience must not turn stale information into apparently
current information; and the HTTP contract must describe failures explicitly.

## Non-goals

- It is not a customer product, public dashboard, or production deployment.
- It has no real external provider, authentication, persistence database, or UI
  business logic.
- It does not promise exactly-once delivery or distributed transactions.

## Success criteria

- A typed `HttpClient` calls a local, configurable fake provider.
- Per-attempt timeout, exponential retry, and circuit breaker behavior are
  observable through structured logs, API state, and the learning dashboard.
- A cached fallback is labeled as `fallback`, includes its age, and never claims
  to be a fresh provider result.
- The API uses ProblemDetails when a request has no honest fallback.
- Documentation, tests, and the dashboard form one guided study path.

## Constraints

- Every project targets `net9.0`.
- Backend is ASP.NET Core Minimal API with thin handlers and separate contracts.
- Ports are API `5306`, dashboard `5406`, and fake provider `5307`.
- The fake provider is local and deterministic; no real provider is required.

## Source-of-truth rule

This vision owns scope. New architectural choices must agree with it and be
recorded in an ADR when they affect system-wide behavior.
