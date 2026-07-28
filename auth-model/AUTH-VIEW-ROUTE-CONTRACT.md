# AuthView and AuthRoute Contract

## Authority

The canonical AuthView and AuthRoute specifications stored under `auth-model` are the source of truth.

- Cosmos AuthView entities are runtime projections.
- `CommonLinks` is the server-side projection of canonical routes.
- `provideAuthRoutes()` is the Angular projection of canonical routes.
- React Native routing and test identifiers are mobile projections.
- Runtime entity ownership, audit, revision, rating, label, and Cosmos metadata do not belong in the canonical specification.

## Identifier rules

- View IDs use dotted `auth.*` keys whose segments are kebab-case.
- Route IDs use dotted `auth.*` keys whose segments are kebab-case.
- Control IDs and action IDs use kebab-case.
- Semantic finders use `<kind>:<kebab-case-id>`.
- Supported finder kinds are `screen`, `field`, `label`, `status`, `display`, and `action`.
- Identifiers are corrected before lock. No backward-compatibility aliases are required for the current normalization pass.

## AuthView responsibility

An AuthView defines a user-visible authentication surface:

- semantic identity
- category and lifecycle status
- related canonical route, when routable
- controls and their interaction semantics
- actions available to the user
- web and mobile implementation status
- implementation provenance useful for reconciliation

AuthView does not define the business effect of an action. Presentation bindings connect view actions to canonical authentication actions or UI navigation.

## AuthRoute responsibility

An AuthRoute defines a canonical authentication navigation or handler surface:

- route identity
- canonical `/auth/...` path template
- route type
- related AuthView, when the route presents a view
- route parameters
- web and mobile implementation status
- `CommonLinks`, Angular, and mobile provenance

Route types are `view`, `handler`, `redirect`, `entry`, and `logout`. Not every route presents an AuthView.

## Projection rules

Every active canonical AuthRoute must be represented in `CommonLinks`.

Every web-supported canonical AuthRoute must be represented by `provideAuthRoutes()` or an explicitly documented web projection.

Every implemented AuthView must expose its canonical screen, control, and action identifiers in the platform implementation.

Equivalent web and mobile surfaces use the same semantic identifiers. Platform-specific controls or routes are represented as platform details, not separate semantic views unless the user experience is materially different.

## Reconciliation sequence

1. Establish canonical AuthView and AuthRoute specifications.
2. Reconcile required authentication surfaces against the Angular components and route table.
3. Capture all meaningful controls, actions, and conditional states.
4. Normalize view, route, control, action, and finder identifiers.
5. Patch the canonical specifications, `CommonLinks`, Angular routing, and Angular components together.
6. Reconcile React Native against the completed canonical contract and classify each surface as implemented, partial, planned, unsupported, or not applicable.
7. Project canonical specifications into runtime entities when runtime consumption requires them.
