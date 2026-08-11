# Authentication Model Normalization Plan

## Purpose

This document is the working migration plan for moving the existing authentication model toward the semantic contract defined in [`AUTH-MODEL-CONTRACT.md`](./AUTH-MODEL-CONTRACT.md) without losing useful authored truth, implementation provenance, test proof, or runtime evidence.

This is a controlled normalization, not a rewrite.

The existing model already contains substantial reviewed material across categories, behaviors, scenarios, AuthViews, AuthRoutes, implementation bindings, test bindings, and platform projections. Existing data is preserved unless an explicit reconciliation decision changes its meaning.

## Migration principles

1. Preserve semantic intent before improving shape.
2. Preserve stable identifiers unless there is a demonstrated identity defect.
3. Separate authored status from execution evidence.
4. Do not mass-edit all categories to satisfy a new shape at once.
5. Normalize one category end-to-end, then use it as the specimen for the next category.
6. Prefer additive compatibility during migration; remove transitional handling only after active definitions conform.
7. Every migration decision should be explainable from the contract, schema, authored definition, implementation, or evidence.
8. Deprecated definitions remain historical evidence but do not participate in active reconciliation unless explicitly requested.

## Target graph

The normalized reconciliation graph is:

`Category -> Behavior -> Scenario -> AuthView/AuthRoute -> Platform Implementation -> Runnable Test -> Runtime Evidence`

Server behavior additionally resolves through logical actions, transitions, endpoint/handler bindings, and C# flow proof.

The target top-level status dimensions are:

- **Authored** - Git-authored category/behavior/scenario/presentation/implementation/test specification is reconciled.
- **C# Flow** - required server flow/integration proof is current and passing.
- **UI Runtime** - required Web/iOS/Android scenario executions are current and passing.

## Current document inventory

| Document type | Current root | Canonical identity today | Important references | Current migration assessment |
| --- | --- | --- | --- | --- |
| Category catalog | `behavior-category-catalog.json` | catalog `key`; category entries use local `key` | `behaviorKeys[]` | Valuable and structurally useful. Category-key naming needs an explicit contract decision. |
| Behavior V2 | `behaviors-v2/` | `key` | `categoryKey`, `scenarioKeys[]`, entry/final view keys | Strong current shape. Progress/maturity semantics need normalization review. |
| Scenario V2 | `scenarios-v2/` | `key` | `categoryKey`, `startViewKey`, `expectedViewKey`, action, transition keys, evidence requirements | Strong executable shape. Finder and typed-reference validation should be strengthened. |
| AuthView | `auth-views/` | `viewId` | `routeId`, controls, actions, platform projection | Strong shape. Must be loaded explicitly by `viewId`; control/action graph validation should become authoritative. |
| AuthRoute | `auth-routes/` | `routeId` | `viewId`, route parameters, platform projection | Strong shape. Must be loaded explicitly by `routeId`; bidirectional view/route validation required. |
| Logical action | `actions/` | `key` | transition/implementation references | Preserve. Validate typed references as graph normalization proceeds. |
| Transition | `transitions/` | `key` | logical action and state references | Preserve. Validate scenario-to-transition relationships by type. |
| Implementation bindings | `implementation/` | binding-specific `key` | endpoint, handler, action, transition, code references | Valuable provenance. Keep separate from platform runtime proof. |
| Test bindings | `implementation/tests/` | `key` | handlers, endpoints, transitions, test symbols, proof obligations | Already useful C# proof specification. Authored test progress should reflect completed proof specification independently of latest execution result. |
| JSON schemas | `schemas/` | schema `$id` / document identity fields | `$ref` relationships | Good foundation. Align only where semantic contract requires it. |
| C# evidence | `.aptix/evidence/` | run/test evidence identity | canonical test/scenario/transition references | Execution evidence only. Must never mutate authored progress implicitly. |
| UI runtime records | runtime service/storage | run/scenario/platform identity | scenario definition/version/hash, platform | Separate runtime truth. Must remain outside authored Git progress. |

## Known normalization decisions

The following issues are intentionally recorded as decisions to make, rather than silently changed during inventory.

### 1. Category identity shape

Current category entries use local keys such as `password-sign-in`, while general stable-key conventions describe `auth.*` identifiers.

We need to choose and document one of two legitimate models:

- category keys are catalog-scoped local identifiers and remain `password-sign-in`, or
- categories become globally typed identities such as `auth.category.password-sign-in`.

