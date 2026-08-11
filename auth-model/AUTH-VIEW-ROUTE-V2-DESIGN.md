# AuthView and AuthRoute V2 Design

## Purpose

This document is the working design for the normalized AuthView/AuthRoute v2 schema family.

It translates the accepted model-level decisions in `AUTH-MODEL-CONTRACT.md` and `AUTH-MODEL-DECISIONS.md` into a concrete presentation/routing shape before schemas or authored definitions are migrated.

This is a design target, not yet a schema. Existing v1 AuthView/AuthRoute documents remain valid during migration.

## Design goals

V2 should:

1. make document identity unambiguous;
2. use the common authored-definition lifecycle instead of a presentation-specific lifecycle vocabulary;
3. distinguish canonical presentation truth from implementation projection truth;
4. distinguish implementation projection targets from runtime execution platforms;
5. make controls, actions, and semantic finders deterministic across Angular and React Native;
6. make implementation reconciliation freshness measurable against the authored definition;
7. eliminate duplicated provenance and runtime-projection metadata from canonical presentation documents;
8. preserve all useful v1 information through deliberate migration rather than silent loss.

## 1. Authored-definition lifecycle

### Current v1 seam

AuthView/AuthRoute currently use document-level:

- `status: proposed | active | deprecated | retired`

Most other authored definitions use:

- `version`
- `maturity: proposed | reviewed | approved | implemented | verified | deprecated`

The presentation-specific `status` vocabulary overlaps with the common authored lifecycle and creates a second definition-lifecycle model.

### V2 direction

AuthView and AuthRoute v2 should use:

- `version`
- `maturity`

and should remove the document-level v1 `status` property.

Projection-level status remains separate because statements such as "Angular implemented" or "React Native partial" describe implementation conformance, not authored-definition maturity.

### Migration rule

Do not mechanically map `status: active` to a particular maturity value.

`active` only says that the v1 surface was considered current. During category migration, each AuthView/AuthRoute receives a deliberate v2 maturity based on its authored review state.

`deprecated` v1 definitions migrate to historical/deprecated v2 definitions where appropriate. No current authored AuthView/AuthRoute usage was found that requires preserving a distinct `retired` lifecycle state.

## 2. Canonical identity

AuthView identity remains:

- `viewId`

AuthRoute identity remains:

- `routeId`

Reference fields never act as document identity.

A loader must know the document type being loaded and use the corresponding identity field explicitly.

## 3. AuthView classification

### Current v1 seam

AuthView currently has `category` values such as:

- `entry`
- `password`
- `provider`
- `passkey`
- `totp`
- `recovery`
- `registration`
- `invitation`
- `email-verification`
- `organization`
- `session`

This is useful classification, but it is not the behavior category referenced by `Behavior.categoryKey` and `Scenario.categoryKey`, which contains values such as `password-sign-in` and `password-recovery`.

### V2 direction

Preserve the classification but rename it to avoid the overloaded word `category`.

Working name: `authArea`.

`authArea` classifies the semantic presentation family of a surface. It does not link the surface to one behavior category, because one AuthView may participate in multiple behaviors/categories.

## 4. Screen identity contract

The UI runner already defines a strong cross-platform screen-root convention:

- screen root selector: `[data-testid="auth-screen"]`
- semantic identity attribute: `data-screen-id`
- attribute value: canonical `viewId`

V2 should make this the canonical AuthView implementation contract.

A platform implementation conforms to AuthView identity when its auth-screen root exposes the canonical `viewId` through this contract or the equivalent native test-automation projection defined for React Native.

### `screen:` finder namespace

Current finder syntax permits `screen:*`, but active AuthView definitions do not appear to author per-view screen finders and the runner does not require them.

Unless a real consumer is identified during migration, v2 should remove `screen:` from the authored control/action finder vocabulary rather than preserve unused semantic vocabulary.

The view itself is identified by `viewId`; controls/actions are identified by their semantic finders.

## 5. Controls

Controls remain owned by the AuthView.

Each control keeps:

- `id`
- `name`
- `controlType`
- `finder`
- optional `required`
- optional `sensitivity`
- optional `visibilityCondition`

`id` is unique within the owning AuthView.

The canonical executable identity is the semantic finder in the context of the owning view.

A scenario input must resolve to a compatible control declared by its applicable starting surface.

## 6. Actions

Actions remain owned by the AuthView.

Each action keeps:

- `id`
- `name`
- `actionType`
- `finder`
- optional `visibilityCondition`

A scenario action must resolve by both `id` and `finder` against the starting AuthView.

The view action is a presentation affordance. Its server effect remains governed by logical actions/transitions/handlers and related implementation bindings.

## 7. Implementation projection targets

V1 uses `platforms.web` and `platforms.mobile`.

V2 should model implementation ownership explicitly rather than using runtime-platform terminology.

### AuthView implementation targets

Working targets:

- `angular`
- `reactNative`

