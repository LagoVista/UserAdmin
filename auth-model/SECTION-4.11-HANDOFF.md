# Section 4.11 Handoff: Guided VTM Onboarding

## Status

Complete

## Canonical decision

Guided onboarding is an orchestration and presentation layer over the canonical authentication model. Conversational turns that explain, gather non-secret information, clarify intent, or route between bounded workflows are not authentication state transitions.

A conversation may request a canonical action only when:

1. the action is explicitly listed in the conversation type's `allowedActionKeys`,
2. authoritative state has been refreshed,
3. the action's guards are satisfied,
4. any required confirmation was obtained for the current authoritative state, and
5. secret proof was collected by a standard secure component and returned only as a sanitized outcome.

The VTM does not receive special authentication powers. Traditional forms and guided conversations must invoke the same logical actions and therefore produce the same canonical transitions, effects, invariant obligations, and audit evidence.

## Conversation architecture

### HR Onboarding router

`auth.conversation.hr-onboarding` is a non-mutating router. It identifies the immediate goal and transfers orchestration to exactly one bounded conversation. It does not collect secrets, perform workflow-specific actions, or treat conversational confidence as authorization.

### Bounded conversations

The current bounded conversation catalog contains:

- account access assistance,
- user registration,
- invitation resolution,
- user invitation,
- organization creation, and
- organization switching.

User registration and invitation resolution are now bound to their currently authored canonical journeys, action keys, confirmation requirements, secure interactions, and scenario coverage. The remaining bounded conversations stay proposed until their underlying canonical action and journey areas are authored.

## Secure interaction boundary

Passwords, one-time codes, passkeys, provider tokens, recovery codes, and equivalent authentication secrets are collected only by standard secure UI components. The conversation may provide display-safe context and a return location, but receives only sanitized outcomes such as:

- completed,
- canceled,
- failed,
- additional verification required, or
- expired.

After every secure handoff, the conversation discards assumptions and reloads authoritative state before selecting the next permitted action.

## Pause and resume contract

Conversation progress is not authoritative authentication state.

On pause, the system may persist:

- the bounded conversation key,
- non-secret collected information,
- references to PendingIdentity, invitation, claimable value, or other canonical records,
- the proposed next action,
- explicit confirmation state, and
- secure-handoff state.

On resume, the system must reload canonical identity, invitation, membership, workspace, proof, and account state. A prior recommendation or confirmation cannot authorize an action if relevant state changed while paused.

## Validation failures

The conversation may translate canonical guard, validation, and invariant failures into human-readable explanations. It must not:

- weaken the failed rule,
- fabricate a successful outcome,
- silently choose another identity,
- replay a stale action,
- create an alternate conversational-only transition, or
- conceal that a secure or traditional workflow is required.

## Traditional UI equivalence

Every guided workflow must expose an equivalent traditional entry point. Presentation may differ, but both surfaces must invoke the same logical actions with equivalent inputs, confirmations, guards, and post-state expectations.

Detailed route, field, view, and control bindings belong to Section 4.12.

## Updated definitions

- `auth-model/conversations/hr-onboarding.json`
- `auth-model/conversations/user-registration.json`
- `auth-model/conversations/invitation-resolution.json`

## Verification implications

A guided scenario is correct only when evidence demonstrates:

- conversational collection excludes secrets,
- secure components receive secret inputs directly,
- sanitized outcomes resume the correct bounded conversation,
- authoritative state is refreshed after pause or handoff,
- the invoked action key matches the traditional flow,
- changed state matches the canonical transition, and
- failed guards remain failed across all presentation channels.

## Next major area

Section 4.12: Traditional form onboarding and presentation bindings.