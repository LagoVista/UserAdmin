# Authentication Model

This directory contains the Git-authoritative authentication model described by `AUTH-MODEL-GUIDE.md`.

## Ownership

- Git owns authored definitions and their review history.
- Cosmos DB owns runtime projections, execution records, evidence, and current operational status.
- Existing code and DDR text are inputs to reconciliation, not automatic authority.

## Directory layout

- `state/` composite-state dimensions and value vocabularies
- `invariants/` state, transition, and side-effect invariants
- `actions/` logical atomic actions
- `transitions/` deterministic transition rules
- `journeys/` ordered human-recognizable journeys
- `scenarios/` concrete `AppUserTestScenario`-aligned requirements
- `presentation/` platform and channel bindings, including AuthView mappings
- `schemas/` JSON Schema 2020-12 contracts
- `decisions/` reconciliation decisions and durable rationale
- `unresolved/` explicit open questions and conflicts

## Authoring rules

1. Every authored definition has a stable key, schema version, maturity, version, summary, and source references.
2. Keys are permanent identifiers. Rename display names freely, but do not reuse or casually rename keys.
3. Each scenario executes exactly one logical state-changing action.
4. Definitions reference other definitions by stable key, never by display name.
5. Runtime evidence records the exact definition hash it evaluated.
6. A changed definition makes older evidence stale until reverified.
7. Uncertainty must be recorded as proposed material or an unresolved item, never silently promoted to approved truth.

See `CONVENTIONS.md` for canonical key, versioning, normalization, hashing, reference, and validation rules.
