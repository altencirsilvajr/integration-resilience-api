# ADR-0005: Use a local Blazor learning dashboard for resilience scenarios

- Status: Accepted
- Date: 2026-07-11

## Context

Retry and circuit transitions are difficult to understand from an HTTP response
alone. A UI could be mistaken for expansion into a customer product.

## Decision

Ship a local Blazor dashboard that calls the real API and displays real payloads,
ProblemDetails, correlation timeline, and circuit state. It is a guided learning
instrument, not a public UI or a second implementation of business rules.

## Consequences

The UI owns only presentation and request orchestration. Its local-only purpose
and explicit scope are kept visible in the README and dashboard.
