# Section 4.10 Handoff

## Status

Complete.

## Decision

Invitation resolution is a membership workflow layered on independently resolved identity. Every durable user already has exactly one home workspace. Accepting an invitation never creates, replaces, or changes that home workspace.

## Canonical flow

1. Attach and validate invitation proof.
2. Resolve or authenticate a durable identity using ordinary authentication or registration.
3. Explicitly select and confirm the durable identity that will accept.
4. Atomically create one external workspace membership and consume the invitation.
5. Leave the current workspace unchanged. A later workspace switch is a separate explicit action.

## Supported presentation routes

- Logged in, use the current account.
- Logged in, authenticate another account.
- Logged out, sign into an existing account.
- Logged out, complete normal registration and receive a home workspace first.

All four routes converge on `auth.transition.invitation.select-identity` and `auth.transition.invitation.accept`.

## Security boundaries

- Invitation possession is never identity proof.
- Invite email, provider email, verified account email, and user-entered email remain distinct evidence.
- Acceptance requires an explicitly confirmed durable identity.
- A consumed invitation cannot be applied again.
- Invitation acceptance does not switch the current workspace.

## Authored definitions

Actions:
- `auth.action.invitation.validate`
- `auth.action.invitation.select-identity`
- `auth.action.invitation.accept`
- `auth.action.invitation.decline`

Transitions:
- `auth.transition.invitation.validate-valid`
- `auth.transition.invitation.validate-expired`
- `auth.transition.invitation.validate-invalid`
- `auth.transition.invitation.select-identity`
- `auth.transition.invitation.accept`
- `auth.transition.invitation.decline`

Journey:
- `auth.journey.invitation.resolve`

Scenarios cover valid invitation validation, all four identity-routing paths, atomic acceptance, and consumed-invitation replay rejection.

## Next major area

Section 4.11: guided onboarding and presentation equivalence.