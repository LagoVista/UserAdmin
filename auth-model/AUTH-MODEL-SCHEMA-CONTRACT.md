# Authentication Model Schema Contract

## Status

**Locked baseline:** 2026-08-11

This document is the human-readable master contract for the JSON Schemas under `auth-model/schemas/`.

The schema files themselves are the machine-readable authority for document shape. This document defines how those schemas are interpreted, how they relate to one another, and how changes to the schema contract are governed.

The authentication model has completed its normalization/design pass. From this baseline forward, authored JSON is expected to conform to its declared schema. Existing authored documents that predate this baseline may require migration to the standardized schema vocabulary and shape.

A validation failure is therefore presumed to be **authored-data drift**, not a reason to weaken the schema. A schema should change only when the semantic review record or an explicit new design decision proves the schema itself is wrong.

## Lock policy

The schema family is now a compatibility boundary.

1. **Do not loosen a schema merely to make an older authored file validate.** Normalize the authored file when the schema represents the accepted contract.
2. **Do not rename fields, schema files, definition roots, or canonical keys casually.** These are compatibility-sensitive across C#, Aptix, Angular, React Native, generators, validators, and runtime evidence.
3. **A breaking semantic or structural change requires an explicit decision and deliberate migration.** Prefer a new schema version when old and new meanings cannot safely coexist.
4. **Backward-compatible tightening is allowed when it makes an already-accepted semantic rule mechanically enforceable.** Examples include typed key namespaces, uniqueness, enum alignment, and graph-reference validation.
5. **JSON Schema owns local document shape. Graph validation owns cross-document truth.** A valid file can still be invalid in the authored graph if a referenced key is missing, points to the wrong document type, or violates a reciprocal relationship.
6. **Free-form semantic expressions remain semantic text unless an explicit executable grammar is introduced.** Validators must not pretend prose predicates are executable policy.
7. **Runtime execution evidence never mutates the authored schema contract or authored reconciliation progress.** Authored proof specification and current execution status are separate concerns.
8. **Deprecated definitions remain historical/provenance material and are excluded from active readiness.** They do not become alternate canonical truth.

## Decision precedence

When historical documentation conflicts, use this order:

1. this master schema contract and the schema files it documents;
2. the completed field reviews:
   - `AUTH-CATEGORY-BEHAVIOR-SCENARIO-FIELD-REVIEW.md`
   - `AUTH-VIEW-ROUTE-FIELD-REVIEW.md`
   - `AUTH-IMPLEMENTATION-PROOF-FIELD-REVIEW.md`
   - `AUTH-STATE-INVARIANT-FIELD-REVIEW.md`
   - `AUTH-ORCHESTRATION-PRESENTATION-FIELD-REVIEW.md`
3. `AUTH-MODEL-CONTRACT.md` and `AUTH-MODEL-COMPATIBILITY-MAP.md`;
4. older decision logs, migration plans, handoffs, and exploratory design documents as historical provenance.

Later field reviews intentionally supersede earlier exploratory ideas where they disagree. In particular, the current implementation/proof review keeps transition-based Scenario-to-TestBinding proof ownership and does not require a new `scenarioKeys[]` edge without a demonstrated ambiguity.

## Identity rules

### Global authored identities

Globally keyed definitions use typed `auth.*` namespaces. Important families include:

- `auth.dimension.*`
- `auth.invariant.*`
- `auth.action.*`
- `auth.transition.*`
- `auth.behavior.*`
- `auth.scenario.*`
- `auth.journey.*`
- `auth.conversation.*`
- `auth.flow.*`
- `auth.handler.*`
- `auth.endpoint.*`
- `auth.proxy.*`
- `auth.test-binding.*`

The namespace identifies the referenced document type. Validators should resolve references as typed references rather than treating every `auth.*` string as interchangeable.

Canonical presentation identities may legitimately be two segments, for example `auth.welcome`. AuthView and AuthRoute reference validation must therefore permit a root presentation identity as well as deeper identities such as `auth.continue.email.password`.

### Catalog-scoped category identities

Behavior category keys such as `password-sign-in`, `password-recovery`, and `passkey-registration` are intentionally **not** global `auth.*` keys. They are canonical within `behavior-category-catalog.json` and are referenced through typed `categoryKey` fields.

### Document-specific identities

