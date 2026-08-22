# Client Interaction -> Green

## Purpose

This process defines how to take one Client Interaction or Auth Interaction from authored truth to a trustworthy end-to-end implementation across the Aptix agent server, shared agent/chat behavior, Angular Web, and React Native.

This process is intentionally narrower than `CATEGORY-TO-GREEN.md`.

For Client Interactions, the authoritative business operation performed by the interaction is assumed to already exist and behave correctly. This process proves the interaction definition, Client Directive server contract, client implementations, shared agent/chat wiring, and runtime round trip.

For Auth Interactions, the canonical authentication model remains authoritative for guards, transitions, state mutation, and server-side effects. This process verifies that the Auth Interaction binds correctly to that truth and participates correctly in the Client Interaction runtime.

## North star

A general Client Interaction is green when:

1. its authored definition is complete and valid;
2. its server-side Client Directive implementation conforms to the authored definition and is 5/5;
3. Angular Web is 3/3;
4. React Native is 3/3; and
5. current runtime evidence proves the complete round trip on every required platform.

An Auth Interaction has one additional requirement: its canonical Auth Bindings resolve and remain valid.

Backend implementation completeness for the business/auth operation invoked by the interaction is not part of this green score. The Client Directive transport, invocation, continuation, and result seam is part of the score because without it the interaction is not actually connected to the agent.

## Green dimensions

Track these dimensions independently:

- Definition
- Auth Bindings, for Auth Interactions only
- Server
- Angular Web
- React Native
- Runtime Evidence

A useful visual summary is:

`Definition -> [Auth Bindings] -> Server 5/5 -> Angular 3/3 -> React Native 3/3 -> Runtime`

For a general Client Interaction, omit Auth Bindings.

## Authoritative contracts

Client Interactions are authored in `LagoVista/UserAdmin/auth-model`.

The bounded interaction rules are defined by:

- `auth-model/CLIENT-CARD-CONTRACT.md`
- the appropriate Client Interaction/Auth Interaction JSON schema

The canonical server-side Client Directive architecture is defined by:

- `LagoVista/ai/ddrs/AGN-000040 - Agent Client Directive.md`

AGN-000040 owns the transport and runtime concepts used by Client Interactions:

- `AgentClientDirectiveDefinition` is the registered reusable server capability;
- `ClientDirective` is the canonical outbound wire instance;
- `ClientDirectiveResult` is the bounded inbound result;
- `AgentExecuteResponse.ClientDirectives` is the server-to-client transport;
- `AgentExecuteRequest.ClientDirectiveResults` is the dedicated continuation lane for result-bearing directives;
- `InvokeClientDirective` is the formal RequiresResult invocation path;
- `InvokeFireAndForgetDirective` is the formal FireAndForget invocation path;
- pending RequiresResult state is persisted on the originating `AgentSessionTurn`;
- arbitrary structured result payloads are prohibited.

The pre-AGN-000040 `AgentClientDirective` transport and model-text directive envelope no longer exist and MUST NOT be used as compatibility paths.

## Identity mapping

The authored Client Interaction model and AGN-000040 use slightly different names for the same concepts. The mapping is explicit:

- authored `directiveKey` -> server `AgentClientDirectiveDefinition.Action` -> outbound `ClientDirective.Action`
- authored interaction instance/correlation id -> outbound/inbound `DirectiveId`
- authored terminal `outcome` -> inbound `ClientDirectiveResult.Result`
- authored bounded response value -> exactly one allowed `ClientDirectiveResult` value form

Do not create a second parallel identity field on the wire. `DirectiveId` is the canonical issued-instance correlation identifier.

## Shared agent integration seam

Repository:

`nuviot/ai-client`

Canonical seam:

`agent-chat/AgentChatEngine`

Both stable client applications consume `AgentChatEngine` / `AgentChatViewModel`. Every completed agent response already passes through `AgentChatEngine.applyFinalResult()`.

