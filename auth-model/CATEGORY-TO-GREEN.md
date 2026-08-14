# Authentication Category → Green Runbook

## Purpose

This is the current operating process for taking one authentication category from inventory to a trustworthy green state.

Use this document for active reconciliation work. Older handoffs and implementation plans under `auth-model/` remain valuable historical context, but they are not the current execution authority when they conflict with this runbook, `README.md`, `CONVENTIONS.md`, the current V2 definitions, or the running test infrastructure.

Password Management is the reference completed category. Password Sign-In and Password Recovery are the current near-complete examples.

## Core authority model

### Git owns authored truth

The canonical model lives in `auth-model/`.

Current V2 category work is driven by:

- `behavior-category-catalog.json`
- `behaviors-v2/`
- `scenarios-v2/`
- `auth-views/`
- `auth-routes/`
- `actions/`
- `transitions/`
- `implementation/`
- `schemas/`

Canonical AuthViews, AuthRoutes, behaviors, scenarios, actions, transitions, and implementation bindings are read from Git. They are not authored or reconciled through Cosmos DB.

Legacy `behaviors/` and `scenarios/` remain reference material only. New category reconciliation uses the V2 folders declared by `model-manifest.json`.

### Mutable execution status does not belong in Git

Running a test must never require a Git commit.

Git progress describes authored/reconciled completeness. Runtime status describes what actually happened when a current definition was executed.

Runtime execution records may currently be persisted in Cosmos DB, but that storage is an implementation detail and may change. The important contract is that authored definitions remain immutable during execution and runtime status is derived separately.

### Two kinds of green

A scenario, behavior, or category has two independent dimensions of health:

1. **Authored green**: the canonical model and implementation mapping are complete and reviewed.
2. **Runtime green**: the required current execution evidence passes.

A category is fully green only when both are true.

Do not write latest pass/fail state into authored JSON.

## Progress semantics

The category catalog currently tracks:

```text
behaviors
scenarios
presentation
implementation
tests
```

Interpret those fields as authored reconciliation progress.

### behaviors = complete

All materially different outcomes for the category have been identified and represented in `behaviors-v2/`.

Behavior boundaries are semantic, not branch-count based. Input validation variants, audit variants, or implementation branches should remain permutations of one behavior unless they create materially different authentication state, security semantics, or next actions.

### scenarios = complete

Every behavior is decomposed into deterministic single-action `scenarios-v2` steps.

Each scenario has a stable canonical key and runtime ID and establishes, as applicable:

- category
- start AuthView
- one action and finder
- inputs and symbolic test values
- preconditions and deterministic setup state
- whether server interaction is required
- canonical transition key(s)
- expected AuthView / landing destination
- postconditions
- expected auth-log events
- required evidence platforms

A scenario is independently runnable from its declared setup state.

### presentation = complete

Every referenced canonical AuthView and AuthRoute is coherent and reconciled with supported clients.

Check:

- canonical `viewId` and `routeId`
- route path
- controls and semantic finders
- actions and semantic finders
- conditional/error surfaces
- web/mobile semantic parity
- declared platform implementation status

Equivalent web and mobile experiences use the same semantic view/control/action identifiers.

Client-only validation should not create fake server behaviors.

### implementation = complete

The real server/client path implements the canonical behavior without contract drift.

For server-backed auth operations, the expected application shape is generally:

```text
generated client/proxy
  -> existing REST route
  -> AuthenticationFlowService
  -> typed flow handler
  -> manager/domain/security operation
  -> canonical transition
```

Preserve the public HTTP/client contract unless a product/API change is explicitly intended.

Implementation reconciliation includes:

- proxy binding
- endpoint binding
- flow binding
- handler binding
- test binding
- canonical action/transition mappings
- DI registration where applicable
- singular ownership of canonical auth events
- removal or isolation of obsolete competing paths when appropriate

