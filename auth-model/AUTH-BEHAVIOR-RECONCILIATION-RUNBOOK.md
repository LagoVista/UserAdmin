# Authentication Behavior Reconciliation Runbook

## Purpose

This document is the starting point for the next authentication-model reconciliation session.

The goal is not merely to document auth behavior. The goal is to make each behavior traceable from canonical model intent through real implementation and executed test evidence, then let Aptix show that truth in one place.

When starting a new session, read this file first, inspect the current repository state, choose one behavior, and repeat the process below. Password Management is the canonical worked example.

A useful next-session prompt is:

> Read `auth-model/AUTH-BEHAVIOR-RECONCILIATION-RUNBOOK.md`, inspect the latest `master`, and help me reconcile the next auth behavior using Password Management as the reference implementation.

## Repositories Involved

The reconciliation may cross several repositories:

- `LagoVista/UserAdmin` - canonical auth model, application flow service, handlers, managers, integration tests, and generated Aptix runtime evidence.
- `LagoVista/UserAdminRest` - REST endpoints. Public HTTP routes should enter the application through `IAuthenticationFlowService` for canonical user auth flows.
- `nuviot/nuvos-app-contracts` - generated client contract and proxy operation metadata. Treat generated operation identity as immutable unless the external contract itself is intentionally being changed.
- `nuviot/aptix-client` - visualizer that loads the auth model and `.aptix/evidence/*.json` and reconciles model state against runtime proof.
- `nuviot/devtools` - tooling that runs the test project and emits Aptix evidence. Keep local build tools current before trusting generated evidence.

## The Canonical Layering

The stable execution shape for a server-backed authentication behavior is:

```text
Scenario
  -> Generated Proxy
  -> REST Endpoint
  -> AuthenticationFlowService
  -> Typed Flow Handler
  -> Manager / Domain Logic
  -> Canonical Transition
  -> Test Binding
  -> Executed Runtime Evidence
```

For Password Management the concrete path is:

```text
AuthClient.createChangepassword
  -> POST /api/auth/changepassword
  -> AuthenticationFlowService.ChangePasswordAsync
  -> PasswordChangeFlowHandler.HandleAsync
  -> PasswordManager.ChangePasswordAsync
  -> LagoVista.AspNetCore.Identity.Managers.UserManager.ChangePasswordAsync
  -> auth.transition.password-management.change-success
     OR auth.transition.password-management.change-failed
```

### Layer responsibilities

- **Generated proxy / REST route**: external contract. Preserve route, request, response, generated operation ID, and client method unless the external contract is intentionally changing.
- **AuthenticationFlowService**: stable application API and canonical flow entry point. REST should call this layer rather than reaching directly into a handler.
- **Flow handler**: typed executable unit for the auth operation. It chooses the canonical transition deterministically from the manager/domain result.
- **Manager/domain logic**: owns the real security and business rules.
- **Canonical transition**: describes the state change and its required/forbidden effects. It is not merely a code branch name.
- **Test binding**: declares which test methods and proof obligations establish the implementation.
- **Runtime evidence**: proves that the mapped tests actually ran and what auth events were observed.

## Invariants Learned During Password Management

### 1. Generated contract identity is stable

Do not casually change generated client identity while reconciling the server implementation. For Password Management the shared generated contract remains:

```text
operationId: AuthServices_ChangePassword_api_auth_changepassword
route: POST /api/auth/changepassword
client: AuthClient.createChangepassword
request: Models.ChangePassword
response: InvokeResult
```

The server-side landing path may be improved without changing that contract.

### 2. REST enters through AuthenticationFlowService

For canonical user auth flows, prefer:

```text
REST -> IAuthenticationFlowService -> handler -> manager
```

Do not normalize by making REST call the handler directly. The flow service is the stable application boundary and validates the transition returned by the handler.

Privileged/admin operations may legitimately have a different path. For example, `SetUserPassword` remains a direct manager operation and should not be forced into the user change-password flow merely for symmetry.

### 3. A flow handler may mock dependencies in focused tests, but runtime integration proof must keep the claimed entry path real

Focused handler/unit tests are useful for dispatch and transition selection.

The stronger integration invariant is:

```text
REAL:
AuthenticationFlowService
  -> concrete FlowHandler
  -> concrete Manager
  -> concrete LagoVista Identity adapter

MOCK/HARNESS BELOW:
Microsoft.AspNetCore.Identity.UserManager<AppUser>
infrastructure, persistence, notification/log transport dependencies
```

If a test claims to prove a canonical flow entry point, that entry point must not be mocked away.

### 4. Auth event ownership must be singular

Do not log the same canonical auth event at multiple layers.

