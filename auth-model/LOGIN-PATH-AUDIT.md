# Login Path Audit

## Purpose

This document defines a future code-driven audit of every authentication path that can establish a user session or issue authenticated tokens.

It is intentionally complementary to `CATEGORY-TO-GREEN.md` and the canonical auth behavior model.

Category-to-Green asks:

> Are the intended authentication behaviors implemented, represented, and evidenced?

This audit asks the same system a different question from roughly ninety degrees away:

> Starting from every real authentication entry point in the codebase, where can execution go, how is identity proven, where do the paths converge, and do all successful outcomes terminate consistently?

The goal is to catch forgotten side roads, legacy entry points, inconsistent completion paths, misleading audit semantics, and other implementation facts that a model-first review may not reveal.

## When To Run This Audit

Do not treat this as the next implementation task while authentication methods are still being completed.

Run the audit after the remaining login methods are functionally complete and the real client/test runner is exercising the canonical behaviors. At that point the authentication surface should be stable enough to inspect end-to-end without repeatedly revisiting the same paths.

## Core Principle

Authentication proof and session establishment are different events.

Examples of proof include:

- Password accepted
- TOTP verified
- Passkey/WebAuthn assertion verified
- Recovery code accepted
- Magic-link proof accepted
- External identity provider proof accepted
- Single-use token accepted

A successful interactive authentication flow may contain one or more proof events. The final authenticated application session is a separate terminal outcome.

The desired future terminal audit event is:

`UserSessionEstablished`

That event should describe the creation of an authenticated application session, independent of how the user proved identity.

## Desired Invariants

The audit should verify at least the following invariants across all real authentication paths:

1. Every proof mechanism emits proof-specific success/failure audit events that accurately describe what occurred.
2. A failed proof never establishes an authenticated session.
3. MFA-required flows never establish an authenticated session before the required second factor succeeds.
4. Every successful interactive authentication path emits exactly one terminal `UserSessionEstablished` event.
5. Token-only authentication paths emit zero `UserSessionEstablished` events unless they also explicitly create an application session.
6. Shared session-completion code does not emit proof-specific audit events such as `PasswordAuthenticationSucceeded`.
7. MFA transaction challenges are bound to the correct user and provider and are consumed according to their intended single-use semantics.
8. Authentication paths that establish the same logical outcome converge on the same session-completion semantics where practical.
9. Logout/session termination remains distinct from proof and session establishment and records the correct actor/session context.
10. Audit records identify the correct user, provider/factor, actor, organization, challenge/transaction context, and failure reason where applicable.
11. Legacy endpoints or managers cannot establish sessions through an unmodeled or unevidenced side path.
12. Redirect/navigation completion is not confused with authentication proof or session establishment.

## Audit Method

The audit should be code-first rather than behavior-first.

Begin by discovering every real authentication entry point in the repositories. Follow each path through managers, flow handlers, controllers/endpoints, middleware, identity-provider callbacks, and shared completion services until the path reaches one of these terminal states:

- Authenticated application session established
- Authenticated token(s) issued
- MFA/continuation required
- Authentication rejected
- Account locked/disabled/otherwise prevented
- Provisional or pending identity state
- Redirect/navigation outcome without authenticated state

For each path, record:

1. Entry point
2. Client/server surface
3. Proof mechanism(s)
4. User identity source and binding
5. Intermediate transitions/challenges
6. MFA applicability
7. Session completion mechanism
8. Token issuance mechanism, if any
9. Audit events emitted in execution order
10. Final authenticated state
11. Canonical AuthView/behavior/scenario mapping, if one exists
12. Existing automated test/evidence coverage
13. Any divergence from the desired invariants

## Initial Path Inventory

This list is intentionally provisional. The audit must discover paths from source rather than assume this list is exhaustive.

| Path | Primary proof | MFA possible | Expected terminal outcome |
| --- | --- | --- | --- |
| Password sign-in | Password | Yes | Session or MFA continuation |
| Password + TOTP | Password + TOTP | Yes | Session |
| Password + Passkey | Password + WebAuthn | Yes | Session |
| Password + recovery code | Password + recovery code | Yes | Session |
| Passkey-first sign-in | WebAuthn | Policy-dependent | Session |
| Magic-link sign-in | Signed/one-time link proof | Policy-dependent | Session |
| Google sign-in | External OIDC/OAuth proof | Policy-dependent | Session |
| Apple sign-in | External OIDC/OAuth proof | Policy-dependent | Session |
| Other external-provider sign-in | External provider proof | Policy-dependent | Session |
| Single-use-token grant | Single-use token | No session unless explicitly created | Tokens |
| Password + TOTP token flow | Password/MFA transaction + TOTP | Yes | Tokens |
| Password + recovery-code token flow | Password/MFA transaction + recovery code | Yes | Tokens |
| Invitation/pending-identity completion | Varies | Policy-dependent | Pending identity, session, or continuation |
| Provisional-user authentication/promotion | Varies | Policy-dependent | Provisional or durable session |

