# Client Interaction -> Green

## Purpose

This process defines how to take one Client Interaction or Auth Interaction from an authored definition to a trustworthy client implementation across Angular Web and React Native.

This process is intentionally narrower than `CATEGORY-TO-GREEN.md`.

For Client Interactions, the authoritative backend operation performed by the card is assumed to already exist and behave correctly. The purpose of this process is to prove that the interaction definition is complete, both clients render the authored surface, the shared agent/chat behavior actually routes the interaction, and runtime evidence proves the round trip.

For Auth Interactions, the existing canonical authentication model remains authoritative for authentication behavior, guards, transitions, and server-side effects. This process verifies only that the Auth Interaction is correctly bound to that existing truth and correctly integrated into the shared Client Interaction runtime.

## North star

A Client Interaction is green when:

1. its authored definition is complete and valid;
2. Angular Web implements the interaction and conforms to the authored contract;
3. React Native implements the interaction and conforms to the authored contract;
4. the shared agent/chat behavior routes the directive into the correct interaction and routes its terminal completion back into the agent continuation path; and
5. current runtime evidence proves that complete round trip.

An Auth Interaction has one additional requirement: its canonical authentication bindings resolve and remain valid.

Backend implementation completeness for the business/auth operation invoked by the card is not part of the Client Interaction green score. The backend/client-directive transport and continuation seam is part of the green score because without it the card is not actually connected to the agent.

## Green dimensions

Track these dimensions independently:

- Definition
- Auth Bindings, for Auth Interactions only
- Angular Web
- React Native
- Runtime Evidence

Each supported client is evaluated using exactly three conformance checks:

- Exists
- Controls Conform
- Behavior Wired

A useful visual summary is:

`Definition -> Auth Bindings -> Angular 3/3 -> React Native 3/3 -> Runtime`

For a general Client Interaction, omit Auth Bindings.

## Shared agent integration seam

Client Interactions have one shared behavioral seam for Angular Web and React Native.

### Backend source of directives

The canonical agent response contract is owned in `LagoVista/Core`:

`src/LagoVista.Core/AI/Models/AgentExecuteResponse.cs`

`AgentExecuteResponse` already contains:

`ClientDirectives : List<AgentClientDirective>`

The legacy `AgentClientDirective` shape currently contains:

- `action`
- `args`
- `payload`
- `message`

It does **not** currently contain a dedicated interaction/correlation identifier.

The `LagoVista/AI` pipeline already owns collection and delivery of these directives:

- `src/LagoVista.AI/Models/AgentPipelineContext.cs`
  - `AddClientDirective(...)` collects directives during an agent turn.
- `src/LagoVista.AI/Helpers/AgentExecuteResponseBuilder.cs`
  - copies `ctx.ClientDirectives` into `AgentExecuteResponse.ClientDirectives`.
- server-side tools may add directives directly. `EaCreateVtmMeetingTool` is a concrete current example which emits `enter_vtm_meeting` plus `vtmMeetingId` arguments.

Therefore Client Interaction work must build on `AgentExecuteResponse.ClientDirectives`; it must not introduce a second unrelated server-to-client signaling channel.

### Shared client behavior

Repository:

`nuviot/ai-client`

Canonical seam:

`agent-chat/AgentChatEngine`

Both stable client applications consume `AgentChatEngine`/`AgentChatViewModel`. Every completed agent response already passes through `AgentChatEngine.applyFinalResult()`.

The shared behavior must therefore live here once, rather than being independently implemented in Angular and React Native.

The target responsibility is:

1. inspect `AgentExecuteResponse.clientDirectives` when a final agent response is applied;
2. normalize supported directives into explicit Client Interaction view-model state;
3. expose the active/pending interaction through `AgentChatViewModel`;
4. allow the platform UI to submit one modeled terminal completion;
5. clear/advance the pending interaction state;
6. submit the completion through the canonical agent continuation contract; and
7. apply the resumed agent response through the same `AgentChatEngine` pipeline.

Angular and React Native must not parse raw `AgentExecuteResponse.clientDirectives` independently.

### Platform responsibility

Angular Web and React Native remain responsible for presentation and client-local side effects only.

Each platform:

1. observes the normalized Client Interaction state exposed by `AgentChatViewModel`;
2. passes the directive to its `ClientInteractionHost`;
3. renders the canonical interaction handler;
4. performs the interaction-specific client/backend operation;
5. keeps authoritative session/token/account results client-local; and
6. returns only the modeled Client Interaction completion to the shared `AgentChatEngine`.

The UI layer does not decide how an agent continuation is constructed.

## Directive identity and completion contract

The existing Client Directive mechanism predates this formal Client Interaction model. It has an `action` but no dedicated identifier for one concrete interaction instance.