Until that decision is made, existing category keys remain unchanged.

### 2. Document-type-aware identity loading

AuthView and AuthRoute documents both contain `viewId` and `routeId`, but those fields have different ownership semantics.

Normalized tooling must declare the expected document type and identity field:

- AuthView -> `viewId`
- AuthRoute -> `routeId`

Property-presence precedence is prohibited.

### 3. AuthView/AuthRoute bidirectional binding

For routable views, the graph must satisfy both directions:

- `AuthView.routeId == AuthRoute.routeId`
- `AuthRoute.viewId == AuthView.viewId`

A mismatch is an authored graph defect. A missing route for an active routable view is also an authored graph defect.

### 4. Controls, actions, and finders

Controls and actions are owned by their AuthView.

For scenarios:

- `action.id` and `action.finder` must resolve to the same action on `startViewKey`.
- input finders must resolve to compatible controls on the surface where input occurs.
- expected visible finders must resolve to declared semantic finders when they refer to canonical view elements.

The finder is the cross-platform semantic execution contract; Angular and React Native conform to it.

### 5. Platform projection vocabulary

AuthView/AuthRoute schemas currently model implementation projection as `web` and `mobile`.

Runtime evidence requirements distinguish `web`, `ios`, and `android`.

These are related but not identical concepts. We need an explicit decision whether canonical implementation projection should remain `web/mobile` with iOS/Android as runtime subplatforms, or evolve to first-class `web/ios/android` projection records.

No existing platform data should be discarded during that decision.

### 6. Maturity versus authored progress

Some currently reconciled Password Sign-In definitions retain `maturity: proposed` while their scenario/presentation/implementation progress is complete.

Maturity and reconciliation progress are distinct concepts, but the intended relationship needs to be made explicit so a definition cannot appear simultaneously provisional and fully reconciled without explanation.

Existing maturity values remain untouched until that rule is agreed.

### 7. Authored Tests versus C# Flow execution

Password Sign-In has a concrete test binding with NUnit methods and proof obligations, while category/behavior/scenario authored `tests` progress remains `not-started`.

Under the accepted status model:

- authored `tests` means the test specification/proof obligation mapping is reconciled;
- C# Flow reports whether the current execution evidence passes.

Therefore authored Tests can be complete while C# Flow later fails or becomes stale.

Password Sign-In will be the first category where this distinction is normalized deliberately.

### 8. Runtime platform evidence

Scenario `evidenceRequirements` declares which proof surfaces are required. Runtime execution state must not be written into authored scenario progress.

UI Runtime rolls up only the required UI platforms and must preserve separate Web, iOS, and Android results.

## Password Sign-In specimen

Password Sign-In is the first normalization specimen because it already contains strong authored and implementation coverage.

### Existing authored graph

Category:

- `password-sign-in`

Behaviors:

- `auth.behavior.password.sign-in-success`
- `auth.behavior.password.sign-in-rejected`
- `auth.behavior.password.sign-in-locked-out`

The successful behavior composes shared navigation scenarios followed by the successful submit scenario.

Representative server-submit scenario:

- `auth.scenario.password-sign-in.password-entry-sign-in-success`
- starts at `auth.continue.email.password`
- invokes `sign-in` / `action:sign-in`
- consumes `field:password`
- requires `auth.transition.password-sign-in.success`
- ends at the resolved application destination
- requires server, Web, Android, and iOS evidence

Representative AuthView:

- identity: `auth.continue.email.password`
- route reference: `auth.route.continue.email.password`
- contains `email`, `password`, and `validation-error` controls
- contains `sign-in`, `cancel`, `forgot-password`, and `start-over` actions
- records implemented web and mobile projections

Representative AuthRoute:

- identity: `auth.route.continue.email.password`
- path: `/auth/continue/email/password`
- view reference: `auth.continue.email.password`
- records web and mobile projections

C# test binding:

- `auth.test-binding.password-sign-in`
- NUnit handler-integration proof
- covers success, non-enumerating rejection permutations, and locked-out behavior
- binds endpoint/handler/transition proof obligations to real test methods

### Initial specimen assessment

