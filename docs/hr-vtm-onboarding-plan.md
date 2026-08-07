# HR VTM Onboarding Plan

**Status:** Planning  
**Scope:** Phase 1 — brand-new visitor to paying customer  
**Last updated:** 2026-08-07

## Goal

Create a conversational onboarding experience that takes a brand-new visitor from their first VTM conversation through establishing a permanent account and becoming a paying customer.

The north-star journey is:

> Brand-new stranger → meaningful VTM conversation → preserved work → real account → paying customer

The user should not encounter a registration wall before they have experienced value.

## North-Star UX

A visitor should be able to start working immediately.

On web, the experience should feel like:

> “Just start. We’ll remember you on this browser.”

On mobile:

> “Just start. We’ll remember you on this device.”

Once the visitor has created something worth preserving, the current VTM should gently introduce the HR VTM (Bob):

> “We’ve got something worth keeping here. I’m going to introduce you to Bob in HR, who can get your account set up so we can save your work.”

If the visitor has not yet created an account and there is meaningful work at risk:

> “You haven’t created an account yet. I can remember you on this device, but if you clear your browser or switch devices you could lose what we’ve done. Want to save a recovery code just in case?”

The underlying identity/security machinery can be sophisticated, but it should remain nearly invisible to the user.

## Core Invariants

1. **Conversation first.** Everything starts in a conversation with a VTM/agent.
2. **No registration wall.** A new visitor can meaningfully use the real system before creating an account.
3. **Not authenticated does not mean demo user.** A provisional visitor should have a legitimate workspace and be able to create meaningful work.
4. **Stable identity from the beginning.** Work, conversations, context, and handoffs must have stable user/org identifiers.
5. **The VTM facilitates security operations; it does not perform them.** Credential collection, verification, authentication, and payment happen in trusted client UI.
6. **Secrets never enter agent context.** Passwords, verification codes, OAuth tokens, card data, and recovery credentials are not exposed to the agent.
7. **Promote rather than migrate where practical.** Account creation should preserve the provisional workspace, conversation history, and work rather than requiring a data migration.
8. **Recovery is gentle and mostly invisible.** Same browser/device continuity is automatic; a human-friendly recovery code is the fallback.
9. **Email/password is the universal bootstrap path.** After account creation, the user can be shown other supported authentication mechanisms.

## Working Identity Model

The VTM runtime already expects a user and organization. Rather than creating a separate anonymous-user universe, the working direction is to establish a real but explicitly **provisional** user and organization.

Conceptually:

`Provisional User + Provisional Org + Agent Session`

These identifiers are normal runtime anchors for conversations and work but do not represent a verified/authenticated person.

A provisional user must not be able to use the provisional identity to cross security boundaries such as accessing an existing organization, another user's resources, or privileged security/admin operations.

The exact data model and lifecycle are still to be designed.

## Phase 1 Journey

### 1. First visit

Silently establish the provisional identity/workspace needed by the VTM runtime.

The visitor is allowed to begin a normal VTM conversation immediately.

### 2. Continuity

The client remembers the provisional workspace:

- Web: browser-held recovery/continuity credential.
- Mobile: device identity plus appropriate continuity credential.
- Fallback: a human-friendly recovery code the visitor can save.

The recovery code grants access only to the provisional workspace. Once the workspace is claimed by a permanent account, it must not remain an authentication mechanism for that account.

### 3. Create value

The visitor works normally with one or more VTMs. Work and conversation context are owned by the provisional user/org.

The experience should not feel like a crippled trial.

### 4. Handoff to HR

When the current VTM determines there is something worth preserving, it hands the visitor to Bob/HR with enough context to continue naturally.

The handoff includes the stable identity, conversation/context, relevant work, and current onboarding/account state.

### 5. Establish account

Bob explains why establishing an account is useful and launches trusted client UI for account creation.

Initial bootstrap path:

1. Email address + password.
2. Verify control of email using the six-digit verification-code flow.
3. Establish/promote the permanent user/home organization.
4. Preserve/claim the provisional work and conversations.
5. Return control to Bob with the updated account state.

