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

Each supported client is evaluated using the same three conformance checks:

- Exists
- Controls Conform
- Behavior Wired

A useful visual summary is:

`Definition -> Auth Bindings -> Angular 3/3 -> React Native 3/3 -> Runtime`

For a general Client Interaction, omit Auth Bindings.

## Status values

Recommended status values are:

- `not-started`
- `in-progress`
- `complete`
- `drift`
- `not-applicable`

`drift` means implementation exists but no longer conforms to the current authored contract.

A client is green only when all required conformance checks are complete.

An interaction is fully green only when every required dimension is complete.

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

Bindings may include:

- Behaviors;
- Scenarios;
- Actions;
- Transitions;
- AuthViews where presentation semantics are intentionally reused.

The Auth Interaction must not invent authentication guards, transitions, postconditions, or authoritative account state.

### Auth Bindings complete

Auth Bindings are complete when:

- all declared references resolve;
- the referenced definitions describe the behavior the interaction actually invokes;
- no card-owned authentication semantics conflict with canonical auth truth.

## 3. Reconcile Angular Web

Inspect the current stable Angular implementation repository.

Evaluate exactly three checks.

### Exists

Confirm that:

- the authored ClientDirective is recognized;
- it launches the intended interaction component/card;
- the implementation path and component can be identified from source.

Do not mark Exists complete based on naming similarity alone.

### Controls Conform

Compare the rendered interaction contract to the authored definition.

Confirm that:

- required controls exist;
- required actions exist;
- stable finders match the authored contract where finders are required;
- control types are semantically compatible;
- required/optional behavior matches the definition;
- Auth controls preserve their declared sensitivity boundary;
- no materially different user operation has been silently added to the card.

Visual styling does not need to be identical across platforms.

### Behavior Wired

Confirm from source that:

- each authored action invokes the intended client/service operation;
- the interaction reaches the intended existing backend behavior when server-backed;
- each terminal path maps to an allowed authored outcome;
- completion returns using the original correlation id;
- cancellation and failure terminate the interaction cleanly;
- the response value conforms to the authored response contract;
- Auth Interactions return no response value.

For this process, do not re-prove the backend implementation behind the invoked operation.

### Angular complete

Angular is green when Exists, Controls Conform, and Behavior Wired are all complete.

## 4. Reconcile React Native

Apply the same three checks to the current stable React Native implementation repository:

- Exists
- Controls Conform
- Behavior Wired

The semantic interaction contract must match Angular even when native presentation differs.

Do not require identical component structure, navigation primitives, styling, or platform-specific implementation details.

### React Native complete

React Native is green when Exists, Controls Conform, and Behavior Wired are all complete.

## 5. Detect drift

A previously green client becomes `drift` when the current source no longer conforms to the authored interaction.

Examples include:

- directive key no longer resolves to the expected component;
- a required control or action is missing;
- a finder changed without updating the authored contract;
- an action invokes a different behavior;
- an authored terminal outcome can no longer be produced;
- an unmodeled terminal result was introduced;
- response shape changed;
- an Auth Interaction returns a value;
- correlation is lost or replaced.

Drift should be surfaced separately for Angular and React Native.

Do not change authored truth merely to make implementation drift disappear. First determine whether the implementation changed incorrectly or the authored interaction intentionally evolved.

## 6. Runtime evidence

Runtime evidence proves the client interaction protocol actually works when executed.

At minimum, exercise the interaction on each required platform and prove:

- the correct ClientDirective launches the correct interaction;
- the interaction is usable;
- the expected client/service behavior is invoked;
- a modeled terminal outcome is produced;
- the response returns the original correlation id;
- the interaction terminates/closes;
- any allowed response value has the authored shape;
- Auth Interactions return no value.

Runtime evidence should focus on the client interaction boundary.

Do not duplicate existing backend integration evidence unless a failure suggests the backend assumption is invalid.

### Runtime complete

Runtime Evidence is complete when the required supported clients have current passing evidence for the authored interaction contract.

## 7. Determine final green state

### General Client Interaction

Green requires:

- Definition complete
- Angular complete, when supported
- React Native complete, when supported
- Runtime Evidence complete

### Auth Interaction

Green requires:

- Definition complete
- Auth Bindings complete
- Angular complete, when supported
- React Native complete, when supported
- Runtime Evidence complete

Unsupported or explicitly not-applicable platforms do not block green.

Planned platforms do block full green until they are either implemented or intentionally reclassified.

## Aptix visualization

The Client Interactions visualizer should make this process visible without requiring the developer to inspect JSON manually.

For each interaction, show a compact status strip such as:

`Definition ✓   Angular 3/3   React Native 2/3   Runtime ○`

For Auth Interactions:

`Definition ✓   Auth Bindings ✓   Angular 3/3   React Native 3/3   Runtime ✓`

Each client should expand to show:

- Exists
- Controls
- Behavior

The visualizer should also surface:

- implementation repository;
- inspected commit SHA when conformance is recorded;
- component/path evidence;
- concrete drift findings;
- runtime evidence status.

The visualizer is a projection of authored definitions and implementation evidence. It must not become the authoritative store for either.

## First reference interaction

The first reference interaction is:

`Accept or Reject Terms and Conditions`

It is intentionally simple and should be used to establish the implementation-evidence and visualization pattern before applying this process broadly.

Its purpose is to prove the general Client Interaction process first. Authentication-specific interaction reconciliation can then reuse the same client-conformance machinery with the additional Auth Bindings and no-response-value constraints.