Password Change initially emitted success twice because both `PasswordManager` and the LagoVista Identity adapter logged `ChangePasswordSuccess`. The fix was to let the credential-changing adapter own both `ChangePasswordSuccess` and `ChangePasswordFailed`, while upper layers retain diagnostic/admin logging only.

Rule: the layer that truly owns the security operation should normally own its canonical auth event, exactly once.

### 5. Client validation and server behavior are different concerns

`ConfirmPassword` exists in the Password Change presentation, but confirm mismatch is client presentation validation. It is not a distinct server authentication behavior or canonical transition.

Do not multiply server behaviors for every UI validation permutation. Model the meaningful server outcomes.

### 6. Completion views return control to the host

Auth completion views should describe leaving the auth flow, not invent global application navigation. For Password Change, `action:done` means return control to the caller/host. The destination is intentionally outside the auth model.

### 7. Authored test status is not runtime proof

This distinction is critical:

- **Test definition evidence**: mapped test/binding exists and claims a proof obligation.
- **Test execution evidence**: the builder actually ran the test and emitted a result.

A scenario should not be treated as truly test-complete merely because a test file exists.

The green Aptix scenario Tests pill should be backed by executed evidence, for example:

```text
✓ Tests · 1/1 passed
```

If tests exist but fresh evidence does not, the UI should say so rather than manufacturing green confidence.

## Runtime Evidence Contract

Generated evidence lives at:

```text
.aptix/evidence/[ProjectName].json
```

For the main auth suite:

```text
.aptix/evidence/LagoVista.UserAdmin.Auth.Tests.json
```

Evidence is generated output. **Never hand-edit it.** If it is wrong or stale, fix/update the build tooling or test metadata and regenerate it.

The current schema is `1.1`. Relevant document fields include:

```text
ProjectName
ProjectPath
AssemblyPath
RunId
StartedUtc
CompletedUtc
ExitCode
Outcome
Summary
Tests[]
Issues[]
StandardOutput[]
StandardError[]
```

Each mapped NUnit test uses `AptixEvidence` metadata encoded as:

```text
auth|reference|reference|reference
```

The builder emits that as:

```json
{
  "Profile": "auth",
  "References": [
    "auth.test-binding...",
    "auth.flow...",
    "auth.transition..."
  ]
}
```

Tests may also carry:

```csharp
[Property("AptixAuthEvents", "ChangePasswordSuccess")]
```

which the builder emits as:

```json
"ObservedAuthEvents": ["ChangePasswordSuccess"]
```

Aptix accepts schema 1.0/1.1, PascalCase/camelCase, and uses the per-test outcome with project outcome as fallback.

### Important tooling lesson

A local green test run is not enough if the local `devtools` version is too old to emit the current evidence format. Before diagnosing Aptix, verify that the local builder actually regenerated `.aptix/evidence/...` and that the file contains the new test methods/references.

We lost time on this once: the source tests were green and the Aptix UI was correct, but the evidence file was still an older July 30 run because the local build tools did not yet contain the evidence-generation changes.

## Aptix Reconciliation Behavior

`nuviot/aptix-client/src/extension/AuthModel/AuthImplementationPanel.ts` loads:

- category catalog
- behaviors
- scenarios
- auth views
- implementation test bindings
- `.aptix/evidence/*.json`

For each server-backed scenario it builds `ScenarioProof` containing:

```text
scenarioKey
serverRequired
status: passing | failing | skipped | uncovered
testCount
passingTestCount
transitionKeys
bindingKeys
observedAuthEvents
latestCompletedUtc
```

Evidence matches a scenario when an `auth` evidence reference contains the scenario key or one of the scenario's canonical transition keys.

The UI intentionally rolls evidence up at higher levels:

- category card: simple phase state such as `✓ Tests`
- behavior card: rolled-up server proof summary
- scenario card: concrete evidence such as `✓ Tests · 1/1 passed`
- expanded scenario: transition keys, test binding keys, timestamp, and observed auth events

The scenario is where detailed proof matters. Category-level numbers would add noise.

## Password Management: Canonical Worked Example

### Behaviors

```text
auth.behavior.password.change-success
auth.behavior.password.change-failed
```

Wrong current password, password-policy rejection, and similar server rejections are permutations of the failed behavior unless they represent materially different auth state semantics.

### Scenarios

```text
auth.scenario.password-management.change-password-success
auth.scenario.password-management.change-password-failed
```

Success:

```text
start: auth.password.change
action: change-password
end: auth.password.change.complete
transition: auth.transition.password-management.change-success
expected auth event: ChangePasswordSuccess
```

Failure:

```text
start: auth.password.change
action: change-password
end: auth.password.change
transition: auth.transition.password-management.change-failed
expected auth event: ChangePasswordFailed
```

### Presentation

`auth.password.change` contains current/new/confirm password controls, change action, and error presentation.

`auth.password.change.complete` presents successful completion and a Done action that returns control to the host.

