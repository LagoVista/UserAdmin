# Authentication Model Contract

## Purpose

This document defines the semantic contract for the Git-authoritative authentication model as a whole.

It does not replace the detailed rules in [`CONVENTIONS.md`](./CONVENTIONS.md), [`AUTH-VIEW-ROUTE-CONTRACT.md`](./AUTH-VIEW-ROUTE-CONTRACT.md), or the JSON schemas under `schemas/`. Instead, it defines how the major authored objects relate, which identifiers are canonical, what tooling may rely on, and how authored truth is reconciled to server, Angular, React Native, test, and runtime evidence.

The model grew incrementally and already contains valuable authored data. Normalization must preserve existing authored intent, implementation references, test evidence, and runtime evidence unless an explicit reconciliation decision changes them.

## Authority layers

The authentication model has four distinct layers of truth.

1. **Canonical authored model**
   - categories
   - behaviors
   - scenarios
   - AuthViews
   - AuthRoutes
   - logical actions and transitions
   - implementation and test bindings
   - schemas and authored reconciliation progress

2. **Implementation projections**
   - server/CommonLinks and authentication implementation
   - Angular routes, components, controls, actions, and finders
   - React Native routes, screens, controls, actions, and finders

3. **C# flow proof**
   - integration/flow tests that exercise the authoritative server authentication implementation
   - Aptix evidence that binds those tests to canonical scenarios/transitions

4. **UI runtime proof**
   - execution evidence from required UI platforms such as Web, iOS, and Android
   - runtime evidence is separate from authored Git progress

No downstream projection silently overrides the canonical authored model. Differences are reconciliation findings that must be resolved explicitly.

## Canonical graph

The primary reconciliation graph is:

`Category -> Behavior -> Scenario -> AuthView/AuthRoute -> Platform Implementation -> Runnable Test -> Runtime Evidence`

Server behavior also participates through logical actions, transitions, handlers, endpoints, and C# flow proof.

A category is fully green only when its authored model is reconciled, required C# flow proof is green, and required UI runtime proof is green.

## Canonical identity

Each document type has exactly one canonical identity field. Reference fields are never used as document identity.

| Object | Canonical identity | Primary references |
| --- | --- | --- |
| Category | `key` | `behaviorKeys[]` |
| Behavior | `key` | `scenarioKeys[]` |
| Scenario | `key` | `startViewKey`, `expectedViewKey`, `action`, transition keys |
| AuthView | `viewId` | `routeId` |
| AuthRoute | `routeId` | `viewId` |
| Logical Action | `key` | transition/implementation references |
| Transition | `key` | action and state references |
| Test Binding | `key` | scenario/transition references |

### Identity rules

- `AuthView.viewId` is always the identity of an AuthView document.
- `AuthView.routeId` is a typed reference to an AuthRoute, never an AuthView identity.
- `AuthRoute.routeId` is always the identity of an AuthRoute document.
- `AuthRoute.viewId` is a typed reference to an AuthView, never an AuthRoute identity.
- Scenario `key`, Behavior `key`, Category `key`, action `key`, transition `key`, and test-binding `key` are their document identities.
- Tooling must load a document group using the identity field declared for that document type. Tooling must not infer document identity by checking which identifier-like property happens to exist.

Stable-key syntax, permanence, normalization, hashing, and general reference rules remain defined by [`CONVENTIONS.md`](./CONVENTIONS.md).

## AuthView controls and actions

An AuthView is the canonical semantic description of a user-visible authentication surface.

### Controls

Each entry in `controls[]` has a canonical identity within its AuthView:

- `id` is the stable control identity within the view.
- `controlType` describes the semantic control type.
- `finder` is the platform-neutral executable locator contract.
- `required`, `sensitivity`, and `visibilityCondition` describe canonical presentation semantics when present.

Control IDs are scoped to their AuthView. A control may reuse the same local ID on another view when it represents the same local concept, but tooling resolves a control in the context of its owning `viewId`.

### Actions

Each entry in `actions[]` has a canonical identity within its AuthView:

- `id` is the stable action identity within the view.
- `actionType` describes how the action is presented.
- `finder` is the platform-neutral executable locator contract.
- `visibilityCondition` describes canonical presentation semantics when present.

An AuthView action describes a user-invokable presentation affordance. It does not redefine authentication business rules. Logical actions, transitions, handlers, and navigation bindings provide the authoritative effect.

### Finders

Semantic finders are part of the canonical cross-platform contract.

- Authored finders use semantic namespaces such as `screen:`, `field:`, `label:`, `status:`, `display:`, and `action:`.
- Angular and React Native implementations conform to these semantic identifiers.
- Platform implementations must not invent alternate semantic identifiers for an already-defined canonical control or action.
- A platform-specific technical selector may exist internally, but canonical reconciliation and runnable tests use the semantic finder contract.

Detailed finder syntax remains defined by the AuthView and scenario schemas.

## Scenario binding rules

A scenario represents one deterministic UI/auth action from one known starting surface/state to one expected resulting surface/state.

For every active scenario:

1. `startViewKey` must resolve to exactly one canonical AuthView.
2. An auth-scoped `expectedViewKey` must resolve to exactly one canonical AuthView.
3. `action.id` and `action.finder` must resolve to one action declared by the starting AuthView.
4. Input finders must resolve to compatible controls on the surface where the input is entered.
5. Server transition references must resolve to the expected canonical transition type.
6. Evidence requirements declare which proof surfaces are required; they do not themselves record pass/fail execution state.