- AuthView identity is `viewId`.
- AuthRoute identity is `routeId`.
- Behavior, Scenario, Action, Transition, Journey, Conversation, implementation binding, State Dimension, and Invariant identities are their typed `key` fields.
- `runtimeEntityId` is compatibility/runtime identity and must not replace the authored canonical key.
- Child IDs such as control IDs, action IDs, route parameter IDs, goal keys, and state value keys are local to their owning document unless their schema explicitly says otherwise.

## Active category-to-green schema family

These schemas form the active authored chain used by `CATEGORY-TO-GREEN.md`.

| Schema | Version | Purpose | Canonical identity / key relationship |
| --- | --- | --- | --- |
| `behavior-category-catalog.schema.json` | 1.0 | Inventory of authentication behavior categories and authored category reconciliation roll-ups. | Catalog key plus catalog-scoped category keys. |
| `auth-reconciliation-progress.schema.json` | 1.0 | Shared authored progress vocabulary for Category, Behavior, and Scenario reconciliation. | Embedded status contract, not runtime evidence. |
| `linear-user-behavior-v2.schema.json` | 2.0 | Ordered composition of atomic scenarios into one outcome-specific behavior. | `auth.behavior.*`; category reference plus ordered unique `scenarioKeys[]`. |
| `app-user-test-scenario-v2.schema.json` | 2.0 | One deterministic UI/auth action from a known start state/surface to an expected result. | `auth.scenario.*`; stable `runtimeEntityId`; typed view/action/input/transition relationships. |
| `auth-view.schema.json` | 1.0 | Canonical authentication presentation surface, controls, actions, and implementation reconciliation. | `viewId`; required `routeId`; view-local control/action identities and semantic finders. |
| `auth-route.schema.json` | 1.0 | Canonical authentication route plus CommonLinks, Angular, and React Native route projections. | `routeId`; optional typed `viewId`; reciprocal view-route relationship for view routes. |
| `action.schema.json` | 1.0 | Logical operation boundary: inputs, guards, permitted state mutation surface, effects, and invariants. | `auth.action.*`. |
| `transition.schema.json` | 1.0 | Deterministic modeled outcome for an Action, including state transform and outcome-specific effects. | `auth.transition.*` → one `auth.action.*`; transforms target `auth.dimension.*`. |

### Behavior V2

A Behavior is an ordered composition, not an alternative policy engine.

- `scenarioKeys[]` order is meaningful and duplicates are invalid.
- `entryViewKey` should agree with the first active scenario when present.
- `finalViewKey` should agree with the final active scenario when present.
- `supportedPlatforms[]` describes execution surfaces (`server`, `web`, `android`, `ios`, `vtm`), not implementation-projection ownership.
- `maturity`, authored `progress`, and runtime evidence are independent axes.

### Scenario V2

A Scenario is the atomic runnable authored unit.

- `startViewKey` and auth-valued `expectedViewKey` resolve to AuthViews; `app.*` denotes an application surface outside the AuthView catalog.
- `action.id` and `action.finder` must resolve to the same action on the starting AuthView.
- Scenario inputs use canonical `field:*` finders and must resolve to compatible controls where applicable.
- `serverInteraction.required` determines C# Flow applicability.
- `serverInteraction.transitionKeys[]` reference canonical Transitions.
- `expectedVisibleFinders[]` should resolve on the expected AuthView where that surface is an AuthView.
- `expectedAuthLogEvents[]` uses actual runtime audit vocabulary when server behavior is being proved.
- `evidenceRequirements[]` is the canonical source for required runtime proof surfaces.
- `sourceReferences[]` may include authored provenance such as AuthViews, Behaviors, and Journeys; provenance is not behavioral ownership.

### AuthView

AuthView defines **what exists on an authentication surface**.

- `viewId` is the canonical presentation identity.
- `routeId` is a required typed reference to its AuthRoute.
- `category` is a presentation-family classification, not a Behavior category key.
- `controls[]` and `actions[]` own stable view-local IDs and platform-neutral semantic finders.
- Finder namespaces are the canonical automation contract, not display text.
- `platforms.web` represents the Angular implementation projection.
- `platforms.mobile` represents the shared React Native implementation projection.
- Web/iOS/Android runtime evidence remains separate from these implementation projections.
- `source.runtimeEntityId` is compatibility-sensitive and retained.

### AuthRoute

AuthRoute defines **where/how the canonical authentication surface is routed**.

