# Section 4.14 Handoff: Account Recovery

## Status

Complete

## Core Decision

Account recovery is a constrained ceremony that restores access to one pre-existing durable identity. It is not registration, identity creation, invitation acceptance, workspace selection, or an alternate route to ordinary authorization.

## Canonical Recovery Lifecycle

1. Request recovery using an enumeration-resistant response.
2. Internally resolve the request to an existing durable identity without exposing account existence to an unproved caller.
3. Issue one policy-approved, single-use, expiring challenge.
4. Verify recovery proof and consume the challenge once.
5. Establish replacement credentials through secure UI.
6. Revoke prior sessions and refresh tokens, invalidate remaining recovery material, rotate security stamps or equivalent revocation state, notify the user, and complete recovery.
7. Return to an anonymous session and require a fresh sign-in with the replacement credential.

## Supported Recovery Reasons

The same canonical lifecycle supports password loss, lost passkeys, lost email access, MFA loss, suspected compromise, recovery-code use, and support-assisted recovery. The selected proof method and required proof strength are policy inputs, not alternate behavioral models.

## Security Boundaries

- Conversation and ordinary forms may gather recovery intent and display-safe context.
- Passwords, one-time codes, passkey material, recovery codes, provider tokens, and replacement credentials are collected only by secure components.
- Recovery proof must resolve to the durable identity already bound to the recovery ceremony.
- Recovery never creates a durable user.
- Recovery never accepts invitations or changes home/current workspace.
- Rate limits and cooldowns apply before challenge issuance and after repeated failure.
- A consumed challenge cannot be replayed.
- Normal application authorization remains unavailable during recovery.

## Session Behavior

A proved recovery ceremony receives only authenticated-limited capability. Completing recovery revokes all prior sessions and refresh tokens and produces an anonymous session. A fresh ordinary authentication ceremony is required before normal access resumes.

## Authored Definitions

### Actions

- `auth.action.recovery.request`
- `auth.action.recovery.issue-challenge`
- `auth.action.recovery.verify-proof`
- `auth.action.recovery.establish-replacement-credential`
- `auth.action.recovery.complete`

### Transitions

- `auth.transition.recovery.request`
- `auth.transition.recovery.issue-challenge`
- `auth.transition.recovery.verify-proof`
- `auth.transition.recovery.establish-replacement-credential`
- `auth.transition.recovery.complete`

### Journey

- `auth.journey.account-recovery`

### Scenarios

- `auth.scenario.recovery.request`
- `auth.scenario.recovery.issue-challenge`
- `auth.scenario.recovery.verify-proof`
- `auth.scenario.recovery.establish-replacement-credential`
- `auth.scenario.recovery.complete`

## Conversation Binding

`auth.conversation.account-access-assistance` now references the account recovery journey, actions, scenarios, secure handoff, confirmation requirements, and fresh-sign-in outcome.

## Implementation Alignment

Existing forgot-password and reset-password endpoints should be evaluated against this decomposition. Composite endpoints may remain temporarily, but their effects must correspond to the canonical actions and must not hide identity creation, invitation mutation, workspace mutation, or automatic normal-session establishment.

## Next Major Area

Section 4.15: UI-only transitions and AuthView coverage.
