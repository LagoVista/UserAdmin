# Client AuthView Conformance Reconciliation

## Purpose

This playbook defines the manual process for inspecting a real authentication client and producing a machine-readable manifest of what that client actually implements.

The manifest is implementation evidence. It does not replace canonical AuthViews and it does not declare conformance by itself.

The intended flow is:

```text
canonical AuthViews in LagoVista/UserAdmin
  -> inspect one real client repository
  -> record observable implementation facts
  -> write one client manifest in auth-model/implementation/client-conformance
  -> Aptix compares manifest facts to canonical AuthViews
  -> implementation drift becomes visible
```

Use this playbook when reconciling Angular/web or React Native/mobile implementation status.

## Authority

Canonical presentation truth remains in:

- `auth-model/auth-views/`
- `auth-model/auth-routes/`
- related scenario/presentation definitions

The client repository is authoritative for what the client currently implements.

The generated client conformance manifest is a checked-in observation of that implementation at one specific Git commit.

Aptix is responsible for comparing the observation to the canonical model. The inspecting model must not silently change canonical AuthViews to match the client and must not mark authored progress complete merely because a manifest was generated.

## Output

Generate exactly one manifest for the inspected client:

```text
auth-model/implementation/client-conformance/angular-web.json
auth-model/implementation/client-conformance/react-native.json
```

Each manifest must validate against:

```text
auth-model/schemas/client-auth-view-conformance-manifest.schema.json
```

The manifest must record the exact inspected repository and commit SHA.

## Core rule: one canonical AuthView, one manifest entry

Read the complete active canonical AuthView inventory from `auth-model/auth-views/`.

For every canonical AuthView, emit exactly one `views[]` entry in the client manifest, even when the client implementation is missing, partial, unsupported, or unclear.

Do not omit difficult views. Do not emit extra invented AuthView IDs. Do not collapse several canonical AuthViews into one manifest entry.

The manifest should therefore form a 1:1 inventory projection of canonical AuthViews for that client.

Deprecated or retired AuthViews may be excluded from the required active inventory unless the reconciliation task explicitly asks to inspect legacy coverage.

## What the inspecting model must do

For each canonical AuthView:

1. Read the canonical AuthView definition.
2. Identify whether that AuthView is applicable to the inspected client platform.
3. Search the real client repository for the route, screen/component, semantic finders, actions, view-state behavior, navigation, and server operation associated with that view.
4. Follow imports, routing/navigation configuration, child components, hooks/services, generated proxies, and shared controls when needed to understand the actual implementation.
5. Record only facts supported by source in the inspected repository.
6. Include source paths sufficient for a human or later model to re-check the observation.
7. Emit the entry even when evidence is missing or ambiguous.

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

For `angular-web.json`, inspect the canonical AuthView `platforms.web` contract.

For `react-native.json`, inspect the canonical AuthView `platforms.mobile` contract. The React Native manifest represents the shared mobile implementation used by Android and iOS unless the code genuinely differs by platform. Platform-specific differences should be called out in source evidence or notes and should eventually be represented explicitly if they become materially different auth behavior.

Do not label an applicable but unimplemented view as `not-applicable`. Use `missing`.

## Commit freshness

`inspectedCommit` must be the exact 40-character Git SHA of the client revision that was inspected.

A manifest remains a truthful historical observation after the client changes, but Aptix should treat it as potentially stale when the inspected client commit is no longer current.

Do not update only `inspectedCommit` without re-inspecting the client.

## Recommended model prompt

A user should be able to hand a model this playbook and say, in substance:

```text
Follow auth-model/CLIENT-CONFORMANCE-RECONCILIATION.md.
Reconcile the Angular client at <repository> against the canonical AuthViews in LagoVista/UserAdmin and update auth-model/implementation/client-conformance/angular-web.json.
```

or:

```text
Follow auth-model/CLIENT-CONFORMANCE-RECONCILIATION.md.
Reconcile the React Native client at <repository> against the canonical AuthViews in LagoVista/UserAdmin and update auth-model/implementation/client-conformance/react-native.json.
```

The model should inspect current source rather than asking the user to manually enumerate views or components.

## Completion checklist for the inspecting model

Before committing a manifest, verify:

- the client repository and exact commit SHA are recorded
- every active canonical AuthView has exactly one entry
- there are no unknown or invented AuthView IDs
- platform applicability follows the canonical AuthView definition
- route/component values came from client source, not copied expectations
- view states came from observable implementation branches
- control/action finders came from real client source
- API operations were traced from the client
- missing implementation is represented as `missing`, not omitted
- ambiguity is represented as `unknown` or `partial`, not guessed away
- source evidence paths exist in the inspected client revision
- the manifest validates against the client conformance schema

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

The manifest is evidence. Canonical AuthViews remain the contract. The real client remains the implementation.
