# ADR-0003: Apply timeout, retry, and circuit breaker policies

- Status: Accepted
- Date: 2026-07-11

## Context

An unstable provider must not indefinitely occupy request capacity, while a
temporary failure can merit a bounded retry. Repeated failures should stop traffic
before it worsens an outage.

## Decision

The typed client executes through a Polly pipeline: per-attempt timeout,
exponential backoff retry for timeouts/transient responses, and a circuit breaker.
The pipeline emits correlation-scoped timeline events and a summarized circuit
state for study.

## Consequences

The laboratory deliberately exposes latency and failure behavior. It does not
hide them with an unmarked fallback.
