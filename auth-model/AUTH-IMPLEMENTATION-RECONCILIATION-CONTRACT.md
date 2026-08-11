# Authentication Implementation Reconciliation Contract

## Purpose

This document defines how canonical AuthViews and AuthRoutes are reconciled against their implementation projections in Angular, React Native, and server/CommonLinks.

Implementation existence is not sufficient. Reconciliation proves that a concrete implementation conforms to a specific authored definition version/hash and records enough provenance to detect later staleness.

This document complements:

- `AUTH-MODEL-CONTRACT.md`
- `AUTH-PRESENTATION-ELEMENT-CONTRACT.md`
- `AUTH-VIEW-ROUTE-V2-DESIGN.md`

## Principle

Canonical authored truth and implementation truth are separate.

A projection may exist and compile while still differing from the current AuthView/AuthRoute contract.

Therefore each projection has two independent concepts:

1. **implementation status** - whether an implementation exists and how complete it is;
2. **conformance receipt** - whether that implementation was reconciled against a specific authored definition.

A projection is not considered reconciled merely because its implementation status is `implemented`.

## Implementation targets

### AuthView

AuthView implementation targets are:

- `angular`
- `reactNative`

### AuthRoute

AuthRoute projection targets are:

- `commonLinks`
- `angular`
- `reactNative`

These are implementation ownership targets, not runtime execution platforms.

Runtime execution remains separately recorded as Web, iOS, and Android evidence.

## Projection record

Each implementation projection should support:

- `status`
- `repository`
- `path`
- optional implementation symbol/provenance fields appropriate to the target
- optional `conformance`

Implementation status values remain:

- `implemented`
- `partial`
- `planned`
- `unsupported`
- `not-applicable`

`implemented` means the projection exists and is believed complete. It does not mean it has been reconciled against the current canonical definition.

## Conformance receipt

A conformance receipt records a deliberate comparison between one implementation projection and one authored definition.

It should contain:

- `status`
- `checkedUtc`
- `authoredVersion`
- `authoredDefinitionHash`
- `implementationCommit`
- `checkedAgainst[]`
- optional `notes`

Conformance status values:

- `verified`
- `needs-review`
- `mismatch`
- `not-applicable`

### Freshness

A previously verified receipt is stale when either:

- the current authored semantic `version` differs from `authoredVersion`; or
- the current computed authored definition hash differs from `authoredDefinitionHash`.

A changed implementation commit does not automatically prove non-conformance, but it means the recorded receipt no longer proves the new implementation revision. Tooling should surface the receipt as needing review when the implementation provenance being evaluated is newer/different than the receipt's `implementationCommit`.

Conformance status never changes authored reconciliation progress automatically.

## AuthView reconciliation dimensions

An AuthView projection may be verified only after the applicable dimensions have been checked.

Canonical dimensions are:

- `view-identity`
- `controls`
- `actions`
- `finders`
- `visibility-required-semantics`
- `navigation`

### View identity

The implementation exposes the canonical AuthView `viewId` through the auth-screen identity contract.

### Controls

Every canonical required control is represented by the implementation unless an explicit platform-specific non-applicability rule exists.

Additional implementation controls are not automatically harmless. They must be classified as one of:

- legitimate implementation-only detail that does not change semantic behavior;
- missing canonical authored truth that should be added to the AuthView;
- obsolete/incorrect implementation behavior that should be removed.

Tooling must not silently ignore extra semantic controls.

### Actions

Every canonical AuthView action is implemented when applicable.

Every additional user-visible semantic action exposed by the implementation must be reconciled explicitly rather than ignored.

An extra action may reveal:

- an incomplete AuthView;
- obsolete implementation behavior;
- a platform-specific behavior that needs an explicit canonical decision.

### Finders

Canonical semantic finders must match exactly.

A control/action with the correct visual meaning but a different semantic finder is a conformance mismatch until reconciled.

### Visibility and required semantics

The implementation should honor required/optional semantics and the intended visibility guidance for the states being reconciled.

Human-readable visibility guidance is not an executable predicate. Scenario runtime evidence provides state-specific proof of visibility where required.

### Navigation

Local actions that navigate must reach the canonical AuthRoute/AuthView destination described by the scenario/route graph.

## AuthRoute reconciliation dimensions

