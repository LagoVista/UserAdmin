# Composite Identity State

The authentication model treats identity as one logical composite state derived from multiple independently named dimensions.

## Canonical snapshot

A canonical snapshot includes every state dimension where `authority` is `canonical` and `requiredForCanonicalSnapshot` is true. The current catalog contains:

- Guest Identity
- Claimable Value
- Pending Identity Status
- Pending Identity Flow
- Durable Identity
- Credential Proof
- Email Ownership
- Profile Completeness
- Account Status
- Home Workspace
- Current Workspace
- Workspace Membership
- Invitation Status
- MFA State
- Recovery State

A canonical snapshot is normalized by stable dimension key and fingerprinted using the hashing rules in `../CONVENTIONS.md`.

## Derived projections

These dimensions are deliberately excluded from the canonical identity fingerprint:

- Session Capability is derived from canonical identity, account, workspace, MFA, and recovery state.
- Presentation Context describes the interaction adapter and must not change behavioral truth.

The same canonical state and logical action must produce the same behavioral result regardless of whether the action is presented through a guided VTM, traditional forms, mobile UI, server test, or automation runner.

## Invalid combinations

The catalog does not enumerate every legal composite-state combination. Cross-dimension legality is defined through invariants.

Examples of combinations that must become invariant violations include:

- Durable Identity is resolved while Home Workspace remains required after registration completion.
- Current Workspace is external while Workspace Membership does not permit external access.
- Session Capability is authenticated-normal while Account Status is disabled.
- Pending Identity Status is active while Pending Identity Flow is none.
- Invitation Status is accepted while no durable identity was explicitly selected.
- Claimable Value is claimed while Guest Identity has not reached claimed.

## Exact values versus predicates

A runtime state snapshot records one exact value for every required canonical dimension. A transition or scenario precondition may instead use a predicate that constrains only relevant dimensions and treats all omitted dimensions as unconstrained.

## Future refinement

All current dimensions are `proposed`. Later major areas may add values or split dimensions when real transitions demonstrate that additional state is behaviorally significant. Material changes increment the dimension version and make evidence against the prior definition stale.