Shared Client Interaction behavior therefore belongs here once, rather than being independently reimplemented by Angular and React Native.

Target shared responsibilities are:

1. inspect `AgentExecuteResponse.clientDirectives`;
2. project a supported directive into normalized Client Interaction view-model state;
3. expose the active/pending interaction through `AgentChatViewModel`;
4. accept one modeled terminal completion from the platform interaction host;
5. construct the canonical `ClientDirectiveResult` submission;
6. submit that result through `AgentExecuteRequest.clientDirectiveResults`;
7. apply the resumed agent response through the same `AgentChatEngine` pipeline; and
8. clear/advance pending interaction state only after the continuation is resolved.

Angular and React Native MUST NOT parse raw `AgentExecuteResponse.clientDirectives` independently or construct agent continuation requests independently.

## Platform responsibility

Angular Web and React Native own presentation and client-local side effects only.

Each platform:

1. observes normalized Client Interaction state from `AgentChatViewModel`;
2. passes it to its `ClientInteractionHost`;
3. renders the canonical interaction handler;
4. performs the interaction-specific client/backend operation;
5. keeps authoritative session/token/account results client-local; and
6. returns only the authored terminal outcome and any permitted bounded value to `AgentChatEngine`.

The UI layer does not decide how agent continuation is constructed.

## Evidence storage contract

Authored truth remains in:

- `auth-model/client-interactions/*.json`
- `auth-model/auth-interactions/*.json`

Implementation evidence is stored separately:

- Server: `auth-model/implementation/client-interaction-conformance/server.json`
- Angular Web: `auth-model/implementation/client-interaction-conformance/angular-web.json`
- React Native: `auth-model/implementation/client-interaction-conformance/react-native.json`

Schemas:

- Server: `auth-model/schemas/client-interaction-server-conformance-manifest.schema.json`
- Clients: `auth-model/schemas/client-interaction-conformance-manifest.schema.json`

These manifests record observed implementation facts. They are not authored product truth. Do not edit a Client Interaction definition merely to make an implementation observation green.

Aptix derives progress indicators by combining the authored definition, server evidence, client evidence, Auth Bindings where applicable, and runtime evidence.

## Server 5/5

The server dimension is evaluated using five independent evidence-backed checks.

### 1. Transport Exists

Green when the canonical AGN-000040 shared transport required by the interaction exists:

- `ClientDirective` outbound transport;
- `ClientDirectiveResult` bounded result transport when applicable;
- `AgentExecuteResponse.ClientDirectives`; and
- `AgentExecuteRequest.ClientDirectiveResults` for result-bearing interactions.

This is primarily shared infrastructure. Once proven, many interactions may inherit this check.

### 2. Definition Registered

Green when the interaction's authored `directiveKey` exists as a registered `AgentClientDirectiveDefinition.Action` and its source definition can be identified.

The definition must expose enough server metadata to reconcile invocation mode, allowed outcomes/results, bounded response type, payload contract, and optional deterministic handlers.

### 3. Invocation Wired

Green when the registered definition is enabled/reachable through the formal AGN-000040 invocation path appropriate to its response mode.

For RequiresResult interactions this means `InvokeClientDirective` can resolve and issue the directive.

For FireAndForget interactions this means `InvokeFireAndForgetDirective` can resolve, issue, and return `submitted` without creating a pending continuation.

A server-side code path that manually constructs a `ClientDirective` is useful legacy/precursor behavior but does not satisfy this check for an authored Client Interaction unless that direct production is the explicitly approved invocation contract.

### 4. Completion Wired

Green when the definition's declared completion semantics are fully wired.

For RequiresResult interactions this includes:

- pending directive state persisted on the originating `AgentSessionTurn`;
- the next continuation accepts the matching `ClientDirectiveResult`;
- `DirectiveId` and `Action` are correlated and validated;
- allowed result/value rules are validated;
- optional result handler executes deterministically;
- duplicate submission does not repeat side effects; and
- the normalized result is returned to the model continuation.