## AuthView/AuthRoute invariants

The detailed route contract remains in [`AUTH-VIEW-ROUTE-CONTRACT.md`](./AUTH-VIEW-ROUTE-CONTRACT.md). The following graph invariants are mandatory:

1. If an AuthView declares `routeId`, that AuthRoute must exist.
2. If an AuthRoute of type `view` declares `viewId`, that AuthView must exist.
3. For a routable view, `AuthView.routeId -> AuthRoute.routeId` and `AuthRoute.viewId -> AuthView.viewId` must agree bidirectionally.
4. A handler, redirect, entry, or logout route may legitimately have no AuthView when its route type does not present a user-visible surface.
5. Missing or mismatched route/view references are authored graph defects, not runtime failures.

## Platform reconciliation

Angular and React Native are projections of the canonical model and must be reconciled independently.

For each required platform implementation of an AuthView, reconciliation should establish:

- the screen/component exists
- it represents the correct canonical `viewId`
- required controls exist
- required actions exist
- canonical semantic finders are exposed
- required/optional and visibility semantics are honored
- navigation behavior conforms to the canonical route/view graph

For each required platform implementation of an AuthRoute, reconciliation should establish:

- the route/path registration exists
- it conforms to the canonical route path
- it binds the expected view/component when applicable
- route parameters conform to the canonical definition
- navigation reaches the canonical destination

Server/CommonLinks, Angular, and React Native projection details remain documented in [`AUTH-VIEW-ROUTE-CONTRACT.md`](./AUTH-VIEW-ROUTE-CONTRACT.md).

## Reconciliation status model

The visualizer and related tooling should present three top-level status measures.

### Authored

Answers whether the Git-authored category -> behavior -> scenario -> presentation -> implementation -> test specification is reconciled.

Authored phase progress comes only from canonical Git definitions. C# or UI runtime execution must never silently change authored progress.

### C# Flow

Answers whether required server/flow integration proof currently passes against the authoritative authentication implementation.

C# flow evidence may be associated with scenarios and transitions, but it is a separate proof dimension from authored progress.

### UI Runtime

Answers whether required UI scenarios have executed successfully on the required client platforms.

The roll-up is derived only from platforms declared by the scenario evidence requirements. Current client platforms are:

- Web
- iOS
- Android

A platform that is not required for a scenario must not prevent the scenario from becoming runtime green.

## Validation levels

Validation should be layered so findings are actionable.

1. **Schema validation**
   - each JSON document validates against its declared schema

2. **Identity validation**
   - the expected canonical identity field is present and valid for the document type
   - document loaders use that field explicitly

3. **Referential validation**
   - typed internal references resolve to exactly one definition of the expected type

4. **Graph validation**
   - Category -> Behavior -> Scenario links resolve
   - Scenario -> AuthView/action/control links resolve
   - AuthView <-> AuthRoute links agree
   - transition and implementation/test bindings resolve

5. **Platform reconciliation validation**
   - server/CommonLinks, Angular, and React Native projections conform to the authored contract

6. **Proof validation**
   - C# flow proof is current and passing where required
   - UI runtime evidence is current and passing on required platforms

Schema-valid JSON is therefore necessary but not sufficient for a reconciled authentication model.

## Migration policy

Normalization is incremental. We will not rewrite the entire authentication model in one pass.

During migration, tooling should distinguish:

- **contract-compliant**: conforms to the current semantic contract and schemas
- **legacy-valid**: existing authored data is understood and preserved but still requires normalization
- **invalid**: violates a current invariant or contains an unresolved reference/semantic defect

Migration must preserve existing authored intent, provenance, implementation references, C# evidence, and runtime evidence unless an explicit reconciliation decision changes them.

Deprecated definitions remain historical evidence but should not participate in active roll-ups unless explicitly requested.

## Migration sequence

Use this order:

1. Establish and review this semantic contract.
2. Inventory current document types, schemas, identities, references, and known inconsistencies.
3. Align JSON schemas with the accepted contract without gratuitous data-shape changes.
4. Make validators and visualizers document-type-aware and enforce typed identity/reference rules.
5. Normalize one category at a time, beginning with Password Sign-In as the first canonical specimen.
6. Reconcile its server/CommonLinks projection.
7. Reconcile its Angular implementation.
8. Reconcile its React Native implementation.
9. Confirm C# flow proof.
10. Execute and record required Web/iOS/Android runtime evidence.
11. Repeat for the next category.
12. Remove transitional compatibility only after active authored definitions have migrated.

## Relationship to other documents

- [`CATEGORY-TO-GREEN.md`](./CATEGORY-TO-GREEN.md) defines the operating process for taking one category to fully green.
- [`CONVENTIONS.md`](./CONVENTIONS.md) defines durable key, versioning, normalization, hashing, reference, maturity, and validation conventions.
- [`AUTH-VIEW-ROUTE-CONTRACT.md`](./AUTH-VIEW-ROUTE-CONTRACT.md) defines detailed AuthView/AuthRoute responsibilities and platform projection rules.
- JSON schemas under `schemas/` define machine-valid document shapes.
- This document defines how those pieces form one coherent authentication-model graph.
