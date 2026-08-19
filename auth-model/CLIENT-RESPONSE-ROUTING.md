# Client Response-Directed Authentication Routing

## Purpose

Some authentication actions call the server, but the provider-specific next screen is selected entirely by the client after inspecting the successful server response. These routes must not be modeled as separate server authentication transitions merely because they lead to different AuthViews.

Password Sign-In MFA handoff is the canonical example.

## Ownership boundary

The server owns authentication state and returns one canonical outcome when the password is accepted but another factor is still required:

- `auth.transition.password-sign-in.mfa-required`
- no authenticated application session is issued
- password proof has succeeded
- response metadata identifies the required provider

The client owns the presentation routing that follows that response:

```text
Password submit
  -> MFA Required
       -> provider=totp    -> auth.continue.totp
       -> provider=passkey -> auth.continue.passkey
```

`provider=totp` and `provider=passkey` do not create separate server transitions. The destination AuthViews already belong to the TOTP Sign In and Passkey Sign In categories respectively.

## Scenario modeling

A response-directed routing scenario still records the password submit action as server-backed because the action itself calls the server:

```json
"serverInteraction": {
  "required": true,
  "transitionKeys": ["auth.transition.password-sign-in.mfa-required"]
}
```

The provider-specific client obligation is represented by deterministic setup, scenario summary/intent, and `expectedViewKey`:

- TOTP-enabled setup + `provider=totp` -> `auth.continue.totp`
- passkey-required setup + `provider=passkey` -> `auth.continue.passkey`

The scenario's `evidenceRequirements` determines which runtime proof surfaces are required. For provider-specific client routing, use only the client platforms:

```json
"evidenceRequirements": ["web", "android", "ios"]
```

Do not add `server` merely because the initiating action calls the server. The generic MFA-required server transition may have its own server proof, but the provider-specific navigation branch is not a separate C# flow proof obligation.

## Test-progress semantics

`progress.tests = complete` means the authored proof obligation is fully specified. It does not mean a C# test must exist for every scenario.

For response-directed client routing, the test specification is complete when the required client runtime platforms and deterministic destination are declared. Current UI execution status remains separate runtime evidence.

## UI-only versus response-directed routing

Two related cases must remain distinct:

1. **Pure UI-only navigation**: no server call occurs. Use `serverInteraction.required = false`. Example: Welcome -> Continue with Passkey.
2. **Response-directed client routing**: the user action calls the server, but a response field selects the next AuthView. Keep `serverInteraction.required = true`, reference the generic server transition, and omit `server` from `evidenceRequirements` when only the client routing distinction is under proof.

This distinction prevents presentation routing from being inflated into fake authentication state transitions while preserving an executable end-to-end scenario contract.