For FireAndForget interactions this check is green when the interaction correctly has no pending continuation/result obligation.

### 5. Contract Conforms

Green when the registered server definition and runtime contract match the authored interaction definition semantically:

- authored `directiveKey` matches server `Action`;
- invocation payload shape matches;
- response mode matches the authored interaction behavior;
- every server-accepted semantic result is authored;
- response-value allowance and shape match;
- Auth Interactions return no value; and
- server handling does not introduce hidden workflow semantics outside the authored bounded interaction.

Server is green only at 5/5.

## Client 3/3

Each required client is evaluated using exactly three checks.

### Exists

The normalized authored interaction can be dispatched by the platform `ClientInteractionHost` into the intended handler and has identifiable source evidence.

A presentational component by itself does not satisfy Exists. The platform host-to-handler dispatch path must exist.

### Controls Conform

Required controls, actions, semantic finders, control types, required/optional behavior, and sensitivity boundaries match the authored definition semantically.

Visual styling may differ across platforms.

### Behavior Wired

Behavior Wired is an end-to-end source-integration check from normalized shared interaction state through platform behavior and back to the shared continuation seam.

Confirm from source that:

1. `AgentChatEngine` projects the authored directive into normalized Client Interaction state;
2. the platform consuming surface observes that state and mounts `ClientInteractionHost`;
3. the host launches the intended handler;
4. authored actions invoke the intended client/service behavior;
5. terminal paths map to authored outcomes;
6. the original `DirectiveId` is preserved;
7. completion obeys the authored response-value/security contract;
8. `AgentChatEngine` submits the canonical `ClientDirectiveResult`; and
9. the resumed agent response returns through the normal `AgentChatEngine` pipeline.

The server's own 5/5 proof is not duplicated inside each client observation. Client Behavior Wired depends on that seam operationally but proves the client/shared-agent portion of the source path.

## Stable client implementation homes

### Angular Web

Repository: `softwarelogistics/nuviot-ui-shared`

Canonical root: `client-interactions/`

- `client-interactions/shared/` - reusable presentation primitives
- `client-interactions/handlers/` - one folder per interaction
- `client-interactions/client-interaction.types.ts` - protocol-facing client types
- `client-interactions/client-interactions.module.ts` - module exports
- `client-interactions/ClientInteractionHostComponent` - platform dispatcher

Reusable shell:

`client-interactions/shared/client-interaction-shell/ClientInteractionShellComponent`

### React Native

Repository: `nuviot/vtm-client`

Canonical root: `src/features/client-interactions/`

- `components/` - reusable presentation primitives
- `handlers/` - one folder per interaction
- `client-interaction-types.ts` - protocol-facing client types
- `index.ts` - feature exports
- `ClientInteractionHost` - platform dispatcher

Reusable presentation:

- `components/ClientInteractionShell`
- `components/ClientInteractionButton`

## Runtime evidence

Runtime evidence proves that the source wiring actually works.

For each required platform prove:

1. a real agent response contains the expected `ClientDirective` with a `DirectiveId` and authored action;
2. `AgentChatEngine` exposes the normalized interaction;
3. the platform mounts the intended handler;
4. the interaction is usable;
5. the expected client/service behavior occurs;
6. a modeled terminal outcome is produced;
7. the completion returns the original `DirectiveId`;
8. any permitted response value has the authored bounded shape;
9. Auth Interactions return no response value;
10. the continuation is submitted and resumes successfully for RequiresResult interactions; and
11. the interaction terminates/closes correctly.

Runtime evidence status remains:

- `not-run`
- `passed`
- `failed`

## Process

### 1. Author the interaction definition

Confirm one bounded purpose, stable interaction key, stable `directiveKey`, invocation payload contract, controls, actions, presentation states, terminal outcomes, response-value contract, and required platforms.

The interaction must satisfy `CLIENT-CARD-CONTRACT.md`.