### Implementation bindings

Reference files:

```text
auth-model/actions/password-management-change.json
auth-model/transitions/password-management-change-success.json
auth-model/transitions/password-management-change-failed.json
auth-model/implementation/proxies/password-management-change.json
auth-model/implementation/endpoints/password-management-change.json
auth-model/implementation/flows/password-management-change.json
auth-model/implementation/handlers/password-management-change.json
auth-model/implementation/tests/password-management-change.json
```

Key implementation identities:

```text
auth.proxy.password-management.change
auth.endpoint.password-management.change
auth.flow.password-management.change
auth.handler.password-management.change
auth.test-binding.password-management.change
```

### Runtime integration tests

Reference test file:

```text
tests/LagoVista.UserAdmin.Auth.Tests/PasswordChangeFlowIntegrationTests.cs
```

Canonical methods:

```text
SuccessfulChange_Should_RunRealFlowAndManagers_AndEmitSuccessTransitionEvidence
RejectedChange_Should_RunRealFlowAndManagers_AndEmitFailedTransitionEvidence
```

Success evidence references:

```text
auth.test-binding.password-management.change
auth.flow.password-management.change
auth.transition.password-management.change-success
```

Failure evidence references:

```text
auth.test-binding.password-management.change
auth.flow.password-management.change
auth.transition.password-management.change-failed
```

Expected observed events:

```text
success -> ChangePasswordSuccess
failure -> ChangePasswordFailed
```

## Repeatable Reconciliation Process

Use this sequence for one behavior at a time. Do not try to normalize the entire auth system in one sweep.

### Step 1: Select one behavior and inspect current truth

Start from the Aptix category/behavior card and the current JSON model.

Read:

- category entry in `behavior-category-catalog.json`
- behavior JSON
- linked scenario JSON files
- start/end AuthViews
- source references
- any existing implementation bindings
- current runtime evidence

Do not assume the authored model or existing implementation is correct. Treat both as hypotheses to reconcile.

### Step 2: Refine the behavior semantics first

Ask:

- What meaningful user/auth outcome is this behavior describing?
- Is it actually one behavior, or are multiple materially different outcomes collapsed together?
- Are apparent variations merely input/error permutations of one canonical outcome?
- What must be true before and after?
- What state must remain unchanged?

Keep behaviors semantic. Avoid mirroring every implementation branch or UI validation branch.

### Step 3: Refine scenarios and presentation

For each scenario, establish:

- start AuthView
- action and finder
- inputs
- server-required flag
- canonical transition key(s)
- expected end AuthView
- expected visible state
- preconditions/postconditions

Then inspect the referenced AuthViews for web/mobile semantic parity.

Presentation progress can be marked complete when the canonical views/actions are coherent, even before implementation is complete.

### Step 4: Trace the generated/public contract

Before changing server code, find the existing generated operation and REST route.

Record:

- generated contract ID / operation ID
- client type and method
- HTTP method and route
- request type
- response type

Preserve this contract unless there is a deliberate product/API contract change.

### Step 5: Trace the real server implementation

Follow the actual code from REST downward.

Look for architectural drift such as:

- REST calling a manager directly when the flow-service pattern should apply
- multiple competing paths for the same operation
- handler bypasses
- duplicate domain/security logic
- duplicate auth event emission
- dead/obsolete code

Normalize only what is necessary to make the canonical path clear and stable.

### Step 6: Establish the typed application flow

The desired shape is generally:

```text
IAuthenticationFlowService.OperationAsync(...)
AuthenticationFlowService.OperationAsync(...)
TypedOperationFlowRequest
TypedOperationFlowHandler
Manager/domain operation
Canonical transition key(s)
```

Register the handler with DI.

The handler should normally be small. Real password/security/business rules belong in the manager/domain layer.

### Step 7: Create or reconcile implementation binding JSON

Bind the canonical model to code using the implementation folders:

```text
implementation/proxies
implementation/endpoints
implementation/flows
implementation/handlers
implementation/tests
```

Also create/reconcile canonical action and transition files when required.

`model-manifest.json` has `requireResolvedReferences: true`, so unresolved references are not acceptable as a completed state.

### Step 8: Add focused tests if useful

A focused flow/handler test may mock the manager and prove deterministic transition mapping.

Do not mistake this for full runtime integration proof.

### Step 9: Add real-path integration evidence

For server-backed scenarios, create integration tests that keep the canonical application path real down through the business/security adapter.

Attach `AptixEvidence` metadata for the binding, flow, and transition. Attach `AptixAuthEvents` when meaningful auth events are expected.

The test should also assert the real observed event sequence, not merely annotate expected metadata.

### Step 10: Run the full AdminAuth suite

Run the relevant test project with the current DevTools/evidence builder.

