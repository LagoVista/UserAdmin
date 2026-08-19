# Client Response-Directed Authentication Routing

## Purpose

Some authentication actions call the server, but the provider-specific next screen is selected entirely by the client after inspecting the successful server response. These routes must not be modeled as separate server authentication transitions merely because they lead to different AuthViews.

Password Sign-In MFA handoff is the canonical example.

## Ownership boundary

The server owns authentication state and returns one canonical outcome when the password is accepted but another factor is still required:

- `auth.transition.password-sign-in.mfa-required`
- no authenticated application session is issued
- password proof has succeeded
- `availableMfaProviders` identifies every enrolled method that may satisfy the outstanding MFA requirement
- `provider` remains a backward-compatible single-provider hint for older clients

The client owns the presentation routing that follows that response:

```text
Password submit
  -> MFA Required
       -> availableMfaProviders=[totp]          -> auth.continue.totp
       -> availableMfaProviders=[passkey]       -> auth.continue.passkey
       -> availableMfaProviders=[passkey,totp]  -> auth.select.mfa-type
                                                   -> select-passkey -> auth.continue.passkey
                                                   -> select-totp    -> auth.continue.totp
```

Provider-specific routing does not create separate server transitions. The TOTP and Passkey destination AuthViews continue to belong to their respective sign-in categories. When multiple methods are available, `auth.select.mfa-type` is a presentation-only choice surface and selecting a method does not constitute an authentication state transition.

Clients MUST prefer `availableMfaProviders` when present. A one-element list routes directly to that method. A list with more than one supported method routes to `auth.select.mfa-type`. `provider` exists only for backward compatibility and MUST NOT be used to suppress an alternative method that appears in `availableMfaProviders`.

## Scenario modeling

A response-directed routing scenario still records the password submit action as server-backed because the action itself calls the server:

```json
"serverInteraction": {
  "required": true,
  "transitionKeys": ["auth.transition.password-sign-in.mfa-required"]
}
```

The provider-specific client obligation is represented by deterministic setup, scenario summary/intent, and `expectedViewKey`:

- TOTP-only setup -> `auth.continue.totp`
- passkey-only setup -> `auth.continue.passkey`
- TOTP + passkey setup -> `auth.select.mfa-type`

The scenario's `evidenceRequirements` determines which runtime proof surfaces are required. For provider-specific client routing, use only the client platforms:

```json
"evidenceRequirements": ["web", "android", "ios"]
```

Do not add `server` merely because the initiating action calls the server. The generic MFA-required server transition may have its own server proof, but the provider-specific navigation branch is not a separate C# flow proof obligation.

## Select MFA Type behavior category

`mfa-type-selection` owns the presentation-only decision after Password Sign-In discovers more than one available MFA method.

It contains exactly two behaviors, each composed of exactly one scenario:

- `auth.behavior.mfa-type-selection.passkey`
  - starts at `auth.select.mfa-type`
  - invokes `action:select-passkey`
  - ends at `auth.continue.passkey` (`Passkey Start`)
- `auth.behavior.mfa-type-selection.totp`
  - starts at `auth.select.mfa-type`
  - invokes `action:select-totp`
  - ends at `auth.continue.totp` (`TOTP Start`)

Both scenarios use `serverInteraction.required = false`. They only select which already-existing MFA ceremony to enter.

## Test-progress semantics

`progress.tests = complete` means the authored proof obligation is fully specified. It does not mean a C# test must exist for every scenario.

For response-directed client routing and pure UI selection, the test specification is complete when the required client runtime platforms and deterministic destination are declared. Current UI execution status remains separate runtime evidence.

## UI-only versus response-directed routing

Two related cases must remain distinct:

1. **Pure UI-only navigation**: no server call occurs. Use `serverInteraction.required = false`. Example: `auth.select.mfa-type -> auth.continue.passkey`.
2. **Response-directed client routing**: the user action calls the server, but response metadata selects the next AuthView. Keep `serverInteraction.required = true`, reference the generic server transition, and omit `server` from `evidenceRequirements` when only the client routing distinction is under proof.

This distinction prevents presentation routing from being inflated into fake authentication state transitions while preserving an executable end-to-end scenario contract.
