# Authentication Model

This directory contains the Git-authoritative authentication model.

For active category reconciliation, start with [`CATEGORY-TO-GREEN.md`](./CATEGORY-TO-GREEN.md).

## Ownership

- **Git owns authored truth**: categories, V2 behaviors, V2 scenarios, AuthViews, AuthRoutes, actions, transitions, implementation bindings, schemas, review history, and authored reconciliation progress.
- **Runtime execution status is separate from Git**. Test execution may currently be persisted in Cosmos DB, but running a test must never require changing an authored definition or committing pass/fail state.
- Existing code, generated contracts, DDR material, historical handoffs, and runtime results are evidence used to reconcile the canonical model. They do not silently override it.

## Current working set

The current category-to-green process uses:

- `behavior-category-catalog.json` - category inventory and authored roll-up progress
- `behaviors-v2/` - current linear outcome-specific behaviors
- `scenarios-v2/` - current independently runnable UI/auth scenarios
- `auth-views/` - canonical authentication surfaces
- `auth-routes/` - canonical authentication routes
- `actions/` - logical atomic actions
- `transitions/` - deterministic state transitions
- `implementation/` - proxy, endpoint, flow, handler, and test bindings
- `schemas/` - JSON Schema contracts
- `state/` and `invariants/` - canonical authentication state and invariant vocabulary

Legacy `behaviors/` and `scenarios/` remain reference material. New category reconciliation uses the V2 roots declared by `model-manifest.json`.

## `model-manifest.json`

`model-manifest.json` is **structural metadata, not operational status truth**.

Use it for:

- definition-root locations
- schema locations
- validation policy
- model-level conventions and provenance

Do **not** use its `inventory` or `currentWork` values as a dashboard, readiness calculation, or current execution status. Those fields are retained as historical snapshot metadata and may be stale. The manifest carries a machine-readable `metadataPolicy` that states this explicitly.

Once the model shape stabilizes, CI may derive or validate inventory and roll-up metadata directly from the repository. Until then, detailed JSON definitions and current runtime execution evidence remain authoritative for their respective concerns.

## Green means two things

Authored progress and runtime execution are deliberately separate.

**Authored green** means the canonical category/behavior/scenario/presentation/implementation/test specification is complete and reconciled.

**Runtime green** means the required current scenario executions pass on the required platform(s), including the expected UI outcome and server-side receipt.

A category is fully green only when both are true.

See [`CATEGORY-TO-GREEN.md`](./CATEGORY-TO-GREEN.md) for the full process.

## Authoring rules

1. Every authored definition has a stable key, schema version, maturity, version, summary, and source references where evidence exists.
2. Keys are permanent identifiers. Rename display names freely, but do not reuse or casually rename keys.
3. Each V2 scenario represents one deterministic UI action from one known starting surface/state to one resulting surface/state.
4. Definitions reference other definitions by stable key, never by display name.
5. Runtime evidence should identify the definition version/hash or Git revision it evaluated.
6. A materially changed definition makes older runtime proof stale until reverified.
7. Conversation definitions may select and orchestrate journeys, but cannot redefine authentication guards, transitions, or postconditions.
8. Secrets and credential proofs are collected only through deterministic secure components, never through conversational context or authored JSON.
9. Uncertainty must be recorded as proposed material or an unresolved item, never silently promoted to approved truth.

See [`CONVENTIONS.md`](./CONVENTIONS.md) for canonical key, versioning, normalization, hashing, reference, and validation rules.

## Documentation map

Use these documents in this order for current work:

1. `AUTH-MODEL-CONTRACT.md` - semantic graph, canonical identities, controls/actions/finders, platform reconciliation, validation layers, and migration policy
2. `AUTH-MODEL-COMPATIBILITY-MAP.md` - consumer dependencies, minimal-change review rules, and compatibility guardrails for C#, Aptix, Angular, React Native, schemas, and authored JSON
3. `AUTH-CATEGORY-BEHAVIOR-SCENARIO-FIELD-REVIEW.md` - field-by-field semantics and tightening candidates for the existing category, Behavior V2, and Scenario V2 contracts
4. `AUTH-VIEW-ROUTE-FIELD-REVIEW.md` - field-by-field semantics and Keep / Clarify / Validate / Consolidate / Migrate classifications for the existing AuthView/AuthRoute contracts
5. `AUTH-IMPLEMENTATION-PROOF-FIELD-REVIEW.md` - field semantics and cross-layer conformance rules for Action, Transition, Flow, Handler, Endpoint, and Test Binding contracts
6. `AUTH-STATE-INVARIANT-FIELD-REVIEW.md` - field semantics and tightening candidates for State Dimensions, the composite state catalog, Invariants, and their Action/Transition relationships
7. `AUTH-MODEL-MIGRATION-PLAN.md` - current-state inventory, normalization decisions, Password Sign-In specimen, and incremental migration phases
8. `CATEGORY-TO-GREEN.md` - current operating process
9. `CONVENTIONS.md` - durable model rules
10. `AUTH-VIEW-ROUTE-CONTRACT.md` - detailed canonical presentation/routing contract
11. `AUTH-BEHAVIOR-RECONCILIATION-RUNBOOK.md` - detailed implementation and evidence lessons, including Password Management
12. dated evidence handoffs, `AUTH-IMPLEMENTATION-PLAN.md`, and `SECTION-*.md` - historical context

Historical documents are useful provenance, but they may describe older Cosmos projection or evidence workflows that are no longer the active authoring/runtime boundary.