An AuthView generally does not need a `server` implementation projection because it is a UI semantic surface.

### AuthRoute projection targets

Working targets:

- `commonLinks`
- `angular`
- `reactNative`

These are projections of one canonical AuthRoute.

Runtime execution remains separate:

- Web
- iOS
- Android

React Native is reconciled once as an implementation, then independently executed on iOS and Android when required.

## 8. Projection record

Each implementation/projection target should be able to record both location and reconciliation state.

The v2 projection record should support, as applicable:

- `status`: implemented / partial / planned / unsupported / not-applicable
- `repository`
- `path`
- `component`, `route`, or `member` as target-specific provenance
- `conformance`

### Conformance receipt

Conformance must be tied to the authored definition it reviewed so staleness can be detected.

The receipt should include:

- `status`: verified / needs-review / mismatch / not-applicable
- `checkedUtc`
- `checkedAgainst[]`
- authored definition `version`
- authored `definitionHash` when available
- optional source commit/revision used for implementation reconciliation
- notes

For AuthView, `checkedAgainst` should support at least:

- view identity
- controls
- actions
- finders
- visibility/required semantics
- navigation

For AuthRoute, `checkedAgainst` should support at least:

- canonical path
- route registration
- view binding
- parameters
- navigation

A conformance receipt whose recorded authored version/hash no longer matches the current definition is stale even if its previous status was verified.

## 9. Canonical provenance

### Current v1 seam

AuthView currently duplicates implementation provenance between:

- `platforms.web/mobile`
- `source.webComponent/mobileComponent`

and may carry runtime-projection metadata such as:

- `source.runtimeEntityId`
- `source.runtimeSha256Hex`

AuthRoute similarly duplicates projection provenance between `platforms` and `source` fields.

The existing AuthView/AuthRoute contract already states that runtime entity ownership and Cosmos/runtime projection metadata do not belong in the canonical specification.

### V2 direction

Move implementation provenance into the explicit implementation/projection target records.

Do not carry runtime entity IDs or runtime projection hashes in canonical AuthView/AuthRoute v2 documents.

Use standard `sourceReferences` for historical/design provenance that is not itself an implementation projection location.

Existing v1 source data is preserved during migration and mapped into the appropriate v2 projection/source-reference location where it remains useful.

## 10. AuthView/AuthRoute binding

For routable AuthViews:

- `AuthView.routeId` resolves to one AuthRoute;
- that AuthRoute has `routeType: view`;
- `AuthRoute.viewId` resolves back to the AuthView;
- the relationship is bidirectionally consistent.

Non-view AuthRoutes may legitimately have no `viewId`.

Cross-document graph validation owns this invariant; JSON Schema should not attempt to resolve repository-wide references itself.

## 11. Route semantics

AuthRoute keeps the strong v1 concepts:

- `routeId`
- `name`
- optional `description`
- canonical `path`
- `routeType`
- optional `viewId`
- `parameters[]`
- `version`
- `maturity`
- implementation projections
- source references / notes where useful

The canonical route path is authored truth. Angular, React Native, and CommonLinks are reconciled projections of it.

## 12. Definition hash

The canonical definition hash remains derived according to `CONVENTIONS.md`.

The AuthView/AuthRoute source file does not need to persist a hash merely to be canonical; tooling may compute it.

Projection conformance receipts may record the hash they were checked against so tooling can detect staleness.

## 13. Relationship to authored reconciliation progress

AuthView/AuthRoute documents describe presentation/routing truth and their implementation projections.

Category/Behavior/Scenario `progress.presentation` and `progress.implementation` remain the authored roll-up mechanism.

A projection conformance receipt is evidence supporting reconciliation. It must not automatically mutate category/behavior/scenario progress.

The reviewer/tool may use conformance findings to decide that a phase is ready to mark complete, but the authored progress transition remains explicit.

## 14. Migration strategy

1. Finalize this v2 design.
2. Create versioned AuthView/AuthRoute v2 schemas.
3. Teach tooling to load v1 and v2 explicitly by document type/schema version.
4. Migrate only the AuthViews/AuthRoutes required by Password Sign-In.
5. Reconcile those v2 definitions against Angular and React Native implementations.
6. Validate routes against CommonLinks and platform routing.
7. Complete Password Sign-In authored presentation/implementation reconciliation under the v2 contract.
8. Repeat category by category.
9. Remove v1 support only after all active presentation/routing definitions have migrated.

## Open questions to resolve before schema authoring

1. Confirm `authArea` as the final name for the broad AuthView classification.
2. Confirm whether `screen:` can be removed from the v2 finder vocabulary.
3. Confirm exact JSON nesting/naming for `angular`, `reactNative`, and `commonLinks` projection records.
4. Decide whether implementation source commit belongs directly in conformance or in a nested implementation revision object.
5. Decide whether AuthView/AuthRoute v2 should require `sourceReferences` or allow them to be omitted when no provenance beyond implementation projections exists.