### 2. Validate Auth Bindings

Auth Interactions only. Confirm every referenced Behavior, Scenario, Action, Transition, and AuthView resolves to current authored authentication truth.

### 3. Reconcile / implement Server 5/5

Inspect the current `LagoVista/Core` and `LagoVista/ai` default branches, record exact commit SHAs in `server.json`, and evaluate the five server checks using source evidence.

Implement gaps against AGN-000040 rather than adding interaction-specific transport shortcuts.

### 4. Reconcile Angular Web 3/3

Inspect the stable Angular repository and update `angular-web.json` with source-supported facts.

### 5. Reconcile React Native 3/3

Inspect the stable React Native repository and update `react-native.json` with source-supported facts.

### 6. Wire shared AgentChatEngine behavior

Implement directive projection and completion/resume once in `nuviot/ai-client`. Then reconcile each platform's Behavior Wired evidence from the complete source path.

### 7. Capture runtime evidence

Execute the complete interaction on every required platform and record current evidence references.

### 8. Determine final green state

A general Client Interaction is green when:

`Definition ✓   Server 5/5   Angular 3/3   React Native 3/3   Runtime ✓`

An Auth Interaction is green when:

`Definition ✓   Auth Bindings ✓   Server 5/5   Angular 3/3   React Native 3/3   Runtime ✓`

Unsupported or explicitly not-applicable platforms do not block green. Planned platforms do.

## Aptix visualization

The Client Interactions visualizer SHOULD project these dimensions directly rather than infer one opaque percentage.

General example:

`Definition ✓   Server 4/5   Angular 2/3   React Native 2/3   Runtime ○`

Auth example:

`Definition ✓   Auth Bindings ✓   Server 5/5   Angular 3/3   React Native 3/3   Runtime ✓`

The Server dimension expands to:

- Transport Exists
- Definition Registered
- Invocation Wired
- Completion Wired
- Contract Conforms

Each client expands to Exists, Controls Conform, Behavior Wired, inspected commit, source evidence, and runtime evidence.

Aptix is a projection. Authored JSON and evidence manifests remain authoritative.

## Reference specimen: Accept or Reject Terms and Conditions

Use `client.interaction.terms-and-conditions` as the reference specimen.

Authored definition:

`auth-model/client-interactions/accept-reject-terms-and-conditions.json`

The authored interaction requires:

- directive `client.terms-and-conditions`;
- authoritative current Terms and Conditions version display resolved by the client through normal APIs;
- View Terms, Accept, and Reject actions;
- terminal outcomes `accepted`, `rejected`, `canceled`, and `failed`;
- no response value.

Current server evidence is stored in:

`auth-model/implementation/client-interaction-conformance/server.json`

At the initial AGN-000040 reconciliation, Terms & Conditions is **Server 1/5**:

- Transport Exists: true
- Definition Registered: false
- Invocation Wired: false
- Completion Wired: false
- Contract Conforms: false

This is intentional and useful. The shared canonical transport exists, while the Terms-specific server capability and runtime path remain the next implementation target.

Angular and React Native already have their canonical interaction surfaces and substantial card-local behavior, but Behavior Wired remains false until the shared `AgentChatEngine` directive projection and continuation path are complete.

## How to use this specimen

When implementing a new Client Interaction:

1. author the definition;
2. create/update its Server observation and make Server 5/5 using AGN-000040;
3. create the handler in each required stable client home;
4. add host-to-handler dispatch;
5. update Angular and React Native observations;
6. expose Exists and Controls as soon as evidence supports them;
7. wire shared `AgentChatEngine` projection/completion behavior;
8. mark Behavior Wired only when the complete shared/client source path exists;
9. execute each required platform and attach runtime evidence; and
10. mark Runtime passed only from current evidence.

The Terms and Conditions interaction is the reference layout for authored truth, server directive implementation, shared agent behavior, platform handlers, conformance evidence, and runtime proof.