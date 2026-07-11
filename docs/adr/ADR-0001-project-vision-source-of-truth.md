# ADR-0001: Project vision is the source of truth

- Status: Accepted
- Date: 2026-07-11

## Context

This laboratory has intentionally narrow scope. Without an explicit scope owner,
demonstration features can quietly become product features.

## Decision

`PROJECT_VISION.md` is written before implementation and is the authoritative
statement of purpose, constraints, non-goals, and success criteria.

## Consequences

Implementation and documentation must trace back to the vision. Scope changes
require changing the vision and recording an ADR when architectural.
