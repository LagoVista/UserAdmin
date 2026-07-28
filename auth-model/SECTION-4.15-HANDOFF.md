# Section 4.15 Handoff: UI-Only Transitions and AuthView Coverage

## Status

Complete

## Objective

Catalog presentation-only movement independently from canonical authentication actions, and establish how AuthView, AuthViewField, AuthFieldAction, routes, view IDs, and stable semantic finders bind to the shared authentication model.

## Core decision

A screen change is not automatically an authentication transition.

Presentation bindings now declare one of two kinds:

- `canonical-action`: the UI invokes a canonical authentication action and may require server, web, Android, iOS, or VTM evidence.
- `ui-navigation`: the UI moves between views without changing canonical authentication state and therefore requires presentation evidence only.

UI-only bindings use a stable `auth.navigation.*` key instead of inventing a fake `auth.action.*` key.

## Initial UI-only coverage

The web authentication welcome view now has explicit bindings for:

- Continue with email
- Continue with a provider
- Continue with passkey

Each binding records:

- source route and view ID
- stable semantic finder
- component operation
- destination route and expected view ID
- capability-derived inputs where applicable

The passkey option is explicitly capability-gated by `window.PublicKeyCredential` availability.

## AuthView relationship

`AuthView`, `AuthViewField`, and `AuthFieldAction` remain runtime-maintained presentation catalogs. Git presentation bindings are the durable behavioral map that may reference those records through `authViewKey`, `authViewFieldId`, and `authFieldActionId` when stable IDs are available.

A binding may still use route, view ID, and semantic finder evidence while a runtime AuthView record has not yet been reconciled.

## Evidence rules

- UI-only navigation does not require server mutation evidence.
- Canonical-action bindings retain the scenario evidence requirements of their referenced actions and scenarios.
- Stable semantic finders are required for deterministic automation where a user-visible control exists.
- Expected routes and view IDs must be validated independently because route aliases and rendered screens may diverge.
- Missing mobile or VTM presentation bindings indicate uncovered presentation evidence, not missing canonical behavior.

## Implementation seams discovered

- The current web welcome screen performs `logout(false)` during initialization. That behavior is not modeled as part of the three navigation bindings and should be reviewed separately because it may mutate session state before a user chooses a path.
- Current AuthView child IDs are generated runtime IDs. Git bindings therefore use semantic finders until stable reconciled IDs are available.
- Existing presentation bindings for registration, email verification, and invitation review are now explicitly classified as `canonical-action` bindings.

## Completion criteria satisfied

- UI-only scenarios no longer require meaningless server evidence.
- Canonical-action and UI-navigation bindings are structurally distinct.
- Web routes, view IDs, and stable semantic finders are explicit for the first navigation family.
- AuthView, field, action, and Git binding responsibilities are separated.
- The model can expand platform-by-platform without redefining authentication behavior.

## Next area

Section 4.16: Test binding and evidence aggregation.
