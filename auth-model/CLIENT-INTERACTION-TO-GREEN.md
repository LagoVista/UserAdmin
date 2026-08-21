# Client Interaction -> Green

## Purpose

This process defines how to take one Client Interaction or Auth Interaction from an authored definition to a trustworthy client implementation across Angular Web and React Native.

This process is intentionally narrower than `CATEGORY-TO-GREEN.md`.

For Client Interactions, the backend operation is assumed to already exist and behave correctly. The purpose of this process is to prove that the client-side interaction exists, matches the authored contract, invokes the intended behavior, and returns the correct correlated result to the agent.

For Auth Interactions, the existing canonical authentication model remains authoritative for authentication behavior, guards, transitions, and server-side effects. This process verifies only that the Auth Interaction is correctly bound to that existing truth and correctly implemented by the clients.

## North star

A Client Interaction is green when:

1. its authored definition is complete and valid;
2. Angular Web implements the interaction and conforms to the authored contract;
3. React Native implements the interaction and conforms to the authored contract; and
4. current runtime evidence proves the interaction launches, completes, and returns the expected correlated result.

An Auth Interaction has one additional requirement:

5. its canonical authentication bindings resolve and remain valid.

Backend implementation completeness is not part of the Client Interaction green score.

## Green dimensions

Track these dimensions independently:

- Definition
- Auth Bindings, for Auth Interactions only
- Angular Web
- React Native
- Runtime Evidence

Each supported client is evaluated using exactly three conformance checks:

- Exists
- Controls Conform
- Behavior Wired

A useful visual summary is:

`Definition -> Auth Bindings -> Angular 3/3 -> React Native 3/3 -> Runtime`

For a general Client Interaction, omit Auth Bindings.

## Evidence storage contract

Authored interaction definitions remain in:

- `auth-model/client-interactions/*.json`
- `auth-model/auth-interactions/*.json`

Current client implementation observations are stored separately from authored interaction truth:

- Angular Web: `auth-model/implementation/client-interaction-conformance/angular-web.json`
- React Native: `auth-model/implementation/client-interaction-conformance/react-native.json`

Both manifests validate against:

- `auth-model/schemas/client-interaction-conformance-manifest.schema.json`

Each manifest records:

- the exact inspected repository;
- the exact inspected commit SHA;
- one observation per reconciled interaction;
- Exists;
- Controls Conform;
- Behavior Wired;
- runtime-evidence status;
- component/directive-handler identity when found;
- observed semantic finders;
- source evidence;
- concrete notes explaining gaps or drift.

These manifests are implementation evidence, not authored product truth. Do not edit the interaction definition merely to make a client observation green.

Aptix derives progress indicators from the interaction definition plus these evidence manifests.

## Status values

The three client checks are evidence-backed booleans. Runtime evidence uses:

- `not-run`
- `passed`
- `failed`

A client is green when Exists, Controls Conform, and Behavior Wired are all true.

An interaction is fully green when every required client is green, required runtime evidence passes, and Auth Bindings are valid when applicable.

If implementation exists but does not match authored truth, treat it as drift/in-progress rather than redefining the interaction to match the implementation.

## 1. Author the interaction definition

Confirm that the interaction represents exactly one bounded client capability.

The definition must establish:

- stable interaction key;
- stable ClientDirective key;
- one bounded purpose;
- invocation payload contract;
- controls;
- actions;
- allowed local presentation states;
- terminal outcomes;
- response-value contract;
- supported client platforms.

The interaction must satisfy the constraints in `CLIENT-CARD-CONTRACT.md`.

In particular:

- one ClientDirective maps to one interaction;
- every invocation returns using the same correlation id;
- the interaction does not navigate directly to another interaction;
- arbitrary structured response payloads are prohibited;
- response values are limited to the contract-approved scalar, EntityHeader, or homogeneous multi-select shapes;
- Auth Interactions return no response value of any kind.

### Definition complete

Definition is complete when:

- the JSON validates against the correct schema;
- required fields are present;
- response rules are explicit;
- supported platforms are declared honestly;
- no unresolved authored ambiguity remains about what the client must render or return.

## 2. Validate Auth bindings

This phase applies only to Auth Interactions.

Confirm that every declared canonical authentication reference resolves to current authored authentication truth.

Bindings may include Behaviors, Scenarios, Actions, Transitions, and AuthViews where presentation semantics are intentionally reused.