Bob can explain that other authentication mechanisms may be connected/used going forward.

### 6. Commercial activation

Once the account is established, Bob guides the user through selecting/starting the appropriate subscription.

Payment details are collected only through trusted payment UI/provider integration. Bob receives business state such as `SubscriptionActivated`, never card data.

The Phase 1 journey is complete when the visitor's valuable work is owned by a verified account with the intended paid subscription.

---

## Work Buckets

### Bucket 1 — Provisional Identity + Workspace

**Objective:** Give every new visitor the stable user/org identity the VTM runtime needs without requiring registration.

- [ ] Define provisional user representation/state.
- [ ] Define provisional organization representation/state.
- [ ] Define creation lifecycle on first conversation.
- [ ] Define security/authorization boundaries for provisional principals.
- [ ] Define expiration/cleanup policy for abandoned provisional identities.
- [ ] Confirm existing user/org services can tolerate provisional entities.
- [ ] Decide whether promotion retains the same UserId/OrgId or uses an explicit claim mapping.

**Key decision pending:** Exact representation of provisional state in `AppUser`, organization, and session context.

### Bucket 2 — Browser / Device Continuity

**Objective:** Let a visitor return to the same provisional workspace without creating an account.

- [ ] Define server-side continuity/recovery credential.
- [ ] Define browser storage strategy.
- [ ] Define mobile device identity strategy.
- [ ] Define human-friendly recovery-code format and lifecycle.
- [ ] Implement recovery-code rotation/revocation as necessary.
- [ ] Define behavior after a provisional workspace is promoted.
- [ ] Define lost/invalid/expired recovery behavior.
- [ ] Threat-model credential theft and replay.

**UX principle:** Most users should never need to think about the recovery code.

### Bucket 3 — Provisional VTM Experience

**Objective:** Make the normal agent/VTM environment work naturally with a provisional identity.

- [ ] Add account/provisional state to agent/session context.
- [ ] Confirm conversation persistence uses the provisional user/org.
- [ ] Confirm artifact/work ownership uses the provisional user/org.
- [ ] Identify operations that must be unavailable before verification.
- [ ] Identify expensive/external side effects requiring additional gating, if any.
- [ ] Confirm handoffs between VTMs preserve provisional context.
- [ ] Verify the experience is meaningfully useful rather than a limited demo.

### Bucket 4 — Value → HR Handoff

**Objective:** Allow another VTM to introduce Bob at the point where preserving the user's work becomes valuable.

- [ ] Define the signal/decision that onboarding should be offered.
- [ ] Define the agent-to-agent handoff payload.
- [ ] Include provisional identity and relevant work/context.
- [ ] Define conversational introduction copy/guidance.
- [ ] Define resume behavior after Bob completes onboarding.
- [ ] Ensure the handoff works consistently on web and mobile.

### Bucket 5 — Bob / HR Onboarding State Model

**Objective:** Give Bob a deterministic understanding of where the visitor is and what action can advance them.

Initial conceptual progression:

`Provisional → Account Created → Email Verified → Customer`

- [ ] Define canonical states and transitions.
- [ ] Decide which state is authoritative versus derived.
- [ ] Expose safe state to the HR agent context.
- [ ] Define allowed client directives/actions per state.
- [ ] Define retry/cancel/resume behavior.
- [ ] Define what Bob should do when the user already has an account.
- [ ] Define failure and recovery paths.

**Important:** The agent recommends/facilitates the next state. Trusted server/client flows authorize and perform the transition.

### Bucket 6 — Trusted Client Cards / Directives

**Objective:** Give Bob a small, finite toolbox for sensitive interactions.

Initial expected set:

- [ ] Create Account
- [ ] Verify Email — six-digit code
- [ ] Sign In
- [ ] Select Plan / Start Subscription
- [ ] Enter Payment Method

For each directive/card:

- [ ] Define invocation payload.
- [ ] Define client UI contract for web.
- [ ] Define client UI contract for mobile.
- [ ] Define server endpoint(s).
- [ ] Define safe result/event returned to the agent.
- [ ] Confirm secrets never enter conversation/agent context.
- [ ] Define cancellation and failure behavior.