- `routeId` is the canonical route identity.
- `path` is the canonical authored route path.
- `routeType` is `view`, `handler`, `redirect`, `entry`, or `logout`.
- For a view route, `viewId` resolves to an AuthView and must agree bidirectionally with that AuthView's `routeId`.
- `commonLinks` is the typed server/CommonLinks projection and uses the schema-defined `path`, `member`, `value`, and conformance structure.
- `platforms.web` and `platforms.mobile` are implementation route projections, not runtime pass/fail records.

### Action

Action defines **what may be attempted and what it is allowed to affect**.

- `requiredInputs[]` are logical inputs, not UI controls.
- `guards[]` define semantic permission/precondition boundaries.
- `permittedMutations[]` contains typed canonical authored references and may be empty for an effect-only operation.
- An empty mutation set is different from an unconstrained mutation set.
- `requiredEffects[]` and `forbiddenEffects[]` are normative side-effect boundaries.
- `invariantKeys[]` resolve to canonical Invariants.

### Transition

Transition defines **one deterministic outcome and its modeled state change**.

- `actionKey` resolves to one Action.
- `sourceState` and `guards[]` select applicability/outcome semantics.
- `destinationTransform[]` is the machine-oriented state mutation contract and targets canonical `auth.dimension.*` keys.
- Transition state changes must remain within the owning Action's permitted mutation boundary.
- `requiredEffects[]` / `forbiddenEffects[]` refine the Action-level effect contract.
- `invariantKeys[]` remain enforced.

A Transition with a real state change should express that mutation in `destinationTransform[]`; effect text is not a substitute for canonical state mutation. This is why the schema retains a non-empty transform requirement.

## Implementation and proof schema family

These schemas provide independently verifiable projections from canonical behavior to concrete code and tests.

| Schema | Version | Purpose |
| --- | --- | --- |
| `auth-flow.schema.json` | 1.0 | Stable application-level authentication operation and allowed outcome set. |
| `auth-flow-handler.schema.json` | 1.0 | Concrete typed handler/interface/method implementing flow behavior. |
| `auth-endpoint-binding.schema.json` | 1.0 | Concrete HTTP endpoint projection and public transport contract. |
| `auth-proxy-binding.schema.json` | 1.0 | Generated/client proxy projection of an authentication endpoint/operation. |
| `auth-test-binding.schema.json` | 1.0 | Authored proof specification tying real tests to implementation paths, transitions, and proof obligations. |

Repeated transition references across Flow, Handler, Endpoint, and Test Binding are intentional conformance assertions. They are not duplicate Transition definitions.

Test Binding `testLevel` uses the standardized proof vocabulary:

- `flow-matrix`
- `handler-integration`
- `endpoint-integration`
- `ui-journey`

`testMethodNames[]` names real runnable proof methods and is non-empty for an implemented runnable binding. `proofObligations[]` defines what authored Tests completeness means. Latest test execution success/failure is separate C# Flow evidence.

Scenario-to-TestBinding proof ownership is resolved through typed implementation/transition relationships unless a concrete ambiguity requires a future explicit edge.

## State and invariant schema family

| Schema | Version | Purpose |
| --- | --- | --- |
| `state-dimension.schema.json` | 1.0 | Canonical vocabulary and authority for one authentication state dimension. |
| `composite-state-catalog.schema.json` | 1.0 | Organizational regions and coarse typed dependencies among State Dimensions. |
| `invariant.schema.json` | 1.0 | Canonical cross-state, transition, or side-effect rule. |
| `invariant-catalog.schema.json` | 1.0 | Inventory/grouping of Invariant definitions. |

State Dimension keys use `auth.dimension.*`. Authorities remain distinct:

- `canonical`
- `session-projection`
- `presentation-context`

Composite dependencies are descriptive graph relationships, not executable replacements for Invariants. The relationship vocabulary is intentionally small and schema-governed.

Invariant `relatedDimensionKeys[]` resolves specifically to State Dimensions. Free-form invariant predicates/rules are normative semantic text unless and until a separate executable grammar is deliberately introduced.

## Orchestration and presentation-adaptation schemas

These schemas are intentionally subordinate to the canonical behavioral chain:

**State / Invariant → Action → Transition → Scenario → Behavior**

