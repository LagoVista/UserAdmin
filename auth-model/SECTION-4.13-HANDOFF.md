# Section 4.13 Handoff: Session Projection and Routing

## Status

Complete

## Decision

Authentication session projection is a read-only derivation over authoritative authentication state. It is not the authoritative state itself and must not perform hidden state changes.

The projection is separated into five concerns:

1. **Canonical state**: durable identity, account, proof, workspace, invitation, MFA, recovery, and other authoritative dimensions.
2. **Session capability**: anonymous, identity-ceremony, authenticated-limited, authenticated-normal, step-up, or blocked.
3. **Logical actions**: required, recommended, and allowed actions derived from canonical state and policy.
4. **Entry intent**: ephemeral application-entry context that may inform presentation only after explicit one-time consumption.
5. **Client navigation**: a presentation adapter maps logical actions and entry intent to platform-specific routes or views.

## Projection Contract

A session projection may:

- read authoritative state;
- derive session capability;
- derive required, recommended, and allowed logical action keys;
- expose display-safe workspace and identity context;
- expose whether a valid entry intent is available;
- provide logical completion or blocking reasons.

A session projection must not:

- consume entry intent;
- mutate identity, account, invitation, workspace, MFA, or recovery state;
- authenticate a user;
- create or revoke a session;
- choose a platform-specific route as behavioral truth;
- rely on stale claims when fresher durable state is available.

## Entry Intent

`IEntryIntentService.ConsumeAsync` is destructive and therefore represents an explicit action, transition, and scenario:

- `auth.action.entry-intent.consume`
- `auth.transition.entry-intent.consume`
- `auth.scenario.entry-intent.consume`

The new projected dimension `auth.dimension.entry-intent-status` distinguishes none, stashed, consumed, expired, and invalid states.

Reading `/api/auth/session` must not consume entry intent. A client or orchestration layer may request explicit consumption after it has selected the appropriate logical flow.

## Routing Contract

Routing is a presentation concern:

```text
Canonical state
  -> Session capability
  -> Required / recommended / allowed logical actions
  -> Presentation adapter
  -> Platform route or view
```

The same logical action may map to different web, Android, iOS, or VTM surfaces. A route may not redefine guards, authorization, required effects, or post-state.

Guest chat remains available whenever anonymous session capability and policy permit it. The presence of an authentication-related recommended action does not automatically force navigation to `/auth`.

## Implementation Alignment

1. Make session retrieval a pure read.
2. Move entry-intent consumption to a dedicated endpoint or explicit operation.
3. Return logical action keys independently from route strings.
4. Distinguish required actions from recommended and allowed actions.
5. Derive session capability from current durable state rather than trusting stale claims alone.
6. Let presentation bindings select routes and views for each platform.
7. Preserve anonymous guest capabilities when policy permits them.

## Known Current Seams

- `Registered` and `ProfileComplete` appear to overlap in the current session projection.
- `EmailVerificationPending` is described as a hard stop but does not consistently drive navigation.
- Existing claims may lag durable account, workspace, MFA, or recovery state.
- The current session read path may consume entry intent.
- Some clients treat `NextPath` as both recommendation and authorization.

These are implementation-alignment issues. They do not change the canonical model.

## Completion Criteria Met

- Logical required and recommended actions are independent from platform routes.
- Session capability is explicitly a non-canonical projection.
- Entry-intent consumption is explicitly modeled as a mutation.
- Guest capability is not automatically collapsed into an auth redirect.
- Client navigation is downstream from logical actions and presentation context.
