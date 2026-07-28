# Section 4.8 Handoff: Passkey Registration and Authentication

## Status

Complete.

## Scope completed

Section 4.8 models passkeys as four distinct journeys:

1. New-user registration through PendingIdentity.
2. Existing-user passkey registration.
3. Passkey authentication.
4. Passkey step-up.

Each ceremony separates challenge issuance from completion and treats challenges as short-lived, origin-bound, relying-party-bound, identity-bound, and single-use.

## Key decisions

- New-user passkey attestation does not create a durable user.
- Verified credential material remains associated with PendingIdentity until durable identity resolution permits creation or selection.
- Durable-user creation and credential binding are separate state-changing actions.
- Existing-user credential registration cannot create or switch durable identity.
- Authentication resolves an existing credential owner and never creates a user.
- Step-up requires user verification and does not change durable identity.
- Credential ownership, challenge ownership, user handle, relying party, origin, and signature counter are independently validated.
- Replayed challenges and credential-owner mismatches are explicit security scenarios.

## Implementation gap exposed

`AppUserPasskeyManager.CompletePasswordlessRegistrationAsync` currently validates attestation, creates the durable AppUser, and persists the credential within one operation. The canonical model requires these to become separate operations:

1. Complete passkey attestation into PendingIdentity.
2. Resolve whether to select an existing user or create a new durable user.
3. Atomically bind the verified credential to the resolved user.

This is intentionally recorded as an implementation gap rather than normalized into the model.

## Model inventory after completion

- State dimensions: 17
- Invariants: 20
- Actions: 34
- Transitions: 34
- Journeys: 9
- Scenarios: 33
- Conversation types: 1

## Next major area

Section 4.9: Home workspace creation.