The Auth Interaction must not invent authentication guards, transitions, postconditions, or authoritative account state.

## 3. Reconcile Angular Web

Inspect the current stable Angular implementation repository and record its exact commit SHA in `implementation/client-interaction-conformance/angular-web.json`.

Evaluate exactly three checks.

### Exists

Confirm that the authored ClientDirective is recognized, launches the intended interaction component/card, and has identifiable source evidence.

### Controls Conform

Compare the rendered interaction to the authored definition. Required controls, actions, semantic finders, control types, required/optional behavior, and sensitivity boundaries must match semantically.

Visual styling does not need to be identical across platforms.

### Behavior Wired

Confirm from source that authored actions invoke the intended client/service behavior, terminal paths map to authored outcomes, the original correlation id returns, cancellation/failure terminate cleanly, and the response obeys the authored response contract.

For this process, do not re-prove the backend implementation behind the invoked operation.

Angular is green when all three checks are true.

## 4. Reconcile React Native

Inspect the current stable React Native implementation repository and record its exact commit SHA in `implementation/client-interaction-conformance/react-native.json`.

Apply the same three checks:

- Exists
- Controls Conform
- Behavior Wired

The semantic interaction contract must match Angular even when native presentation differs.

React Native is green when all three checks are true.

## 5. Runtime evidence

Runtime evidence proves the client interaction protocol actually works when executed.

For each required platform prove:

- the correct ClientDirective launches the correct interaction;
- the interaction is usable;
- the expected client/service behavior is invoked;
- a modeled terminal outcome is produced;
- the response returns the original correlation id;
- the interaction terminates/closes;
- any allowed response value has the authored shape;
- Auth Interactions return no value.

Record runtime status and evidence references on that platform's conformance observation.

Do not duplicate existing backend integration evidence unless a failure suggests the backend assumption is invalid.

## 6. Determine final green state

A general Client Interaction is green when Definition is valid, every required client is 3/3, and required runtime evidence passes.

An Auth Interaction additionally requires valid canonical Auth Bindings.

Unsupported or explicitly not-applicable platforms do not block green. Planned platforms do block full green until implemented or intentionally reclassified.

## Aptix visualization

The Client Interactions visualizer should project the evidence as clear progress indicators.

General interaction example:

`Definition ✓   Angular 1/3   React Native 0/3   Runtime ○`

Auth interaction example:

`Definition ✓   Auth Bindings ✓   Angular 3/3   React Native 3/3   Runtime ✓`

Each client expands to show:

- Exists
- Controls
- Behavior
- inspected commit
- source evidence
- runtime evidence

Aptix is a projection. The JSON definitions and evidence manifests remain authoritative.

## Reference specimen: Accept or Reject Terms and Conditions

Use `client.interaction.terms-and-conditions` as the reference specimen for this process.

### Authored definition

`auth-model/client-interactions/accept-reject-terms-and-conditions.json`

The authored interaction requires:

- directive `client.terms-and-conditions`;
- authoritative T&C display;
- explicit Accept action;
- explicit Reject action;
- terminal outcomes `accepted`, `rejected`, `canceled`, and `failed`;
- no response value.

### Angular baseline

Inspected repository:

`softwarelogistics/workforce-ui`

Reference implementation precursor:

`src/app/ui/workforce/client-directives/promotion-directive/PromotionDirectiveComponent`

The first reconciliation records Angular as 1/3:

- Exists: true
- Controls Conform: false
- Behavior Wired: false

Why: the existing prototype loads/submits real T&C acceptance, but its UI is a consent checkbox plus `Create my workspace`; it does not expose the authored Accept/Reject semantic surface, and completion emits `ContinuitySessionView` rather than the new correlated Client Interaction outcome contract.

This is useful evidence, not a defect in the authored definition. The prototype predates the canonical interaction model.

### React Native baseline

Inspected repository:

`nuviot/vtm-client`

The first reconciliation records React Native as 0/3 because no matching Client Interaction implementation was found.

### How to use this specimen

When implementing a new Client Interaction:

1. author the definition;
2. add or update its Angular observation;
3. add or update its React Native observation;
4. let Aptix expose the three checks;
5. implement until both clients reach 3/3;
6. execute each client and attach runtime evidence;
7. mark runtime passed only from current evidence.

This Terms and Conditions interaction is the template for where definitions, client observations, source evidence, and runtime evidence belong. Future Client and Auth Interactions should follow the same layout.
