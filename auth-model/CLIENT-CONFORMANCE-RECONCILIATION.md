# Client AuthView Conformance Reconciliation

## Purpose

This playbook defines the manual process for inspecting the real authentication clients and producing machine-readable manifests of what those clients actually implement.

The manifests are implementation evidence. They do not replace canonical AuthViews and they do not declare conformance by themselves.

The intended flow is:

```text
canonical AuthViews in LagoVista/UserAdmin
  -> inspect both stable client repositories
  -> record observable implementation facts
  -> write both client manifests in auth-model/implementation/client-conformance
  -> Aptix compares manifest facts to canonical AuthViews
  -> implementation drift becomes visible
```

Use this playbook when reconciling client implementation status.

## Stable client repositories

The authentication client repositories are stable project configuration:

```text
Angular Web:  softwarelogistics/nuviot-ui-shared
React Native: nuviot/vtm-client
```

Do not ask the user to supply these repositories during normal reconciliation. Do not substitute another client repository unless the project explicitly changes this contract.

A normal reconciliation run inspects **both** repositories and regenerates **both** manifests in one pass.

## Authority

Canonical presentation truth remains in:

- `auth-model/auth-views/`
- `auth-model/auth-routes/`
- related scenario/presentation definitions

Each client repository is authoritative for what that client currently implements.

The generated client conformance manifests are checked-in observations of those implementations at specific Git commits.

Aptix is responsible for comparing the observations to the canonical model. The inspecting model must not silently change canonical AuthViews to match a client and must not mark authored progress complete merely because manifests were generated.

## Output

Generate both manifests on every normal reconciliation run:

```text
auth-model/implementation/client-conformance/angular-web.json
auth-model/implementation/client-conformance/react-native.json
```

Each manifest must validate against:

```text
auth-model/schemas/client-auth-view-conformance-manifest.schema.json
```

Each manifest must record the exact inspected repository and commit SHA.

## Core rule: one canonical AuthView, one manifest entry per client

Read the complete active canonical AuthView inventory from `auth-model/auth-views/`.

For every canonical AuthView, emit exactly one `views[]` entry in each applicable client manifest, even when the client implementation is missing, partial, unsupported, or unclear.

Do not omit difficult views. Do not emit extra invented AuthView IDs. Do not collapse several canonical AuthViews into one manifest entry.

Each manifest should therefore form a 1:1 inventory projection of canonical AuthViews for that client.

Deprecated or retired AuthViews may be excluded from the required active inventory unless the reconciliation task explicitly asks to inspect legacy coverage.

## What the inspecting model must do

Perform this process for **Angular Web first, then React Native**, using the stable repositories above.

For each canonical AuthView and each applicable client:

1. Read the canonical AuthView definition.
2. Identify whether that AuthView is applicable to the inspected client platform.
3. Search the real client repository for the route, screen/component, semantic finders, actions, view-state behavior, navigation, and server operation associated with that view.
4. Follow imports, routing/navigation configuration, child components, hooks/services, generated proxies, and shared controls when needed to understand the actual implementation.
5. Record only facts supported by source in the inspected repository.
6. Include source paths sufficient for a human or later model to re-check the observation.
7. Emit the entry even when evidence is missing or ambiguous.
8. Record the exact 40-character commit SHA inspected for that client.
9. Write/update the corresponding manifest in `LagoVista/UserAdmin`.
10. Validate both completed manifests against the client conformance schema before finishing.

This is repository inspection, not visual inference. Source code is the evidence source.

## Observation fields

For each AuthView record:

### `viewId`

Copy the canonical AuthView `viewId` exactly.

### `status`

Use:

- `implemented` when a concrete client implementation was found and the required implementation facts can be observed.
- `partial` when some meaningful portion exists but one or more expected pieces are absent or unresolved.
- `missing` when the view is applicable but no implementation can be found after a reasonable repository search.
- `unknown` when available source is insufficient to determine the implementation accurately.
- `not-applicable` only when the canonical platform contract explicitly makes the view inapplicable to this client.

Status is an observation aid, not the final conformance verdict. Aptix may still report drift for an `implemented` entry when its observed facts differ from the canonical AuthView.

### `route`

Record the actual route, navigation key, or equivalent client destination found in code.

Use `null` when there is no routable destination or the route cannot be determined.

Do not copy the canonical route merely because it is expected.

### `component`

Record the concrete Angular component or React Native screen/component that owns the AuthView.

Use `null` when missing or unknown.

### `viewStates`

Record the canonical-style View State tokens that are actually represented by the client implementation.

