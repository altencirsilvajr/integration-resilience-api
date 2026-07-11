# ADR-0002: Use typed HttpClient for the external integration boundary

- Status: Accepted
- Date: 2026-07-11

## Context

The customer-score provider is an outbound HTTP boundary whose configuration,
failure translation, and telemetry should not leak into route handlers.

## Decision

The API uses `IHttpClientFactory` to create a typed `CustomerScoreProviderClient`.
An application-facing gateway interface separates API contracts from HTTP details.

## Consequences

The handler only orchestrates a use case. HTTP headers, status interpretation,
and resilience behavior remain testable at the integration boundary.
