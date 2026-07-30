# Authentication Evidence Handoff

## Current Goal

Build a deterministic authentication implementation map where canonical transitions are reconciled against executed NUnit evidence, and the Aptix extension shows both transition health and the ordered `AuthLogTypes` milestones observed by each passing test.

## Working Model

- **Transitions** are canonical authentication state changes.
- **Flows** are implemented execution paths. They are not required to be 1:1 with transitions.
- Every non-UI-only transition should eventually be implemented by at least one flow and covered by executed evidence.
- **Passing / Failing / Uncovered** describe transition evidence state, not flow count.
- A flow card summarizes the evidence state of its linked transitions.
- **Evidence Runs** is the number of loaded project evidence files, not the number of tests.
- **Generated Matched** concerns generated client `operations.json` manifests and is independent of test evidence.
- Reconciliation issues are computed live by the extension and are not persisted.

## Completed End-to-End Slice

Password authentication is the first complete vertical slice:

1. NUnit tests carry `AptixEvidence` references for the test binding, flow, and canonical transition.
2. Integration tests exercise `AuthenticationFlowService -> PasswordLoginFlowHandler -> real SignInManager`.
3. `RecordingAuthenticationLogManager` records actual `AuthLogTypes` emissions.
4. Tests assert the expected event sequence.
5. Tests also carry `AptixAuthEvents` metadata matching the asserted sequence.
6. DevTools writes `.aptix/evidence/[ProjectName].json` using schema `1.1`.
7. Aptix watches the evidence folder and reconciles transition status live.

Verified password event trails:

- Invalid credentials: `PasswordAuthenticationStarted -> PasswordAuthenticationFailed`
- Success: `PasswordAuthenticationStarted -> PasswordAuthenticationSucceeded`
- User not found: `PasswordAuthenticationStarted -> PasswordAuthUserNotFound`

The latest shared evidence run had 15 passed, 0 failed, 0 skipped, and no evidence issues.

## Important Commits

### LagoVista/UserAdmin

- `e955a1712ddd19d803857473c6977cb68cfcdde9` recording authentication log manager
- `df0434f18dbe06978399d1a9c6c16c28851160e8` password real-path integration tests
- `467f169256a56465551f09ead0c74f29e6b656d2` identity implementation project reference
- `a8a1f68a85b7939b0038389867ef7eaa551631ef` fixed user-not-found logging argument and normalized ID assertion
- `323f26ff0c9ef0996a5e6482ad8b49ab8f539477` aligned canonical password transition key
- `e672376bf8702538c19c84788908e450639dd680` added password recovery auth event vocabulary
- `99f9615f0cdd8045b831bb4256556c5cf865e5fe` instrumented password recovery milestones
- `bcdc4fa9dcaf22f0c137891f1fb4f15f9c62fddd` added `AptixAuthEvents` metadata to password integration tests

### nuviot/devtools

- `4d238c75879d5f3fa78b674975ad9a0dd0f4dff8` reads `AptixAuthEvents` and emits `ObservedAuthEvents`

### nuviot/aptix-client

- `67e3ab9ee32eef033eafe1d9f75e3e8c312615b4` initial evidence reconciliation
- `76befa531b520a1ffc528d9cadd6d3da2605e229` schema 1.1, PascalCase, and `.aptix/evidence` support

## Evidence Contract

Evidence files live at:

```text
.aptix/evidence/[ProjectName].json
```

Auth tests use:

```text
Profile: auth
```

The additive per-test field is:

```json
"ObservedAuthEvents": [
  "PasswordAuthenticationStarted",
  "PasswordAuthenticationSucceeded"
]
```

The extension should omit empty event arrays from the UI.

## Verified Current State

The latest evidence document proves:

- 15 total tests passed
- 0 failed
- 0 skipped
- 0 evidence issues
- three password integration tests include distinct `ObservedAuthEvents` sequences
- the lightweight delegation test has an intentionally empty event sequence

The Aptix file watcher already detects updated evidence files and currently shows one passing transition.

## Pending Aptix UI Change

A patch bundle was prepared for `src/extension/AuthModel/AuthImplementationPanel.ts`, but it has not yet been committed to source. It should be the first action in the next session.

The pending UI change adds:

- `ObservedAuthEvents` parsing from PascalCase or camelCase evidence
- per-test event trails beneath each transition
- omission of empty event sequences
- a denser multi-column flow-card layout
- less vertical whitespace
- a wider, scrollable detail panel
- status-colored cards with prominent `PASSING`, `FAILING`, `PARTIAL`, or `UNCOVERED` labels

The generated bundle from the prior session was named:

```text
aptix-auth-evidence-flow-layout-bundle.json
```

Recreate or apply that change against the latest `main`, compile the extension, and verify the panel visually before continuing.

## Known Non-Urgent Reconciliation Issues

These proxy bindings reference generated contracts, but no matching `operations.json` manifest is loaded:

- `auth.proxy.invitation.accept`
- `auth.proxy.recovery.complete`
- `auth.proxy.recovery.request`

These are operation-contract issues, not evidence failures. Leave them until the generated-operations lane is intentionally started.

## Next Session

1. Apply and commit the pending Aptix UI change.
2. Compile `nuviot/aptix-client` and verify:
   - compact multi-column cards
   - colored password flow card
   - password transition event trails grouped by test
   - empty handler-only event sequence omitted
3. Continue the password recovery request slice:
   - build a real-path recovery request test harness
   - assert `PasswordRecoveryRequested -> PasswordRecoveryCodeGenerated -> PasswordRecoveryMessageSent`
   - add `AptixEvidence` references for `auth.flow.recovery.request` and its canonical transition
   - add `AptixAuthEvents` metadata matching the asserted sequence
   - run DevTools and confirm Aptix turns the second transition green
4. Follow with password recovery completion and assert `PasswordRecoveryCompleted` only after a successful reset.

## Emerging Rule, Not Yet an Invariant

Every auth flow should emit meaningful `AuthLogTypes` events. A common lifecycle may become Started / Succeeded / Failed, but do not formalize that globally yet. Let the invariant emerge from several implemented flows and tests.
