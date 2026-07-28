# Section 4.6 Handoff: Password Registration

Status: Complete

## Scope completed

Password registration is authored as a deterministic sequence that reuses the shared PendingIdentity lifecycle and keeps proof, persistence, workspace, and session mutations separate.

## Authored actions

- `auth.action.password.assert-email`
- `auth.action.password.verify-email`
- `auth.action.password.complete-profile`
- `auth.action.password.establish-credential`
- `auth.action.password.create-durable-user`
- `auth.action.password.create-home-workspace`
- `auth.action.password.establish-session`

The journey begins with the shared `auth.action.pending-identity.begin` action using `flow=password`.

## Authored transitions

- Assert email and require verification
- Verify email ownership
- Complete required profile data
- Establish password credential
- Create exactly one durable user
- Create exactly one home workspace and owner membership
- Complete PendingIdentity and issue a normal authenticated session

## Authored scenarios

Each atomic happy-path transition has a scenario. A security scenario also verifies that a consumed email-verification proof cannot be replayed.

## Key decisions

1. User-entered email is asserted evidence until an explicit verification proof is consumed.
2. Password establishment does not create the durable user.
3. Durable-user creation does not create the home workspace or grant normal authorization.
4. Home-workspace creation is independent from invitation presence.
5. A normal authenticated session is issued only after the durable identity, active account, home workspace, owner membership, and current home context all exist.
6. The completed password journey leaves `pending-identity-status=resolved`, `pending-identity-flow=none`, and `session-capability=authenticated-normal`.

## Model inventory after completion

- State dimensions: 17
- Composite-state catalogs: 1
- Invariants: 20
- Actions: 17
- Transitions: 17
- Journeys: 3
- Scenarios: 18

## Next major area

Section 4.7: Native-provider registration.

The next section should model provider-subject identity, provider email as distinct evidence, existing-user resolution, explicit linking, and email mismatch behavior for Apple and Google adapters.