### Bucket 7 — Account Promotion / Ownership

**Objective:** Turn the provisional identity into a permanent verified account without losing continuity.

Preserve:

- [ ] Work/artifacts
- [ ] Conversation history
- [ ] Agent/VTM context
- [ ] Workspace/home organization
- [ ] Relevant relationships/metadata

Engineering work:

- [ ] Define atomic promotion/claim operation.
- [ ] Handle duplicate/existing email/account cases.
- [ ] Establish home-organization semantics.
- [ ] Retire/revoke provisional recovery credentials after claim.
- [ ] Make the operation idempotent/retry-safe.
- [ ] Add audit/security events.
- [ ] Add integration tests covering interrupted promotion.

### Bucket 8 — Commercial Activation

**Objective:** Take the newly established account from free/provisional usage to the intended paid subscription.

- [ ] Define plan-selection experience.
- [ ] Connect HR directive/card to existing billing/subscription services.
- [ ] Keep payment-card data entirely outside agent context.
- [ ] Define safe subscription status exposed to Bob.
- [ ] Handle payment failure/retry/cancel.
- [ ] Define successful `SubscriptionActivated` transition/event.
- [ ] Confirm access/entitlement changes after activation.
- [ ] Add end-to-end coverage from provisional visitor through paid account.

---

## Existing Capabilities We Expect to Reuse

These are expected to reduce the amount of net-new work:

- Existing `AppUser` and organization/home-tenant infrastructure.
- Existing agent/VTM conversations and handoff/context mechanisms.
- Email/password account creation infrastructure.
- Six-digit email verification flow.
- Password recovery flow.
- Existing OAuth/external authentication providers for post-bootstrap sign-in options.
- Existing billing/subscription infrastructure.

These should be verified during implementation rather than assumed to fit unchanged.

## Security Boundary

The HR VTM is a conversational facilitator, not an authentication authority.

Bob may know facts such as:

- The visitor is provisional.
- Account creation is required.
- Email verification is pending/completed.
- The user is authenticated.
- A subscription is active/not active.

Bob must not receive or process:

- Passwords.
- Email verification codes.
- OAuth/access/refresh tokens.
- Recovery credentials/codes.
- Payment card data.

Sensitive operations are performed by trusted client UI against server-side authentication/billing endpoints.

## Open Questions / Decisions

Track unresolved design questions here as we work them down.

- [ ] What is the exact persistence model for a provisional `AppUser`?
- [ ] What is the exact persistence model for a provisional organization?
- [ ] Do promotion/claim operations keep the original UserId and OrgId?
- [ ] What are the authorization capabilities of a provisional principal?
- [ ] What is the lifetime/cleanup policy for abandoned provisional workspaces?
- [ ] What is the recovery-code format and storage model?
- [ ] How does a visitor recover a provisional workspace on a new device?
- [ ] What happens when the supplied email already belongs to an existing account?
- [ ] At what point does subscription/payment become required?
- [ ] What signal tells a non-HR VTM that enough value exists to offer the HR handoff?

## Progress Log

Add short, durable entries here when a decision is made or a meaningful implementation slice lands.

| Date | Area | Update |
| --- | --- | --- |
| 2026-08-07 | Planning | Defined Phase 1 journey from brand-new visitor through paid customer. |
| 2026-08-07 | Identity | Working direction is a provisional User + Org rather than a separate anonymous-user domain. |
| 2026-08-07 | Continuity | Same browser/device should recover invisibly; human-friendly recovery code is the fallback. |
| 2026-08-07 | UX | Registration is deferred until there is something the visitor cares about preserving. |
| 2026-08-07 | Security | VTM facilitates auth/payment flows; trusted UI performs them and secrets never enter agent context. |

## Current Next Step

Define **Bucket 1 — Provisional Identity + Workspace** precisely enough to answer:

1. What records exist before the visitor has an account?
2. How are those records marked provisional?
3. What permissions does that identity have?
4. What changes when Bob successfully establishes the permanent account?

Once those are stable, continuity/recovery and the HR state model can be designed against a concrete identity lifecycle.