Neighboring agent contracts already demonstrate the distinction we need:

- client tool calls use `toolCallId`;
- ACP intents/calls use `intentId` / `interactionId`.

For Client Interactions, use these two identities distinctly:

- **directiveKey** identifies the reusable authored interaction type, for example `client.terms-and-conditions`;
- **interactionId** identifies one concrete issued interaction instance and is returned unchanged with its completion.

`interactionId` is the preferred formal term for this model. Do not overload the agent pipeline's internal `CorrelationId`, session id, or turn id for this purpose.

The current legacy `AgentClientDirective.action` is the historical directive selector. As interactions are reconciled, authored `directiveKey` is the canonical selector. Legacy actions may require an explicit compatibility mapping while they are migrated.

### Target outbound shape

The formal Client Interaction transport should provide, at minimum:

- `interactionId`
- `directiveKey`
- optional bounded invocation payload
- optional user-facing message/hints when genuinely needed

Do not send credentials, secrets, authoritative account state, or arbitrary form-shaped payloads in the directive.

### Target completion shape

A general Client Interaction completion contains:

- `interactionId`
- authored terminal `outcome`
- optional bounded response value only when the authored response contract permits it

An Auth Interaction completion contains only:

- `interactionId`
- authored terminal `outcome`

Auth Interactions never return a response value.

### Current backend gap

`AgentExecuteRequest` currently supports user-turn requests and client-tool continuation submissions. It does not currently expose a Client Interaction/Client Directive completion submission field.

An `AcpResultSubmission` model exists in the request source, but Client Interaction completion is not currently part of `AgentExecuteRequest` validation/routing.

Until a canonical Client Interaction completion/continuation contract exists and `AgentChatEngine` uses it, **Behavior Wired cannot be green**, even when an individual card component can perform its backend operation correctly.

This is an intentional distinction between "the card works if manually invoked" and "the card is wired into agent behavior."

## Evidence storage contract

Authored interaction definitions remain in:

- `auth-model/client-interactions/*.json`
- `auth-model/auth-interactions/*.json`

Current client implementation observations are stored separately from authored interaction truth:

- Angular Web: `auth-model/implementation/client-interaction-conformance/angular-web.json`
- React Native: `auth-model/implementation/client-interaction-conformance/react-native.json`

Both manifests validate against:

- `auth-model/schemas/client-interaction-conformance-manifest.schema.json`

Each manifest records the exact inspected repository and commit SHA, one observation per reconciled interaction, the three client checks, runtime-evidence status, component/directive-handler identity when found, observed semantic finders, source evidence, and concrete notes explaining gaps or drift.

These manifests are implementation evidence, not authored product truth. Do not edit the interaction definition merely to make a client observation green.

Aptix derives progress indicators from the interaction definition plus these evidence manifests.

## Stable client implementation homes

Client Interaction implementations have dedicated homes in both stable clients. Do not scatter new handlers through route-specific auth folders or application-specific chat surfaces.

### Angular Web

Repository:

`softwarelogistics/nuviot-ui-shared`

Canonical feature root:

`client-interactions/`

Structure:

- `client-interactions/shared/` - reusable Client Interaction presentation primitives;
- `client-interactions/handlers/` - one folder per canonical interaction handler;
- `client-interactions/client-interaction.types.ts` - shared protocol-facing client types;
- `client-interactions/client-interactions.module.ts` - Angular module exporting the shared shell and handlers.

The reusable shell is:

`client-interactions/shared/client-interaction-shell/ClientInteractionShellComponent`

The shell owns the common card hierarchy and CSS custom properties prefixed with `--nuv-client-interaction-`. Individual handlers should use the shell rather than inventing their own card layout and action styling.

The platform dispatcher is:

`client-interactions/ClientInteractionHostComponent`

It maps normalized Client Interaction state to the authored handler. It is not responsible for parsing raw agent responses or constructing agent continuation requests.

### React Native

Repository:

`nuviot/vtm-client`

Canonical feature root:

`src/features/client-interactions/`

Structure:

- `src/features/client-interactions/components/` - reusable Client Interaction presentation primitives;
- `src/features/client-interactions/handlers/` - one folder per canonical interaction handler;
- `src/features/client-interactions/client-interaction-types.ts` - shared protocol-facing client types;
- `src/features/client-interactions/index.ts` - feature exports.

The reusable shell is:

`src/features/client-interactions/components/ClientInteractionShell`

Shared action styling is provided by:

`src/features/client-interactions/components/ClientInteractionButton`

The platform dispatcher is:

`src/features/client-interactions/ClientInteractionHost`

React Native uses the existing `AppTheme` tokens rather than a second Client Interaction color system.

### Shared visual vocabulary

Angular and React Native do not share rendering code, but every interaction should preserve the same semantic visual hierarchy:

- eyebrow;
- title;
- summary;
- body;
- status/error region;
- primary action;
- secondary action.

Platform-specific presentation differences are allowed. The cards should nevertheless look and behave like members of the same product family.

## Status values

The three client checks are evidence-backed booleans. Runtime evidence uses:

- `not-run`
- `passed`
- `failed`

A client is green when Exists, Controls Conform, and Behavior Wired are all true.

An interaction is fully green when every required client is green, required runtime evidence passes, and Auth Bindings are valid when applicable.

If implementation exists but does not match authored truth, treat it as drift/in-progress rather than redefining the interaction to match the implementation.

## 1. Author the interaction definition

Confirm that the interaction represents exactly one bounded client capability.

The definition must establish a stable interaction key, stable ClientDirective key, one bounded purpose, invocation payload contract, controls, actions, allowed local presentation states, terminal outcomes, response-value contract, and supported client platforms.

The interaction must satisfy `CLIENT-CARD-CONTRACT.md`. In particular:

- one ClientDirective maps to one interaction;
- each issued interaction has one stable `interactionId` for its round trip;
- the interaction does not navigate directly to another interaction;
- arbitrary structured response payloads are prohibited;
- response values are limited to the contract-approved scalar, EntityHeader, or homogeneous multi-select shapes;
- Auth Interactions return no response value of any kind.

Definition is complete when the JSON validates against the correct schema, required fields are present, response rules are explicit, supported platforms are declared honestly, and no unresolved authored ambiguity remains about what the client must render or return.

## 2. Validate Auth bindings

This phase applies only to Auth Interactions.

Confirm that every declared canonical authentication reference resolves to current authored authentication truth. Bindings may include Behaviors, Scenarios, Actions, Transitions, and AuthViews where presentation semantics are intentionally reused.

The Auth Interaction must not invent authentication guards, transitions, postconditions, or authoritative account state.

## 3. Reconcile Angular Web

Inspect the current stable Angular implementation repository and record its exact commit SHA in `implementation/client-interaction-conformance/angular-web.json`.

Evaluate exactly three checks.

### Exists

Confirm that the normalized authored Client Interaction can be dispatched by the platform `ClientInteractionHost` into the intended interaction component/card and has identifiable source evidence.

A presentational component by itself does not satisfy Exists. The platform host-to-handler dispatch path must be wired.

Exists does not require the shared `AgentChatEngine` seam to be complete; that requirement belongs to Behavior Wired.

### Controls Conform

Compare the rendered interaction to the authored definition. Required controls, actions, semantic finders, control types, required/optional behavior, and sensitivity boundaries must match semantically.

Visual styling does not need to be identical across platforms.

### Behavior Wired

Behavior Wired is intentionally an end-to-end **source integration** check, not merely a card-internal check.

Confirm from source that:

1. the backend response can carry the authored directive;
2. `AgentChatEngine` projects that directive into normalized Client Interaction view-model state;
3. the Angular consuming surface observes that state and mounts `ClientInteractionHost`;
4. the host launches the intended handler;
5. authored actions invoke the intended client/service behavior;
6. terminal paths map to authored outcomes;
7. the same `interactionId` is returned;
8. the completion obeys the authored response-value/security contract; and
9. `AgentChatEngine` submits the completion through the canonical continuation path and applies the resumed agent response.

For this process, do not re-prove the backend business/auth operation behind the card action.

Angular is green when all three checks are true.

## 4. Reconcile React Native

Inspect the current stable React Native implementation repository and record its exact commit SHA in `implementation/client-interaction-conformance/react-native.json`.

Apply the same three checks:

- Exists
- Controls Conform
- Behavior Wired

A presentational component by itself does not satisfy Exists. The platform host-to-handler dispatch path must be wired.

Behavior Wired requires the same shared `AgentChatEngine` directive projection and continuation path used by Angular. Do not create a separate RN protocol.

The semantic interaction contract must match Angular even when native presentation differs.

React Native is green when all three checks are true.

## 5. Runtime evidence

Runtime evidence proves that the source wiring described above actually works when executed.

For each required platform prove:

1. a real agent response contains the expected Client Interaction directive;
2. `AgentChatEngine` exposes the normalized interaction;
3. the platform mounts the intended interaction;
4. the interaction is usable;
5. the expected client/service behavior is invoked;
6. a modeled terminal outcome is produced;
7. the completion returns the original `interactionId`;
8. the agent continuation is submitted and resumes successfully;
9. the interaction terminates/closes;
10. any allowed response value has the authored shape; and
11. Auth Interactions return no value.

Record runtime status and evidence references on that platform's conformance observation.

Do not duplicate existing backend business-operation integration evidence unless a failure suggests the backend assumption is invalid.

## 6. Determine final green state

