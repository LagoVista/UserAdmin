# Authentication Implementation Plan

## Status

Proposed for review.

This plan begins after completion of the canonical implementation-mapping foundation. The model now connects canonical authentication flows to planned handlers, existing REST endpoints, generated client contracts, and registered test bindings. Aptix can reconcile proxy bindings against generated `operations.json` manifests by stable `contractId`.

## Objective

Migrate authentication behavior behind explicit, typed FlowHandlers without changing existing public HTTP contracts or generated client behavior.

The intended execution chain is:

```text
AuthView action
  → AuthFlow
  → AuthenticationFlowService
  → typed FlowHandler
  → canonical transition/result
  → existing REST endpoint projection
  → generated client contract
  → test evidence
```

## Foundation Already Completed

- Canonical AuthFlow schema and initial flow definitions.
- Planned AuthFlowHandler definitions.
- Existing REST endpoint bindings.
- Stable generated operation identities emitted by Aptix.
- Canonical proxy bindings for the first three flows.
- Test-binding registrations.
- Aptix Authentication Implementation explorer.
- Reconciliation of proxy bindings against generated `operations.json` manifests.
- Detection of missing operations and operation, method, request, or response drift.

The initial mapped flows are:

1. Password recovery request.
2. Password recovery completion.
3. Invitation acceptance.

## Guiding Constraints

1. Existing routes, verbs, request contracts, and response contracts remain compatible.
2. Controllers remain thin HTTP adapters.
3. FlowHandlers own application-flow behavior, not transport concerns.
4. Existing managers and domain services remain the source of domain behavior unless a separate refactor is explicitly approved.
5. Canonical transitions constrain handler outcomes.
6. Generated method names are projections and may change; stable `contractId` values provide identity.
7. A flow is not considered verified until registered tests have produced execution evidence.

## Implementation Readiness Model

Each flow should progress through these observable states:

| State | Meaning |
|---|---|
| Modeled | Canonical flow, transitions, and contracts are defined. |
| Externally Implemented | An existing endpoint and generated client contract are mapped. |
| Handler Planned | A FlowHandler definition exists with dependencies and allowed outcomes. |
| Handler Implemented | The typed handler exists and compiles. |
| Endpoint Delegated | The existing endpoint routes through `AuthenticationFlowService`. |
| Tests Registered | Required unit, endpoint, and transition tests are mapped. |
| Tests Executed | Test evidence exists for the current implementation context. |
| Verified | The complete chain is reconciled and tests pass. |

These states should eventually be calculated from definitions and evidence rather than manually asserted.

## Phase 1: Confirm the Existing Chain

Before changing runtime behavior, validate the three mapped flows in the Aptix implementation explorer with both `LagoVista/UserAdmin` and `nuviot/nuvos-app-contracts` available in the workspace.

Acceptance criteria:

- Each flow resolves its planned handler definition.
- Each endpoint binding resolves.
- Each proxy binding resolves by stable `contractId`.
- HTTP method, route, request body, and result contract match the generated manifest.
- Each flow resolves its registered test binding.
- Any issue displayed by Aptix represents an actual discrepancy.

## Phase 2: Establish the FlowHandler Application Layer

Introduce the minimum shared runtime abstractions required to execute typed handlers.

Expected concepts:

- `IAuthenticationFlowService`
- `AuthenticationFlowService`
- `IAuthenticationFlowHandler<TRequest, TResult>` or an equivalent typed contract
- A flow execution context containing only approved runtime information
- A normalized result that identifies the canonical transition and public result
- Dependency-injection registration for the dispatcher and handlers

Design expectations:

- The service is a thin dispatcher and coordinator.
- Handler selection is deterministic.
- A handler cannot emit a transition outside the flow definition.
- Transport-specific concerns remain in controllers.
- Sensitive values are not added to general chat, diagnostics, or model context.

## Phase 3: First Vertical Slice

Implement `auth.flow.recovery.request` first.

Why this flow:

- It is bounded.
- It has a stable existing endpoint and generated proxy.
- It has limited output variation.
- It exercises notification-oriented behavior without changing session state.
- It provides a clean template for subsequent handlers.

Work sequence:

1. Review the existing `SendResetPasswordLinkAsync` behavior and dependencies.
2. Finalize the recovery-request FlowHandler definition.
3. Implement the typed handler using existing managers and services.
4. Register it with dependency injection.
5. Route the existing endpoint through `AuthenticationFlowService`.
6. Preserve the existing endpoint contract and generated proxy behavior.
7. Add or adapt unit and endpoint tests.
8. Capture test evidence.
9. Mark the flow verified only after reconciliation and tests succeed.

Acceptance criteria:

- No route, verb, request, or response contract changes.
- Existing behavior remains compatible.
- The controller no longer coordinates the application flow directly.
- The handler emits only the canonical recovery-request transition.
- Registered tests pass.
- Aptix shows the complete chain with no unresolved implementation issues.

## Phase 4: Expand the Pattern

After the first slice is verified, apply the same pattern to:

1. Password recovery completion.
2. Invitation acceptance.

Each flow should be implemented independently and verified before beginning the next. Shared abstractions should only be generalized after at least two concrete handlers expose the same requirement.

## Phase 5: Execution Evidence

Add evidence records for test execution and implementation verification.

Minimum evidence fields:

- Test-binding key.
- Flow key.
- Repository and commit SHA.
- Execution timestamp.
- Environment.
- Outcome.
- Evidence source.
- Relevant implementation-context hash or version.

Aptix should distinguish:

- test registered but never executed
- evidence stale for the current implementation
- test failed
- test passed and current

## Phase 6: Automation and Governance

Once multiple handlers exist, automate readiness calculation and drift detection.

Candidate automation:

- Reconcile canonical proxy bindings whenever generated contracts change.
- Re-evaluate endpoint and handler mappings when source files change.
- Mark evidence stale when relevant implementation context changes.
- Surface the next incomplete readiness state for each flow.
- Prevent a flow from being labeled verified when references or evidence are missing.

## Deferred Work

The following are intentionally outside the first vertical slice:

- Replacing existing domain managers.
- Redesigning public authentication routes.
- Unifying semantically different login endpoints.
- Broad handler code generation before the first pattern is proven.
- Runtime persistence of all canonical definitions.
- Automatic production deployment decisions.

## Review Decisions

Before implementation begins, review and approve:

1. The readiness-state vocabulary.
2. The typed FlowHandler contract shape.
3. The normalized handler-result shape.
4. How canonical transition enforcement occurs at runtime.
5. The minimum execution-evidence record.
6. Whether the first slice remains password recovery request.

## Immediate Next Step

Review this plan, adjust the application-layer contracts where needed, and then implement the password-recovery-request vertical slice as the reference pattern for all subsequent authentication FlowHandlers.