Additional paths discovered in source must be added rather than forced into the table above.

## Suggested Working Matrix

During the audit, maintain a matrix similar to:

| Path | Entry point | Proof events | MFA/challenge | Completion code | Terminal audit | Session/token | Canonical model | Tests/evidence | Findings |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Password | TBD | TBD | TBD | TBD | TBD | TBD | TBD | TBD | TBD |
| Password + TOTP | TBD | TBD | TBD | TBD | TBD | TBD | TBD | TBD | TBD |
| Password + Passkey | TBD | TBD | TBD | TBD | TBD | TBD | TBD | TBD | TBD |

The matrix should be populated from inspected source and executable evidence, not assumptions.

## Relationship To Category-to-Green

This audit does not replace Category-to-Green.

The two reviews should converge from opposite directions:

### Category-to-Green

Model-driven:

`Canonical behavior -> scenario -> implementation -> client surface -> test evidence`

It answers whether the behaviors we intended to build are complete and proven.

### Login Path Audit

Code-driven:

`Real entry point -> execution path -> proof -> continuation -> completion -> audit -> terminal state`

It answers whether the implementation contains any unexpected, legacy, inconsistent, or unmodeled path.

A strong final state is reached when both views describe the same authentication surface.

If Category-to-Green contains a behavior with no real code path, that is a modeling/implementation gap.

If the code-driven audit finds a real login/session path that Category-to-Green does not describe, that is an unmodeled implementation path and should be reconciled.

## Audit Event Cleanup

A likely output of this audit is a final normalization of authentication audit events.

The working semantic split is:

### Proof-specific events

Examples:

- `PasswordAuthenticationSucceeded`
- `TotpVerifySuccess`
- Passkey authentication success event
- Recovery-code verification success event
- Magic-link verification success event
- External-provider verification success event

These answer:

> What proof succeeded?

### Terminal session event

`UserSessionEstablished`

This answers:

> Was an authenticated application session actually established?

Shared session completion code should emit the terminal event and should not pretend to know which credential or provider proved identity.

A typical password + TOTP sequence should ultimately resemble:

```text
PasswordAuthenticationStarted
PasswordAuthenticationSucceeded
TotpVerifyStart
TotpVerifySuccess
UserSessionEstablished
```

A password + Passkey sequence should resemble:

```text
PasswordAuthenticationStarted
PasswordAuthenticationSucceeded
PasskeyAuthentication...
PasskeyAuthentication...Success
UserSessionEstablished
```

A token-only flow should end in its appropriate token issuance audit events and should not emit `UserSessionEstablished` unless a real application session was also created.

## Test/Evidence Expectations

When the audit is performed, existing Aptix evidence should be used as an independent cross-check of the source trace.

For each interactive success path, tests should eventually be able to establish the invariant:

> Exactly one `UserSessionEstablished` event is observed after all required proof succeeds.

For rejected, incomplete-MFA, and token-only paths, tests should establish that no session-established event is emitted.

Do not update test event expectations mechanically. First determine the correct semantic event sequence for the path, then update the implementation and the evidence expectations together.

## Likely Findings To Look For

The audit should actively search for:

- Direct calls to `SignInAsync` outside the intended shared completion path
- Direct cookie/session creation in controllers or legacy managers
- Authentication managers that both verify proof and establish sessions without a clear boundary
- External-provider callbacks that bypass canonical authentication flow services
- Old endpoints still reachable but absent from the canonical auth model
- Shared completion methods emitting password-specific or provider-specific audit events
- Session establishment occurring before MFA challenge consumption
- Token flows accidentally creating interactive sessions
- Multiple session-established events for one logical login
- Success paths with no terminal session audit
- Different identity sources used before and after MFA
- Redirect-only success being mistaken for authenticated success
- Inconsistent user/org/actor values in audit records

## Exit Criteria

The login-path audit is complete when:

1. Every real authentication entry path has been traced to a terminal state.
2. Every path is mapped to the canonical auth model or explicitly identified as an intentional non-modeled infrastructure path.
3. No unexpected session-establishment side roads remain.
4. Proof events accurately describe proof mechanisms.
5. Every successful interactive login produces exactly one `UserSessionEstablished`.
6. Token-only and failed/incomplete flows produce no `UserSessionEstablished`.
7. MFA transaction and factor challenges have consistent validation/consumption semantics.
8. Aptix/test evidence agrees with the final code paths and event sequences.
9. Any legacy or duplicate paths have been removed, reconciled, or explicitly documented.
10. Category-to-Green and the code-driven audit describe the same authentication system from their two different perspectives.

## Status

Deferred intentionally until the remaining authentication methods and real-world runner coverage are further along.

This document is the starting point for that future audit. Do not treat the current path inventory as authoritative until the source-driven audit is actually performed.
