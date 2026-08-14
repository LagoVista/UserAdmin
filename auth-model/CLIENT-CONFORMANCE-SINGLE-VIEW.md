# Single AuthView Client Conformance Reconciliation

Use this playbook when reconciling one canonical AuthView on demand.

This is the focused form of `auth-model/CLIENT-CONFORMANCE-RECONCILIATION.md`. All observation, no-guessing, route-normalization, platform-applicability, source-evidence, and schema rules from that playbook still apply.

## Stable clients

- Angular Web: `softwarelogistics/nuviot-ui-shared`
- React Native: `nuviot/vtm-client`

## Input

The caller supplies exactly one canonical `viewId`.

Read that AuthView from `auth-model/auth-views/` and its canonical route from `auth-model/auth-routes/`.

## Procedure

For the supplied `viewId` only:

1. Inspect the current default branch of both stable client repositories.
2. For each applicable platform, trace the real route/navigation target, owning component/screen, View States, control finders, action finders, generated-client/API operations, and source evidence.
3. Record only facts supported by current client source. Do not guess to make the view conformant.
4. Rebuild the observation for this `viewId` in:
   - `auth-model/implementation/client-conformance/angular-web.json`
   - `auth-model/implementation/client-conformance/react-native.json`
5. Replace only the matching `views[]` entry in each applicable manifest. Preserve every other AuthView observation byte-for-byte in meaning and do not perform a full-library reconciliation.
6. If an applicable manifest has no entry for this `viewId`, add exactly one entry in the correct canonical position/order used by that manifest.
7. If the canonical platform contract says the view is not applicable to a client, keep or write the appropriate `not-applicable` observation according to the main playbook.
8. Update manifest-level inspection metadata only when required to truthfully describe the client revision used for this focused inspection. Do not claim that untouched entries were re-inspected at a newer commit.
9. Validate the complete resulting manifests against `auth-model/schemas/client-auth-view-conformance-manifest.schema.json`.
10. Commit only the manifest changes needed for this `viewId` unless a genuine reconciliation-contract defect is discovered and explicitly called out.

## Critical scope rule

Do **not** reconcile, rewrite, normalize, or clean up any other AuthView entry during a focused run, even if unrelated drift is noticed.

The purpose of this mode is to make one selected AuthView independently re-checkable without causing a sweeping manifest rewrite.

## Completion response

Report:

- reconciled `viewId`
- Angular result and concrete drift, if any
- React Native result and concrete drift, if any
- exact client commits inspected
- UserAdmin commit containing the manifest update
- whether both complete manifests still validate

If no manifest change is required because the selected entry already truthfully matches current source, say so explicitly and do not manufacture a commit.