The canonical Git-backed AuthView/AuthRoute model is used by the UI test runner. Do not require persisted AuthView records in Cosmos to build a test plan.

### tests = complete

The test specification and proof obligations are complete.

This means the category has enough declared tests/scenarios to prove the intended behavior. It does **not** mean the most recent execution passed.

Runtime execution status is tracked separately.

#### Test architecture and mocking boundary

For a server-backed authentication scenario, the authored proof test must exercise the real application path far enough to prove the behavior being claimed. The preferred boundary is:

```text
AuthenticationFlowService
  -> real typed flow handler
  -> real manager/domain/security operation
  -> mocked infrastructure boundaries only
```

Mocks are appropriate for infrastructure seams that are not themselves the authentication behavior under proof, such as:

- persistence repositories / storage adapters
- email or SMS delivery
- external identity providers and remote services
- framework adapters where the framework itself is not the behavior under proof
- clocks or other environmental dependencies when deterministic control is required

Do **not** mock the manager/domain/security operation whose behavior the scenario claims to prove. A handler test that returns a prearranged result from a mocked underlying manager proves handler branching only; it does not prove the underlying authentication behavior and is insufficient by itself for authored test completeness.

One canonical server test binding may cover several behavior-owned scenarios when those scenarios exercise the same underlying transition family. Every scenario must still be explicitly named by `scenarioKeys`, and the binding must prove every canonical transition required by those scenarios.

## Category → Green workflow

Work one category at a time. Within the category, work behavior-by-behavior and scenario-by-scenario. Avoid broad auth-system rewrites while reconciling one lane.

### 1. Select the category

Open its entry in `behavior-category-catalog.json`.

Confirm:

- category name and boundary
- expected `behaviorKeys`
- current authored progress
- relevant source references / prior decisions

Treat prior progress values as claims to verify, not automatic truth.

### 2. Reconcile behavior inventory

For each meaningful outcome, decide whether it is:

- a distinct behavior
- a scenario/permutation within an existing behavior
- UI validation only
- internal audit detail only

Create/update `behaviors-v2` accordingly.

Before marking behavior inventory complete, every category behavior key must resolve and the set must cover the meaningful user/security outcomes without unnecessary branch multiplication.

### 3. Reconcile the scenario chain

For each behavior, walk the user flow from entry to final outcome.

Create the smallest deterministic sequence of scenarios where each scenario represents one UI action from one known starting surface/state to one deterministic result.

Shared navigation scenarios may be reused by several behaviors. Outcome-specific server-submit scenarios should split only where the resulting authentication semantics differ.

For each scenario verify the full runner contract listed under `scenarios = complete` above.

### 4. Reconcile canonical AuthViews and AuthRoutes

For every `startViewKey` and auth `expectedViewKey`:

- confirm the AuthView exists
- confirm its route exists when routable
- confirm controls/actions/finders match the real client semantics
- confirm web/mobile implementation status
- confirm the actual client exposes the canonical identifiers

`app.*` destinations may intentionally represent host/application landing state rather than an AuthView.

### 5. Reconcile server behavior and public contracts

For each server-backed scenario:

- identify the existing generated client operation
- identify the HTTP method/route/request/response
- trace the real server implementation
- verify the canonical transition selected by each meaningful outcome
- verify auth-log event ownership and expected event trail

Prefer fixing architectural drift behind the stable public contract rather than changing the contract to match the model.

### 6. Reconcile implementation bindings

Ensure the canonical implementation mapping resolves through:

```text
implementation/proxies
implementation/endpoints
implementation/flows
implementation/handlers
implementation/tests
```

`model-manifest.json` requires resolved references. A broken reference cannot be considered complete.

### 7. Make every scenario independently runnable

Before executing UI tests, prove that the server can build the runner plan and deterministic setup for the scenario.

The setup layer must be able to establish required test state without manual preparation, including when applicable:

- durable test user
- organization membership
- password state
- email verification
- lockout/access-failure state
- TOTP/passkey state
- password recovery code / authority state
- other declared `AuthTenantStateSnapshot` properties