The user commonly acts as the local compile/test runner. Do not claim passing execution evidence from source inspection alone.

If a test fails because mocks disagree with production behavior, inspect production before changing code. During Password Login reconciliation the tests incorrectly expected `FindByEmailAsync`; production intentionally used `FindByNameAsync` because app-specific users may use `email@EndUserAppOrgId`. The tests were wrong, not the production lookup.

### Step 11: Regenerate and inspect Aptix evidence

Verify the generated evidence file itself.

Check:

- fresh `RunId`
- fresh timestamps
- expected total/passed/failed/skipped counts
- expected test type/method names
- exact `auth` references
- exact `ObservedAuthEvents`

If the file is stale, stop there. Do not mark runtime proof complete and do not hand-edit the evidence.

### Step 12: Verify Aptix visually

Refresh/reload the Authentication Model panel.

For the scenario being reconciled, verify:

- correct start/action/end flow
- correct UI/AuthView cards
- correct server-required status
- correct transition keys
- correct binding keys
- correct observed auth events
- scenario test pill shows current runtime result, such as `1/1 passed`

The visualizer is a reconciliation surface, not the source of truth. If it disagrees with current source/evidence, trace the loader/matcher rather than editing data to make the UI green.

### Step 13: Only then close authored progress

Once semantics, presentation, implementation, tests, and runtime evidence agree, update progress fields on:

- scenarios
- behaviors
- category
- current-work summary where appropriate

Avoid marking `tests: complete` merely because tests were written. Prefer waiting until fresh runtime evidence exists and is visible.

## Progress Semantics

The usual order is:

```text
Category inventory
  -> Behaviors
  -> Scenarios
  -> Presentation/UI
  -> Implementation/Code
  -> Tests + runtime evidence
```

A category-level green Tests phase means all relevant scenarios have sufficient proof. The detailed pass count belongs on the scenario card.

`implementation/tests/*.json` binding maturity can remain `implemented` even when current execution evidence is passing. Binding maturity and evidence execution state are separate concepts. Do not automatically rename maturity to `verified` without an explicit model convention.

## Git / Collaboration Discipline

The user may be changing and pushing the same repositories while an agent is working.

Before every write:

1. fetch the latest branch head
2. fetch the current version of every file being touched
3. apply the smallest change against current content
4. commit directly to the agreed branch (currently `master` for UserAdmin)
5. verify the resulting commit SHA / branch head

Never rely on a stale blob SHA from earlier in the session.

Prefer small architectural commits that encode one discovered truth at a time. That makes compile/test checkpoints and rollback much safer.

## Common Failure Modes

### Green authored state but no runtime count

Usually means the evidence file is stale or lacks matching scenario/transition references.

Check `.aptix/evidence/...` before debugging the Aptix UI.

### Evidence exists but scenario remains uncovered

Check that the test's `AptixEvidence` `Profile` is `auth` and that `References` contains either the scenario key or one of the scenario's exact canonical transition keys.

### Test suite is green but evidence is stale

Update/rebuild the local DevTools evidence generator. A normal `dotnet test` run may not update `.aptix/evidence`.

### Duplicate auth events

Trace which layer truly owns the security operation. Remove duplicated canonical auth-event emission from upper orchestration layers.

### Integration test passes while the real path is mocked away

The test is too shallow for canonical runtime proof. Keep the flow entry point, concrete handler, manager, and owning adapter real.

### Generated contract does not match implementation

Do not rewrite generated proxy metadata to fit server drift. First determine whether the server should be normalized behind the existing public contract.

### A behavior explodes into many failure variants

Ask whether the variants produce materially different auth state transitions. If not, keep one failed behavior and test representative permutations beneath it.

## Definition of Done for One Server-Backed Scenario

A scenario is genuinely reconciled when all of the following are true:

- semantic behavior is agreed
- scenario start/action/end presentation is canonical
- generated/public contract is identified and preserved
- REST enters the stable application flow boundary
- typed handler chooses a canonical transition
- manager/domain layer owns real rules
- canonical auth event ownership is unambiguous and non-duplicated
- implementation bindings resolve to real code
- integration test exercises the claimed real path
- test asserts expected public result and meaningful side effects/events
- `AptixEvidence` references the correct binding/flow/transition
- generated evidence is fresh and passing
- Aptix shows the scenario's passing count and supporting evidence
- authored progress is then marked complete

## Where To Continue

Password Management is complete enough to serve as the reference slice. The next behavior/category should be selected from the current Aptix UI rather than assumed from this document, because repository state will continue to evolve.

When starting the next behavior, resist the urge to copy Password Management mechanically. Reuse its architecture and evidence discipline, but let the semantics of the new behavior determine the correct transitions, event ownership, and test depth.

That is the central lesson from this reconciliation work: **model intent, implementation, and runtime evidence must converge on the same truth.**
