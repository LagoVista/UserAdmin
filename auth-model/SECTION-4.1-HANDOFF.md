# Section 4.1 Handoff: Repository Model Foundation

Status: Complete

## Scope completed

- Established the Git-authoritative `auth-model` directory.
- Added the model manifest and schema inventory.
- Defined stable-key, maturity, versioning, source-reference, normalization, hashing, and validation conventions.
- Added JSON Schema 2020-12 contracts for the model manifest, state dimensions, invariants, actions, transitions, journeys, scenarios, and presentation bindings.
- Separated requirement truth from presentation bindings and runtime evidence.

## Decisions made

- Authored definitions use JSON Schema draft 2020-12.
- Stable keys use lowercase period-separated namespaces with kebab-case segments.
- Keys are permanent identities and are not reused.
- Definition maturity is separate from execution/evidence status.
- Runtime evidence must reference the exact canonical definition hash.
- Canonical hashes use SHA-256 over normalized UTF-8 JSON excluding `definitionHash`.
- Internal references use stable keys rather than display names.
- Ambiguous approved transitions are invalid unless guards or explicit priority prove determinism.

## Invariants added or changed

- No behavioral invariants were authored in this section.
- Repository-level validation rules now enforce one action per transition/scenario, reference resolution, unique keys, and deterministic approved transitions.

## Definitions created or updated

- `auth-model/README.md`
- `auth-model/CONVENTIONS.md`
- `auth-model/model-manifest.json`
- `auth-model/schemas/model-manifest.schema.json`
- `auth-model/schemas/state-dimension.schema.json`
- `auth-model/schemas/invariant.schema.json`
- `auth-model/schemas/action.schema.json`
- `auth-model/schemas/transition.schema.json`
- `auth-model/schemas/journey.schema.json`
- `auth-model/schemas/scenario.schema.json`
- `auth-model/schemas/presentation-binding.schema.json`

## Implementation files inspected or changed

- Inspected `AUTH-MODEL-GUIDE.md`.
- No runtime implementation files changed.

## Scenarios completed

- None. Scenario authoring begins after state dimensions, invariants, and action vocabulary exist.

## Evidence completed

- Definition schemas and validation expectations are authored.
- Automated schema and referential validation tooling has not yet been implemented.

## Known DDR conflicts

- None identified in this foundation section.

## Known implementation conflicts

- None identified in this foundation section.

## Open questions

- Whether source definitions should carry computed `definitionHash` or leave it exclusively to tooling and runtime projections.
- Whether predicate and transformation expressions should remain a constrained textual DSL or become structured expression trees.
- Whether schema definitions should later share common `$defs` through a common schema file.

## Completion criteria remaining

- None for the authored repository foundation.
- Validation tooling is intentionally deferred until concrete definitions exist to validate.

## Recommended next action

- Begin Section 4.2, Composite identity state dimensions.
- Read `AUTH-MODEL-GUIDE.md`, `auth-model/model-manifest.json`, `auth-model/CONVENTIONS.md`, the state-dimension schema, and this handoff before authoring dimensions.