Secrets remain symbolic in Git (`user.password`, etc.) and are resolved at runtime.

A failure to build a runner plan or apply setup is an infrastructure/model integration failure, not a UI test failure.

### 8. Execute required platform tests

Run the scenario on every platform required by `evidenceRequirements` that is currently part of the supported test harness.

For a UI scenario, passing means more than clicking through the browser/mobile screen.

The runner must establish:

- expected UI destination / visible outcome
- expected server-side identity post-state
- expected auth-log event evidence where declared

For server-backed scenarios, the server proof should exercise the real flow handler and the real manager/domain/security behavior. Mock only infrastructure seams outside the behavior being proved. If the manager/domain operation itself is mocked, treat that result as component-level coverage rather than sufficient authentication proof.

A browser landing on the right page while the server receipt is wrong is a failed scenario.

### 9. Record runtime execution separately

Persist or aggregate the runtime result outside authored Git definitions.

The dashboard should be able to derive at least:

- never run
- passed
- failed
- aborted/running when relevant
- last execution time
- platform
- run ID
- final view/outcome

The long-term contract also requires definition freshness. Runtime evidence should identify the definition version/hash or Git revision it evaluated so an older pass becomes stale after a material definition change.

Until freshness comparison is fully implemented, do not mistake an old pass for proof of a changed definition.

### 10. Close authored progress

Only after the model is coherent should authored progress be promoted.

Roll progress upward:

```text
scenario
  -> behavior
  -> category
```

A parent phase can be complete only when all relevant children satisfy that authored phase.

Do not modify Git simply because a runtime result changed.

### 11. Declare runtime green

Runtime green is calculated from current required scenario executions, not authored progress fields.

At scenario level:

- required executions exist
- required platforms pass
- server receipt passes
- evidence is current for the definition when freshness tracking is available

At behavior level:

- every scenario required by the behavior is runtime green

At category level:

- every behavior is runtime green

### 12. Category is fully green

A category is fully green when:

```text
Authored:
  behaviors       complete
  scenarios       complete
  presentation    complete
  implementation  complete
  tests           complete

AND

Runtime:
  all required current scenario executions pass
```

That is the finish line.

## Recommended working loop

For active development, use this compact loop:

```text
Pick category
  -> reconcile behaviors
  -> reconcile scenario chain
  -> reconcile views/routes
  -> reconcile implementation
  -> make scenario runnable
  -> run one scenario
  -> fix model/implementation/setup/runner
  -> repeat until behavior is green
  -> repeat until category is green
```

Do not wait until the entire category is modeled before exercising the runner. Once one scenario is complete enough to run, execute it. Runtime friction is useful reconciliation evidence.

## Reference categories

### Password Management

Use as the reference for a completed authored reconciliation and real server-flow proof.

### Password Sign-In

Current model demonstrates:

- Success / Rejected / Locked Out as materially distinct behaviors
- shared navigation scenarios
- deterministic password-submit scenarios
- symbolic runtime credentials
- explicit server pre/postconditions
- expected auth-log events where semantically stable
- canonical web/mobile AuthViews

It is the best current reference for the new direct UI runner path.

### Password Recovery

Use as the reference for:

- multi-step stateful auth ceremonies
- six-digit one-time-code state
- recovery authority separated from the code
- completion rejection after successful proof verification

## Documentation hierarchy

When documents disagree, use this priority:

1. Current schemas and canonical V2 JSON definitions
2. `CONVENTIONS.md`
3. This runbook
4. `AUTH-VIEW-ROUTE-CONTRACT.md`
5. `AUTH-BEHAVIOR-RECONCILIATION-RUNBOOK.md` for detailed implementation/evidence lessons
6. Dated evidence handoffs, implementation plans, and `SECTION-*.md` handoffs as historical context

Historical notes should not silently override current canonical definitions or current runtime architecture.