Do not infer a state merely because the canonical AuthView defines it. There must be an observable client branch, state variable, selector, navigation state, or equivalent implementation evidence.

### `controlFinders`

Record semantic control finders that are actually exposed by the client implementation.

Only record canonical-format finders that can be traced to real source.

### `actionFinders`

Record semantic action finders that are actually exposed by the client implementation.

### `apiOperations`

Record the generated proxy/client operation names or other concrete server operation calls used by this AuthView.

For a client-only action, this array may be empty.

Do not infer an API operation from server-side definitions. Trace the call from the client implementation.

### `sourceEvidence`

Provide the relevant source file path for every implementation observation. Add a `symbol` when a component, method, hook, service, route constant, or function name makes the evidence easier to locate.

Use several evidence entries when a view is assembled across routing, UI, and service files.

### `notes`

Use notes for concrete uncertainties, aliases, shared-component behavior, or implementation details that Aptix cannot determine from the structured fields.

Do not use notes to hide missing structured evidence.

## No guessing rule

Never manufacture implementation facts to make a manifest conformant.

If a canonical finder is expected but cannot be found, leave it out of the observed finder list and explain the search result in `notes` when useful.

If two client surfaces might correspond to one AuthView and ownership cannot be determined, use `unknown` or `partial` and document the ambiguity.

If the client appears semantically correct but uses no canonical finder, record the actual absence. Aptix should surface that mismatch.

The manifest describes **what exists**, not what should exist.

## Shared components

A single physical client component may implement several canonical AuthViews or View States. That is allowed.

Still emit one manifest record per canonical AuthView. Reuse the same `component` and source evidence when appropriate, while recording the route, states, controls, actions, and API operations applicable to that specific canonical view.

## Platform applicability

For `angular-web.json`, inspect the canonical AuthView `platforms.web` contract against `softwarelogistics/nuviot-ui-shared`.

For `react-native.json`, inspect the canonical AuthView `platforms.mobile` contract against `nuviot/vtm-client`. The React Native manifest represents the shared mobile implementation used by Android and iOS unless the code genuinely differs by platform. Platform-specific differences should be called out in source evidence or notes and should eventually be represented explicitly if they become materially different auth behavior.

Do not label an applicable but unimplemented view as `not-applicable`. Use `missing`.

## Commit freshness

`inspectedCommit` must be the exact 40-character Git SHA of the client revision that was inspected.

A manifest remains a truthful historical observation after the client changes, but Aptix should treat it as potentially stale when the inspected client commit is no longer current.

Do not update only `inspectedCommit` without re-inspecting the client.

## Recommended model prompt

The normal prompt is intentionally short because this playbook contains the stable repositories and complete process:

```text
Follow auth-model/CLIENT-CONFORMANCE-RECONCILIATION.md in LagoVista/UserAdmin. Reconcile both stable authentication client repositories against every active canonical AuthView, regenerate both client conformance manifests, validate them, and commit the results to LagoVista/UserAdmin. Do not change client source code or canonical AuthViews during this reconciliation pass.
```

The model should inspect current source rather than asking the user to manually enumerate views, components, or repositories.

## Completion checklist for the inspecting model

Before committing the manifests, verify:

- both stable client repositories were inspected
- each client repository and exact commit SHA are recorded
- every active canonical AuthView has exactly one entry in each applicable manifest
- there are no unknown or invented AuthView IDs
- platform applicability follows the canonical AuthView definition
- route/component values came from client source, not copied expectations
- view states came from observable implementation branches
- control/action finders came from real client source
- API operations were traced from the client
- missing implementation is represented as `missing`, not omitted
- ambiguity is represented as `unknown` or `partial`, not guessed away
- source evidence paths exist in the inspected client revision
- both manifests validate against the client conformance schema
- only the two manifest files are changed unless a genuine reconciliation-contract defect is discovered and explicitly called out

## Aptix responsibility

Aptix should perform mechanical comparison only. At minimum it can validate:

- manifest contains exactly one entry for every applicable active canonical AuthView
- no manifest entry references an unknown AuthView
- expected route matches observed route where route conformance is required
- expected View States are present
- expected control finders are present
- expected action finders are present
- canonical platform support agrees with manifest applicability/status
- source evidence exists for claimed implementation
- manifest freshness can be evaluated from `inspectedCommit` when current client revisions are available

Aptix should report drift rather than rewriting either the canonical AuthView or client manifest.

The manifests are evidence. Canonical AuthViews remain the contract. The real clients remain the implementation.
