# Section 4.5 Handoff: PendingIdentity Lifecycle

## Status

Complete.

## Authored definitions

- 7 lifecycle actions
- 7 deterministic transitions
- 5 scenarios
- 1 aggregate lifecycle journey

## Lifecycle coverage

The shared PendingIdentity airlock now covers:

1. Begin with exactly one selected flow.
2. Require additional verification.
3. Begin verification.
4. Complete verification and advance to resolution-required.
5. Resolve to exactly one canonical durable identity.
6. Expire without durable resolution.
7. Cancel explicitly.
8. Fail terminally after non-recoverable validation, security, or provider failure.

## Security guarantees represented

- An active PendingIdentity remains limited to `identity-ceremony` capability.
- Normal application authorization is forbidden until durable identity resolution.
- Exactly one authentication flow bucket may be active.
- Terminal outcomes clear the active flow.
- Resolution must bind to exactly one canonical durable identity.
- Identity resolution may not silently create a duplicate durable user.

## Reuse boundary

The lifecycle actions are intentionally flow-agnostic. Password, native-provider, OAuth-external, and passkey sections should supply flow-specific proof actions and then reuse the common verification, resolution, and terminal lifecycle transitions.

## Next major area

Section 4.6: Password registration.

That section should model email proof, profile completion, password establishment, durable-user creation, home-workspace creation, and session establishment as separate deterministic actions built on this PendingIdentity lifecycle.