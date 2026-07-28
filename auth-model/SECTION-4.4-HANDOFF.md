# Section 4.4 Handoff: Guest and Claimable-Value Flow

## Status

Complete.

## Authored definitions

### Actions

- `auth.action.guest.activate`
- `auth.action.guest.detect-value`
- `auth.action.guest.mark-value-claimable`
- `auth.action.guest.claim-value`

### Transitions

- `auth.transition.guest.activate`
- `auth.transition.guest.detect-value`
- `auth.transition.guest.mark-value-claimable`
- `auth.transition.guest.claim-value`

### Scenarios

- `auth.scenario.guest.activate`
- `auth.scenario.guest.detect-value`
- `auth.scenario.guest.mark-value-claimable`
- `auth.scenario.guest.claim-value`

### Journey

- `auth.journey.guest.claimable-value`

## Decisions preserved

1. Anonymous participation may create meaningful value before account creation or sign-in.
2. Guest identity activation does not create a durable user or grant normal application authorization.
3. Detecting value and promoting it to claimable are separate atomic actions.
4. Claiming value requires an independently resolved durable identity.
5. The claim is atomic, idempotent, bound to the originating guest session, and replay safe.
6. A failed claim may not partially attach resources, create a second durable user, or attach value to an unselected identity.
7. Guided VTM, traditional web, Android, and iOS presentations share the same behavioral journey.

## State progression

`guest:none / value:none / durable:absent`

→ activate guest

`guest:active / value:none / durable:absent`

→ detect value

`guest:active / value:detected / durable:absent`

→ mark value claimable

`guest:active / value:claimable / durable:absent`

→ resolve durable identity through an independent registration or authentication journey

`guest:active / value:claimable / durable:resolved`

→ claim guest value

`guest:claimed / value:claimed / durable:resolved`

## Visualization implication

The model viewer can now present its first complete journey. A future viewer enhancement should render ordered scenarios and transitions, changed dimensions, required effects, and invariant coverage from these authored definitions.

## Next major area

Section 4.5: PendingIdentity lifecycle.