A general Client Interaction is green when Definition is valid, every required client is 3/3, and required runtime evidence passes.

An Auth Interaction additionally requires valid canonical Auth Bindings.

Unsupported or explicitly not-applicable platforms do not block green. Planned platforms do block full green until implemented or intentionally reclassified.

## Aptix visualization

The Client Interactions visualizer should project the evidence as clear progress indicators.

General interaction example:

`Definition ✓   Angular 2/3   React Native 2/3   Runtime ○`

Auth interaction example:

`Definition ✓   Auth Bindings ✓   Angular 3/3   React Native 3/3   Runtime ✓`

Each client expands to show Exists, Controls, Behavior, inspected commit, source evidence, and runtime evidence.

Aptix is a projection. The JSON definitions and evidence manifests remain authoritative.

## Reference specimen: Accept or Reject Terms and Conditions

Use `client.interaction.terms-and-conditions` as the reference specimen for this process.

### Authored definition

`auth-model/client-interactions/accept-reject-terms-and-conditions.json`

The authored interaction requires:

- directive `client.terms-and-conditions`;
- current authoritative Terms and Conditions version display;
- canonical View Terms action;
- explicit Accept action;
- explicit Reject action;
- terminal outcomes `accepted`, `rejected`, `canceled`, and `failed`;
- no response value.

### Angular reference location

Stable repository:

`softwarelogistics/nuviot-ui-shared`

Platform dispatcher:

`client-interactions/ClientInteractionHostComponent`

Reference handler:

`client-interactions/handlers/terms-and-conditions/TermsAndConditionsInteractionComponent`

Shared shell:

`client-interactions/shared/client-interaction-shell/ClientInteractionShellComponent`

Current reference state:

- Exists: true;
- Controls Conform: true;
- Behavior Wired: false;
- Runtime: not-run.

The platform dispatcher recognizes `client.terms-and-conditions`; the handler fetches the authoritative version, opens the canonical Termly document, performs the existing promotion/acceptance operation, and maps local Accept/Reject/Fail paths.

Behavior remains false because the shared `AgentChatEngine` does not yet project `AgentExecuteResponse.clientDirectives` into Client Interaction view-model state and there is no canonical Client Interaction completion submission/continuation contract yet.

The older `softwarelogistics/workforce-ui` `PromotionDirectiveComponent` remains useful precursor evidence for the real promotion/T&C backend path, but it is no longer the canonical Client Interaction implementation home.

### React Native reference location

Stable repository:

`nuviot/vtm-client`

Platform dispatcher:

`src/features/client-interactions/ClientInteractionHost`

Reference handler:

`src/features/client-interactions/handlers/terms-and-conditions/TermsAndConditionsInteraction`

Shared presentation:

- `src/features/client-interactions/components/ClientInteractionShell`;
- `src/features/client-interactions/components/ClientInteractionButton`.

Current reference state:

- Exists: true;
- Controls Conform: true;
- Behavior Wired: false;
- Runtime: not-run.

The same shared-agent seam blocks Behavior Wired: RN must consume normalized Client Interaction state from `AgentChatEngine`, not parse the raw backend directive itself, and its completion must return through that same shared continuation path.

### Current shared seam state

Already present:

- `LagoVista/Core` response contract carries `clientDirectives`;
- `LagoVista/AI` pipeline collects Client Directives;
- `LagoVista/AI` response builder publishes Client Directives;
- `nuviot/ai-client` has the shared `AgentChatEngine` through which all final responses already pass;
- Angular and RN have canonical Client Interaction hosts and the T&C card surface/backend action.

Still required:

- formal `interactionId` on issued Client Interactions;
- canonical `directiveKey` transport/mapping from the legacy directive action shape;
- `AgentChatEngine` projection of pending Client Interactions;
- Client Interaction completion submission contract on the agent request/continuation path;
- `AgentChatEngine` completion/resume behavior;
- Angular and RN consuming surfaces mounting their Client Interaction hosts from the shared view model;
- runtime evidence on both platforms.

### How to use this specimen

When implementing a new Client Interaction:

1. author the definition;
2. create the handler in the stable client home for each required platform using the shared shell;
3. add host-to-handler dispatch in each platform;
4. add or update its Angular observation;
5. add or update its React Native observation;
6. let Aptix expose Exists and Controls as soon as evidence supports them;
7. wire the directive and completion through the shared `AgentChatEngine` seam;
8. mark Behavior Wired only when the complete source path exists;
9. execute each client and attach runtime evidence;
10. mark Runtime passed only from current evidence.

This Terms and Conditions interaction is the template for where definitions, backend directive transport, shared agent behavior, client handlers, shared presentation, client observations, source evidence, and runtime evidence belong. Future Client and Auth Interactions should follow the same layout.
