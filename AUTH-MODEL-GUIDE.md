# Authentication Model Guide

## 1. Purpose and Authority

This guide is the durable collaboration contract for the LagoVista authentication model. It records the working architecture, terminology, plan, section status, and handoff expectations so future sessions can resume from Git rather than conversational memory.

Git is authoritative for authentication model definitions and planning. Cosmos DB is a runtime projection used for imported definitions, execution status, evidence, and historical results.

The current authentication DDR is the starting policy hypothesis, not immutable truth. Implementation, scenarios, tests, and real-world behavior may reveal extensions or conflicts. Those findings must be reconciled deliberately and later folded back into the DDR.

### 1.1 Authority order during this effort

1. Approved reconciliation decisions in Git
2. Authentication state model, invariants, actions, transitions, journeys, and scenarios in Git
3. Verified implementation behavior and test evidence
4. Existing DDR text

Existing code does not silently become policy merely because it currently behaves a certain way.

### 1.2 Definition and runtime ownership

- Git owns authored definitions.
- Cosmos DB owns runtime execution records and current operational status.
- Test results must reference the exact definition hash they evaluated.
- A changed definition makes older evidence stale until reverified.

## 2. Foundational Model

### 2.1 One composite identity state

At any moment, the identity is in exactly one logical state.

That state is composite and derived from authoritative variables such as:

- Guest session and claimable value
- Pending identity lifecycle
- Durable user resolution
- Credential proof
- Email ownership
- Profile completeness
- Account status
- Home workspace
- Current workspace
- Workspace memberships
- Pending workspace invitations
- MFA and recovery state
- Session capabilities
- Client presentation context

The system must not attempt to encode every possible combination as one enormous state enumeration or diagram.

### 2.2 Atomic actions and deterministic transitions

One transition executes exactly one state-changing action atomically.

Given the same valid composite state, action, and validated inputs, the resulting state, required effects, forbidden effects, and response must be deterministic.

A transition consists of:

- Source-state predicate
- Action key
- Required inputs
- Guards and validation
- Variables permitted to change
- Required effects
- Forbidden effects
- Destination-state transformation
- Applicable invariants

An action that is not valid for the current state must be rejected without unauthorized mutation.

### 2.3 Identity lifecycle terminology

Use these lifecycle terms:

- Guest Identity: anonymous participation that may own an active conversation and claimable value.
- Pending Identity: the secure identity-resolution airlock used during multi-step authentication, registration, linking, invite, and passkey ceremonies. It has no normal application authorization.
- Durable Identity: the resolved canonical AppUser and its linked authentication methods.

Do not use provisional account as a formal term.

### 2.4 Requirement, presentation, and evidence layers

The model is separated into three primary layers.

#### Requirement layer

Defines behavioral truth:

- Composite state predicates
- Logical actions
- Transitions
- Invariants
- Journeys
- AppUserTestScenario definitions

#### Presentation layer

Defines how a requirement is presented or automated:

- AuthView
- AuthViewField
- AuthFieldAction
- Web bindings
- Android bindings
- iOS bindings
- Guided VTM bindings
- Traditional form bindings

Different presentation channels must call the same logical actions and produce the same post-state when given the same valid state and inputs.

#### Evidence layer

Captures implementation proof:

- Server tests
- Web tests
- Android tests
- iOS tests
- VTM conversation tests
- AppUserTestRun records
- AuthRunnerResult records
- Invariant evaluations
- Screenshots, traces, videos, logs, and other artifacts

Not every scenario requires every evidence type. A UI-only scenario such as Auth Welcome to Choose Provider may have web/mobile evidence and no server equivalent.

### 2.5 Existing model roles

#### AppUserTestScenario

Represents one concrete ceremony:

- Concrete preconditions
- Inputs
- Exactly one action
- Concrete expected postconditions
- Expected view
- Expected auth-log events
- Platform status and evidence bindings

#### AuthView

Defines the semantic UI vocabulary for a view:

- Stable ViewId
- Route
- Fields and field finders
- Actions and action finders

#### AuthRunnerPlan

Compiles a scenario plus a platform presentation binding into one executable run plan.

The runner executes and reports observations. The server evaluates pass/fail against postconditions, invariants, and expected effects.

#### AuthTenantStateSnapshot

The existing snapshot is a useful partial assertion and setup DSL. It currently mixes state assertions, setup operations, side effects, and UI expectations. The new model should gradually separate these concerns into:

- AuthCompositeStatePredicate
- AuthScenarioSetup
- AuthExpectedEffects
- AuthPresentationExpectation

## 3. Foundational Invariants

The initial invariant catalog must include, at minimum:

1. Every durable user has exactly one home workspace.
2. The durable user is always a member and owner of the home workspace.
3. Registration always completes into the user's home workspace.
4. Accepting an invitation never changes the home workspace.
5. Current workspace must reference a workspace the user may access.
6. Workspace switching is explicit and highly visible.
7. A guest may create claimable value without creating an account.
8. Claiming guest value must be atomic, idempotent, and securely bound to the originating guest session.
9. A PendingIdentity has no normal application authorization.
10. Exactly one authentication flow bucket is active on a PendingIdentity.
11. An invitation does not prove identity.
12. An invitation may only be accepted by an explicitly selected durable identity.
13. Invite email and verified provider email may differ.
14. Identity resolution must not create duplicate durable users.
15. Every transition executes exactly one state-changing action.
16. Every valid transition has a deterministic post-state.
17. Guided VTM onboarding and traditional form onboarding invoke the same logical actions.
18. A consumed invitation cannot be applied again.
19. A disabled user cannot receive a normal authenticated session.
20. Recovery must resolve to an existing durable identity and must not silently create a duplicate user.

These invariants are the initial catalog, not the final catalog. New truths discovered during implementation must be added to Git immediately.

## 4. Major-Area Implementation Plan

Each major area should normally be completed within one working session to maximize continuity. Do not advance to the next major area until the current section is Complete or explicitly Blocked.

If a major area becomes too large, split it into named subsections while preserving the same major-area session whenever practical.

### Status values

- Not Started
- In Progress
- Needs Review
- Blocked
- Complete

### 4.1 Repository model foundation

Status: Not Started

Objective:

- Establish the Git-backed authentication model structure.
- Define stable key conventions, schemas, lifecycle statuses, source references, and definition hashing.
- Add the model manifest and validation expectations.

Expected outputs:

- auth-model directory structure
- model-manifest.json
- schemas for state dimensions, invariants, actions, transitions, journeys, scenarios, and presentation bindings
- key and versioning conventions
- validation rules

Completion criteria:

- All authored definition types have a schema.
- References can be validated.
- Definitions can be hashed deterministically.
- Proposed, reviewed, approved, implemented, verified, and deprecated maturity states are defined.

### 4.2 Composite identity state dimensions

Status: Not Started

Objective:

- Define the authoritative variables that compose identity state.
- Separate canonical identity state from session and client projections.

Must include:

- Guest identity
- Claimable value
- PendingIdentity
- Durable user resolution
- Credential proof
- Email ownership
- Profile completeness
- Account status
- Workspace state
- Invitation state
- MFA
- Recovery
- Session capability
- Presentation context

Completion criteria:

- Every dimension has stable values and semantics.
- Invalid combinations are identified through invariants rather than giant enumerations.
- State snapshots can be normalized and fingerprinted.

### 4.3 Invariant catalog

Status: Not Started

Objective:

- Convert the foundational invariants in this guide into authored Git definitions.
- Separate state, transition, and side-effect invariants.

Completion criteria:

- Every invariant has a stable key.
- Applicability and failure meaning are defined.
- Scenarios and transitions can reference invariant keys.

### 4.4 Guest and claimable-value flow

Status: Not Started

Objective:

- Model anonymous chat participation before account creation.
- Model the value signal that recommends account creation.
- Preserve guest conversation and produced value through identity resolution.

Key journey:

Guest starts chat, creates value, chooses account creation, authenticates, resolves a durable identity, and claims the conversation and artifacts.

Completion criteria:

- Guest session identity is stable and secure.
- Value state is modeled explicitly.
- Account creation is recommended rather than universally required.
- Claim operation is atomic and idempotent.

### 4.5 PendingIdentity lifecycle

Status: Not Started

Objective:

- Model PendingIdentity as the shared airlock for password, native provider, OAuth, passkey, linking, invite, and incomplete-registration flows.

Completion criteria:

- Flow bucket invariant is enforced.
- Pending, verification-required, verifying, resolved, expired, canceled, and failed outcomes are modeled.
- PendingIdentity cannot receive normal application authorization.

### 4.6 Password registration

Status: Not Started

Objective:

- Model password-based registration through PendingIdentity to DurableIdentity.

Completion criteria:

- Email proof, profile completion, password establishment, durable-user creation, home-workspace creation, and session establishment are separate deterministic actions.

### 4.7 Native-provider registration

Status: Not Started

Objective:

- Model Google and Apple registration, existing-user resolution, linking, and email mismatch behavior.

Completion criteria:

- Provider subject is treated as the stable provider identity.
- Provider email, invited email, and user-entered email are not conflated.
- Existing-user linking requirements are explicit.

### 4.8 Passkey registration and authentication

Status: Not Started

Objective:

- Model passkey challenge creation, credential creation, attestation validation, pending identity resolution, existing-user registration, authentication, and step-up.

Completion criteria:

- New-user passkey creation does not require premature durable AppUser creation.
- Challenge and credential state are safely associated with the PendingIdentity or durable user as appropriate.
- Begin and complete operations are modeled as separate actions.

### 4.9 Home workspace creation

Status: Not Started

Objective:

- Establish the rule that every durable user receives one home workspace during registration.

Completion criteria:

- Registration no longer branches on invitation presence to decide workspace creation.
- Home workspace, current workspace, membership, and invitation concepts are distinct.
- Concierge presentation requirements for workspace visibility are defined.

### 4.10 Invitation resolution

Status: Not Started

Objective:

- Model invite review and acceptance for logged-in, logged-out, existing-account, and new-account paths.

Underlying paths:

- Logged in, accept with current account
- Logged in, use another existing or new account
- Logged out, create a new account then accept
- Logged out, sign into an existing account then accept

Primary experience:

- Guided Concierge or agent-led invite resolution

Fallback experience:

- Traditional direct invite flow

Completion criteria:

- All paths converge on explicit durable-identity selection.
- Invitation acceptance is a separate atomic action.
- Home workspace remains unchanged.
- Membership creation and invite consumption are atomic and idempotent.

### 4.11 Guided VTM onboarding

Status: Not Started

Objective:

- Model conversational collection of registration inputs through an HR VTM.

Completion criteria:

- Conversational turns that gather or clarify data are not falsely modeled as authentication transitions.
- The VTM invokes the same logical actions as traditional forms.
- The VTM can pause, resume, explain validation failures, and preserve state safely.

### 4.12 Traditional form onboarding

Status: Not Started

Objective:

- Map traditional registration forms onto the same action and transition model used by the VTM.

Completion criteria:

- Form flow does not introduce alternate behavioral rules.
- Presentation bindings cover fields, actions, routes, and expected views.

### 4.13 Session projection and routing

Status: Not Started

Objective:

- Reconcile AuthSessionSnapshot with the broader composite state model.
- Separate canonical identity state, session capability, recommended actions, required actions, allowed actions, and client navigation.

Known concerns:

- Registered and ProfileComplete currently overlap.
- EmailVerificationPending is described as a hard-stop but does not directly drive NextPath.
- Claims may be stale relative to durable state.
- GET /api/auth/session consumes entry intent and therefore performs a state-changing action.
- Unauthenticated guest chat must not always redirect to /auth.

Completion criteria:

- Logical required/recommended actions are independent from platform-specific routes.
- Guest chat remains available when permitted.
- Entry-intent consumption is explicitly modeled.

### 4.14 Account recovery

Status: Not Started

Objective:

- Model password recovery, lost passkey, lost email access, MFA recovery, lockout, compromised account, recovery codes, cooldowns, and session revocation.

Completion criteria:

- Recovery resolves only to an existing durable identity.
- Proof strength and permitted capabilities are explicit.
- Recovery cannot silently accept invitations or switch workspace.

### 4.15 UI-only transitions and AuthView coverage

Status: Not Started

Objective:

- Catalog UI-only transitions such as Auth Welcome to Choose Provider.
- Map AuthView, AuthViewField, and AuthFieldAction to logical actions and platform bindings.

Completion criteria:

- UI-only scenarios do not require meaningless server evidence.
- Web, Android, iOS, and VTM evidence requirements are explicit per scenario.
- Stable semantic finders and ViewIds are validated.

### 4.16 Test binding and evidence aggregation

Status: Not Started

Objective:

- Bind tests to scenarios through attributes.
- Allow multiple tests to provide evidence for one scenario.
- Define scenario-level status aggregation.

Expected evidence statuses:

- Uncovered
- Partial
- Passing
- Failing
- Stale
- Blocked

Completion criteria:

- Test attributes identify scenario key, platform, test level, and evidence role without duplicating behavioral expectations.
- Scenario status is not last-test-wins.
- Results reference the scenario definition hash.

### 4.17 Visualization and generated projections

Status: Not Started

Objective:

- Generate human-sized views from the shared model.

Required projections:

- Journey map
- Region statechart
- Scenario transition view
- Coverage matrix
- Changed-dimensions view

Completion criteria:

- The complete composite state is available on drill-down.
- Diagrams emphasize changed variables and hide irrelevant unchanged dimensions.
- Coverage and failure status can overlay the model.

### 4.18 Cosmos reconciliation

Status: Not Started

Objective:

- Import Git definitions into Cosmos while preserving runtime evidence appropriately.

Completion criteria:

- Git definitions remain authoritative.
- Runtime records are reconciled by stable key and definition hash.
- Changed definitions mark older results stale.
- Cosmos editing cannot silently fork definition truth.

### 4.19 DDR reconciliation

Status: Not Started

Objective:

- Fold approved learned truth back into the authentication DDR after the model and implementation stabilize.

Completion criteria:

- Every extension or conflict is resolved deliberately.
- DDR prose accurately reflects the implemented and verified state model.
- Traceability from DDR clauses to invariants, actions, transitions, and scenarios is preserved.

## 5. Git Model Layout

The initial target structure is:

```text
auth-model/
  model-manifest.json
  state/
    dimensions.json
    values.json
  invariants/
  actions/
  transitions/
  journeys/
  scenarios/
  presentation/
    views/
    bindings/
      web/
      android/
      ios/
      vtm/
  decisions/
  unresolved/
  schemas/
```

One scenario should normally be stored per file to keep diffs readable and reduce editing collisions.

Generated projections such as diagrams, matrices, documentation tables, Cosmos import payloads, and composite-state fingerprints should not be manually maintained as parallel truths.

## 6. Definition Maturity

Every authored definition must declare one maturity state:

- discovered
- proposed
- reviewed
- approved
- implemented
- verified
- deprecated

Generated first-pass content must never masquerade as approved truth.

Uncertainty must be explicit through fields such as:

- sourceReferences
- confidence
- openQuestions
- conflicts
- rationale

## 7. Working-Session Protocol

Each major area should normally receive one dedicated session.

### 7.1 Starting a session

A future session can begin with a command such as:

> Read AUTH-MODEL-GUIDE.md and the current auth-model manifest. We are working in Section 4.7, Native-provider registration. Inspect the authoritative Git definitions and current implementation before proposing changes.

The session must then:

1. Read this guide.
2. Read model-manifest.json once it exists.
3. Read the selected section's authored model files.
4. Read the section handoff.
5. Inspect current implementation and tests relevant to the section.
6. Identify conflicts, extensions, and open decisions before changing definitions.

### 7.2 During a session

- Update Git truth as decisions are made.
- Do not rely on conversation memory as the durable record.
- Keep behavioral definitions separate from presentation bindings and runtime evidence.
- Do not silently alter early invariants to accommodate later code.
- Record conflicts explicitly.
- Stay within the current major area until Complete or Blocked.

### 7.3 Completing a session

Before declaring the major area complete:

- Update the section status.
- Update the model manifest.
- Update affected definitions and source references.
- Record decisions and unresolved questions.
- Record implementation conflicts.
- Record tests and evidence completed or still required.
- Update the section handoff.

## 8. Section Handoff Format

Each major section must maintain a compact handoff using this structure:

```text
Status:
Scope completed:
Decisions made:
Invariants added or changed:
Definitions created or updated:
Implementation files inspected or changed:
Scenarios completed:
Evidence completed:
Known DDR conflicts:
Known implementation conflicts:
Open questions:
Completion criteria remaining:
Recommended next action:
```

The handoff is section-scoped, not a diary of every small step.

## 9. Initial Bootstrap Strategy

The first broad modeling pass should inspect:

- The current authentication DDR
- LagoVista.UserAdmin.Models.Testing
- PendingIdentity
- AuthSessionSnapshot and routing logic
- Registration managers and endpoints
- Invite handling
- Organization and membership logic
- Passkey managers and challenge storage
- Existing AuthView definitions
- Existing web and mobile auth routes and screens
- Existing server and UI tests

The bootstrap may generate a broad first-pass model corpus, but each item must be marked according to its actual confidence and maturity.

The first implementation milestone is:

> A Git-backed authentication model containing schemas, initial state dimensions, foundational invariants, and one complete guest-to-invited-member journey, validated and renderable as a simple journey diagram.

## 10. Current Handoff

Status: Planning baseline established

Scope completed:

- Established one composite identity state model.
- Established one atomic action per transition.
- Established deterministic transitions and invariant validation.
- Established GuestIdentity, PendingIdentity, and DurableIdentity terminology.
- Established Git as definition authority and Cosmos as runtime projection.
- Established requirement, presentation, and evidence layers.
- Established home-workspace invariant.
- Established guest chat and claimable-value onboarding.
- Established guided VTM and traditional form equivalence.
- Established invitation-resolution paths.
- Established test-scenario binding and UI-only scenario expectations.

Open questions:

- Final repository location and naming conventions for auth-model files.
- Exact schema shapes and stable-key conventions.
- Initial model bootstrap breadth versus review depth.
- Exact evidence-role taxonomy and aggregation rules.

Recommended next action:

- Begin Section 4.1, Repository model foundation.