| Schema | Version | Purpose |
| --- | --- | --- |
| `journey.schema.json` | 1.0 | Ordered composition of scenarios into a larger proposed workflow. |
| `conversation-type.schema.json` | 1.0 | Bounded conversational orchestration, routing, safe information collection, and secure-interaction boundaries. |
| `presentation-binding.schema.json` | 1.0 | Platform presentation binding to a canonical Action or UI-only navigation relationship. |

Journeys, Conversations, and Presentation Bindings may compose, route, explain, or adapt canonical behavior. They may **not** redefine authentication guards, transitions, state changes, canonical outcomes, or security policy.

Existing proposed Journey/Conversation material may intentionally reference the retained legacy Scenario catalog. Do not automatically rewrite those references to Scenario V2. That migration is a separate deliberate reconciliation activity.

## Retained legacy/reference schemas

The repository intentionally retains older authored models as historical/proposed reference material.

| Schema | Purpose |
| --- | --- |
| `scenario.schema.json` | Legacy Scenario contract retained for historical/proposed Journey and Conversation references. |
| `end-to-end-behavior.schema.json` | Earlier end-to-end Behavior representation retained as authored history/reference. |

These retained schemas do not replace the V2 Category → Behavior → Scenario working set used for current category-to-green reconciliation.

## Structural/meta schemas

| Schema | Purpose |
| --- | --- |
| `model-manifest.schema.json` | Structural model metadata: roots, schema locations, validation policy, conventions, and provenance. It is not a live readiness dashboard. |
| `auth-reconciliation-progress.schema.json` | Shared authored reconciliation state vocabulary. Runtime execution status must not be written here. |

## Cross-document graph rules

JSON Schema validation is necessary but not sufficient. The graph validator should enforce at least:

1. Category ↔ Behavior category membership agreement.
2. Behavior → Scenario references resolve, are unique, remain ordered, and form a coherent active chain.
3. Behavior entry/final views agree with the first/last active Scenario where declared.
4. Scenario category references resolve.
5. Scenario start/expected AuthViews resolve when the value is `auth.*`.
6. Scenario action ID and finder resolve together on the starting AuthView.
7. Scenario input finders resolve to compatible starting-view controls.
8. Scenario transition references resolve.
9. Expected visible finders resolve on the expected AuthView where applicable.
10. AuthView ↔ AuthRoute reciprocal relationships agree.
11. AuthView control/action IDs and semantic finders are unique in their owning view.
12. State Dimension references resolve from composite state, Invariants, Actions, and Transitions.
13. Transition `actionKey` resolves and Transition mutations/effects remain compatible with the Action boundary.
14. Flow/Handler/Endpoint/Proxy/Test Binding references resolve and their repeated contracts/outcome sets are mutually compatible.
15. Required server proof can be resolved from a server-interacting Scenario through its canonical implementation/transition relationships to an authored Test Binding.
16. Deprecated definitions are excluded from active readiness/runnable inventories while remaining inspectable as history.

## Reconciliation versus runtime evidence

Three axes remain deliberately separate:

- **Maturity**: lifecycle/acceptance of the authored definition.
- **Authored reconciliation progress**: whether downstream authored layers have been reviewed and agreed.
- **Runtime evidence**: whether the current C# Flow and UI Runtime proof succeeds now.

A passing test does not make authored Tests complete. A failing test tomorrow does not automatically reopen an already-reconciled test specification. Material definition changes may make prior runtime evidence stale.

## Schema changes after this lock

A future schema change must be classified before implementation:

### Correction

The schema contradicts the accepted semantic contract. Correct the schema, document why, and migrate nothing unless the correction exposes authored drift.

### Tightening

The schema is made more mechanically precise without changing accepted meaning. Update validators and normalize authored data that violates the already-existing semantic rule.

### Extension

A backward-compatible optional capability is introduced for a demonstrated need. Document ownership and graph semantics before use.

### Breaking change

Existing valid authored data changes meaning or can no longer be represented. Require an explicit decision, compatibility review, migration plan, and normally a new schema version.

No schema may be widened simply because one historical JSON document is inconvenient to migrate.

## Cleanup rule from this baseline

From this locked baseline, the repository cleanup process is mechanical:

1. run JSON Schema validation across the repository;
2. group failures by schema/error family;
3. normalize authored JSON to the declared schema;
4. run graph validation;
5. resolve genuine semantic contradictions rather than adding aliases;
6. repeat until the authored corpus is schema-clean and graph-clean.

Once the corpus is clean, schema validation becomes a permanent guardrail against reintroducing historical drift.