Canonical route dimensions are:

- `canonical-path`
- `route-registration`
- `view-binding`
- `parameters`
- `navigation`

### CommonLinks

For server/CommonLinks projection, reconciliation verifies the canonical route is represented by the expected member/value and matches the canonical route path.

### Angular

Angular route reconciliation verifies the registered route/path, bound component, parameters, and navigation behavior.

### React Native

React Native route reconciliation verifies the Expo/native route path, bound screen/component, parameters, and navigation behavior.

## Difference classification

When authored truth and implementation differ, the reconciler must classify the difference before changing either side.

Allowed finding classes:

- **authored-gap** - implementation exposes legitimate intended behavior missing from canonical authored truth;
- **implementation-drift** - implementation contains behavior that conflicts with accepted canonical truth;
- **identifier-drift** - semantic meaning agrees but canonical identity/finder/path differs;
- **platform-exception** - a genuine platform-specific distinction that requires explicit documentation/modeling;
- **obsolete-legacy** - old implementation behavior retained after canonical model evolution;
- **needs-decision** - intent cannot yet be established from current evidence.

Reconciliation never assumes the Git side or implementation side is correct merely because one is newer.

## Password Sign-In specimen findings

The first reality check against Password Sign-In already demonstrates why this contract is needed.

### Angular password-entry surface

Canonical AuthView:

- `auth.continue.email.password`
- canonical actions include `sign-in`, `cancel`, `forgot-password`, and `start-over`
- Magic Link is intentionally excluded from the V2 Password Sign-In authored surface

Current Angular implementation:

- correctly exposes `data-testid="auth-screen"`
- correctly exposes `data-screen-id="auth.continue.email.password"`
- correctly exposes `field:email`
- correctly exposes `field:password`
- correctly exposes `status:validation-error`
- correctly exposes `action:sign-in`
- correctly exposes `action:cancel`
- correctly exposes `action:forgot-password`
- correctly exposes `action:start-over`
- additionally exposes `action:send-magic-link`

The extra Magic Link action is a reconciliation finding. It must be classified rather than silently ignored. Based on the current accepted Password Sign-In authored decision, it is a likely implementation-drift/obsolete-legacy candidate, but the migration pass will make that decision explicitly before changing the client.

### React Native semantic ID catalog

The current React Native `AuthTestIds` catalog contains many semantic screen IDs and finders that align with the canonical model, including:

- `auth.continue.email`
- `auth.continue.email.password`
- `auth.sign-in.locked-out`
- `action:sign-in`
- `field:email`
- `field:password`

It also contains legacy identifier drift, including:

- `auth.signin-unable` where the canonical AuthView is `auth.sign-in.unable`

Other older screen IDs should be reconciled category-by-category rather than mass-renamed without checking their current canonical counterparts.

These findings demonstrate that an implementation can be broadly correct while still requiring exact semantic reconciliation.

## Tooling behavior

The visualizer/validator should distinguish:

- implementation exists;
- implementation reconciliation is not evaluated;
- implementation reconciliation is stale;
- implementation reconciliation has mismatches;
- implementation reconciliation is verified against the current authored version/hash.

A green implementation-reconciliation indicator requires a current verified receipt, not merely `status: implemented`.

## Relationship to authored progress

Projection conformance is evidence used during `progress.presentation` and `progress.implementation` reconciliation.

It does not automatically write those progress fields.

Once the required projections are reconciled and findings resolved, an explicit authored reconciliation decision may mark the appropriate phase complete.

## Relationship to runtime proof

Implementation reconciliation is static/source reconciliation.

UI Runtime is execution proof.

For example:

- React Native source may be verified against the AuthView once;
- iOS runtime execution may pass;
- Android runtime execution may fail.

Those are three valid independent facts and must remain visible independently.

## Migration sequence

For Password Sign-In:

1. migrate required AuthViews/AuthRoutes to the accepted v2 schema;
2. compute authored definition version/hash;
3. reconcile Angular source and routes;
4. reconcile React Native source and routes;
5. reconcile CommonLinks routes;
6. classify and resolve every semantic difference;
7. write current conformance receipts;
8. only then consider presentation/implementation authored reconciliation complete under v2;
9. run Web/iOS/Android scenarios separately for UI Runtime proof.

This process becomes the template for subsequent categories.
