# Section 4.9 Handoff: Home Workspace Establishment

## Status

Complete.

## Scope

Section 4.9 establishes home-workspace creation as a canonical, registration-method-independent lifecycle. Password, native-provider, passkey, invitation-assisted, and future registration journeys should converge on these definitions rather than owning separate workspace semantics.

## Authored definitions

### Actions

- `auth.action.home-workspace.begin-creation`
- `auth.action.home-workspace.complete-creation`
- `auth.action.home-workspace.reconcile-existing`
- `auth.action.home-workspace.flag-conflict`

### Transitions

- `auth.transition.home-workspace.begin-creation`
- `auth.transition.home-workspace.complete-creation`
- `auth.transition.home-workspace.reconcile-existing`
- `auth.transition.home-workspace.flag-conflict`

### Scenarios

- `auth.scenario.home-workspace.begin-creation`
- `auth.scenario.home-workspace.complete-creation`
- `auth.scenario.home-workspace.reconcile-existing`
- `auth.scenario.home-workspace.flag-conflict`

### Journey

- `auth.journey.home-workspace.establishment`

## Decisions

1. Every resolved durable identity must converge on exactly one home workspace.
2. Home-workspace creation is idempotent and begins under an explicit operation boundary.
3. Workspace creation, owner membership, and deterministic selection of the home workspace commit as one valid completion outcome.
4. One valid pre-existing home workspace may be reconciled without creating a replacement.
5. Duplicate home workspaces or invalid ownership relationships are never resolved by choosing an arbitrary candidate.
6. Conflict blocks registration completion and normal authorization until deterministic repair occurs.
7. Invitation presence does not decide whether a home workspace is created and invitation acceptance does not replace the home workspace.

## Follow-on implementation guidance

The method-specific workspace actions currently authored under password and native-provider registration should eventually delegate to, reference, or be deprecated in favor of the canonical home-workspace lifecycle. Runtime implementation should expose idempotency keys and transactional or compensating guarantees across workspace creation, owner membership, and current-workspace selection.

## Next major area

Section 4.10: Invitation resolution.
