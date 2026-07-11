# ADR-0004: Surface integration failures with ProblemDetails

- Status: Accepted
- Date: 2026-07-11

## Context

Clients need a stable, HTTP-native explanation when no safe fallback exists.

## Decision

The API maps timeout, throttling, provider failure, and open-circuit outcomes to
ProblemDetails. Each response includes a correlation id and failure class.

## Consequences

Clients can distinguish a usable marked fallback from a failed integration without
parsing logs or guessing from an ad-hoc JSON shape.