| Area | Current state | Normalization work |
| --- | --- | --- |
| Category -> Behavior | Resolves | Decide category identity convention; otherwise preserve. |
| Behavior -> Scenario | Resolves for active Password Sign-In behaviors | Add typed graph validation and exclude deprecated scenarios from active roll-ups. |
| Scenario -> start AuthView | Authored references are meaningful | Make loader document-type-aware and validate against AuthView identity. |
| Scenario -> action | `sign-in` / `action:sign-in` matches the password AuthView | Add authoritative id+finder resolution validation. |
| Scenario -> inputs | `field:password` matches the password AuthView control | Add typed finder/control validation. |
| AuthView -> AuthRoute | Representative password view points to matching route | Validate entire active graph bidirectionally. |
| AuthRoute -> AuthView | Representative password route points back to matching view | Validate entire active graph bidirectionally. |
| Angular projection | Marked implemented in authored data | Reconcile actual component, route, controls, actions, and finders against canonical model. |
| React Native projection | Marked implemented in authored data | Reconcile actual route/screen, controls, actions, and finders against canonical model. |
| Authored test specification | Concrete Password Sign-In test binding exists | Reconcile and then mark authored Tests complete independently of run outcome. |
| C# Flow | Existing evidence mechanism supports real integration proof | Validate current evidence freshness and bindings. |
| UI Runtime | Required Web/iOS/Android evidence declared | Connect runtime records to visualizer and execute required scenarios. |

## Migration phases

### Phase 0 - Contract and inventory

Status: **in progress**

- [x] Establish top-level semantic contract.
- [x] Preserve and reference existing conventions and AuthView/AuthRoute contract.
- [x] Create current-state normalization inventory.
- [ ] Resolve the explicit normalization decisions listed above.

No authored JSON shape changes should occur before the relevant decision is accepted.

### Phase 1 - Schema alignment

- Compare each active schema to the accepted semantic contract.
- Add constraints where they can safely express identity and local shape rules.
- Do not force graph rules into JSON Schema where cross-document validation is clearer.
- Preserve existing properties unless a deliberate migration decision replaces them.
- Version schemas when a change is materially incompatible.

Deliverable: schemas accurately express document-local contracts.

### Phase 2 - Authoritative graph validation

- Make loaders document-type-aware.
- Validate canonical identity fields explicitly.
- Validate Category -> Behavior -> Scenario references by expected type.
- Validate Scenario -> AuthView/action/control references.
- Validate AuthView <-> AuthRoute bidirectionally.
- Validate transition and implementation/test binding references.
- Classify findings as contract-compliant, legacy-valid, or invalid.

Deliverable: visualizer issues are trustworthy and actionable.

### Phase 3 - Password Sign-In authored normalization

- Reconcile the category key decision.
- Reconcile maturity/progress semantics.
- Reconcile active versus deprecated scenario participation.
- Confirm all active view/route/control/action/finder references.
- Reconcile authored test specification and mark authored Tests correctly.

Deliverable: Password Sign-In becomes the canonical normalized authored specimen.

### Phase 4 - Platform reconciliation

For Angular:

- route/path registration
- canonical view identity
- component mapping
- controls
- actions
- finders
- visibility/required semantics
- navigation outcome

For React Native:

- route/screen registration
- canonical view identity
- controls
- actions
- finders
- visibility/required semantics
- navigation outcome

Deliverable: platform implementation projections are proven against the canonical authored specimen.

### Phase 5 - Proof reconciliation

- Verify C# integration evidence against current definition/version/hash.
- Connect UI runtime records to canonical scenario identity.
- Execute required Web/iOS/Android scenarios.
- Record runtime evidence without changing authored Git status.

Deliverable: Password Sign-In is fully green across Authored, C# Flow, and UI Runtime.

### Phase 6 - Category-by-category migration

Repeat the proven specimen process, likely beginning with Password Recovery because it already has substantial authored and implementation reconciliation.

Each category should move independently. A category still using understood legacy semantics must not block already-normalized categories from being trustworthy.

## Definition of migration complete

The normalization effort is complete when:

- active document types have explicit canonical identity ownership;
- schemas agree with the semantic contract;
- all active typed references resolve;
- AuthView/AuthRoute graph invariants hold;
- scenario controls/actions/finders resolve canonically;
- Angular and React Native implementations are reconciled to the authored model;
- authored test specifications are distinct from C# execution status;
- UI runtime evidence is separate and platform-aware;
- active categories have migrated through the canonical process;
- transitional loader inference and legacy compatibility can be safely removed.

At that point, the authentication model is not merely a collection of useful JSON documents. It is one validated, typed, executable graph whose authored truth, implementations, and evidence can be reasoned about independently and together.
