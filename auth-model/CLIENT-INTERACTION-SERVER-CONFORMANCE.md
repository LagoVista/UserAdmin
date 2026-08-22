# Client Interaction Server Conformance

## Purpose

This playbook extends `CLIENT-INTERACTION-TO-GREEN.md` with a first-class server implementation dimension for Client Interactions and Auth Interactions.

The authored interaction definition remains product truth. Server conformance records implementation evidence showing whether the Aptix Agent runtime can actually issue that authored interaction and, when required, accept its bounded completion and resume the model.

This playbook does not re-prove the backend business or authentication operation performed by the interaction. Those operations remain owned by their existing canonical implementation and evidence.

## Relationship to AGN-000040

The normative Agent Client Directive runtime contract is `LagoVista/ai/ddrs/AGN-000040 - Agent Client Directive.md`.

Server reconciliation MUST evaluate implementation against both:

- the authored Client Interaction definition in `auth-model/client-interactions/*.json` or `auth-model/auth-interactions/*.json`; and
- AGN-000040.

The authored interaction determines the directive key, bounded invocation payload, terminal outcomes, and response-value contract. AGN-000040 determines how those semantics are registered, invoked, transported, persisted, validated, and returned to the model.

## Green dimensions

Client Interaction progress should be visualized as:

`Definition -> [Auth Bindings] -> Server 5/5 -> Angular 3/3 -> React Native 3/3 -> Runtime`

The Server dimension contains exactly five evidence-backed checks:

1. Transport Exists
2. Definition Registered
3. Invocation Wired
4. Completion Wired
5. Contract Conforms

Server is green only when all five checks are true.

## Evidence storage

Server observations are stored separately from authored interaction truth and separately from client implementation evidence:

`auth-model/implementation/client-interaction-conformance/server.json`

The manifest validates against:

`auth-model/schemas/client-interaction-server-conformance-manifest.schema.json`

Because the server contract spans `LagoVista/Core` and `LagoVista/ai`, the manifest records every inspected repository and exact commit SHA.

Do not change an authored interaction merely to make server implementation evidence green. A mismatch is drift until the authored truth or implementation is intentionally changed through its normal process.

## 1. Transport Exists

Transport Exists proves that the shared AGN-000040 transport needed by this interaction exists in the current server code.

For a result-bearing interaction, confirm from source that:

- `AgentExecuteResponse.ClientDirectives` carries canonical `ClientDirective` values;
- each issued directive has a stable `DirectiveId` and `Action`;
- result-bearing handoff is identified by `AgentExecuteResponse.Kind = client_directive_continuation` without exposing internal Client Tool calls;
- `AgentExecuteRequest.ClientDirectiveResults` exists as a dedicated request lane;
- the result transport supports only the bounded value shapes allowed by AGN-000040; and
- legacy Client Directive wire shapes or model-text directive envelopes are not required by the interaction.

For a fire-and-forget interaction, the outbound `ClientDirective` transport is required but no inbound result lane is exercised by that interaction.

Transport Exists is primarily shared platform evidence. It may therefore become true for multiple interactions from the same Core/AI implementation evidence.

## 2. Definition Registered

Definition Registered proves that the authored `directiveKey` exists as a code-registered `AgentClientDirectiveDefinition` in `LagoVista/ai`.

Confirm from source that:

- the registered `Action` matches the authored dotted `directiveKey` exactly;
- the directive has an explicit response mode;
- model-facing usage metadata exists;
- any optional build/result handler is discoverable through the registered directive type; and
- the definition is registered through the canonical Client Directive registry rather than through an ad hoc response-writing path.

A generic registry or framework does not satisfy this check by itself. The specific authored interaction must have a registered definition.

## 3. Invocation Wired

Invocation Wired proves that the model can invoke this specific registered directive through the formal AGN-000040 invocation path.

For `RequiresResult` directives, confirm that the interaction can be resolved through `invoke_client_directive` and enters the client-execution-required flow.

For `FireAndForget` directives, confirm that the interaction can be resolved through `invoke_fire_and_forget_directive`, the canonical `ClientDirective` is added to the response, and the model receives the immediate submitted result.

Also confirm that:

- the registered directive catalog is exposed when Client Directive invocation tools are active;
- any build handler runs before the final outbound payload is emitted;
- the outbound payload is the canonical `ClientDirective` shape; and
- the model is not expected to manufacture a raw directive JSON envelope in assistant text.

Per-Agent narrowing of the registered directive catalog is a future governance extension and does not change this source-conformance check.

## 4. Completion Wired

Completion Wired proves that the declared response mode completes correctly.

For `RequiresResult` directives, confirm from source that:

1. pending directive state is persisted on the originating `AgentSessionTurn`;
2. unrelated normal input cannot bypass the pending interaction;
3. the next request accepts exactly one matching `ClientDirectiveResult` through the dedicated request lane;
4. `DirectiveId` and `Action` are correlated to the pending directive;
5. allowed outcomes and bounded response values are validated;
6. duplicate completion does not repeat deterministic side effects;
7. any configured result handler runs at most once; and
8. the normalized semantic result is translated back into the suspended model tool call and normal model reasoning resumes.

The server MAY reuse `ToolCallManifest` internally for the suspended model call, but that internal continuation mechanism must not leak into the Client Directive wire contract.

For `FireAndForget` directives, Completion Wired means the invocation completes server-side without creating a pending Client Directive result obligation and returns the canonical immediate tool result to the model.

A card-local client completion handler does not satisfy this server check.

## 5. Contract Conforms

Contract Conforms compares the registered server implementation to the authored Client Interaction definition.

Confirm that:

- registered action equals authored `directiveKey`;
- response mode matches whether the authored interaction returns a terminal completion;
- invocation payload handling matches the authored `invocation` contract;
- allowed result values match authored terminal outcomes;
- allowed response value shape matches `responseContract`;
- Auth Interactions permit no response value;
- user-entered text, when allowed by a general interaction, uses the bounded string Scalar rather than a second response channel;
- client-provided arbitrary JSON is not accepted as a result; and
- any server-side deterministic enrichment does not change the authored semantic contract.

A server implementation that functions but differs from authored truth is not conformant.

## Relationship to client Behavior Wired

The Server dimension and client `Behavior Wired` dimension intentionally overlap at the seam but prove different things.

Server 5/5 proves that Aptix can issue and resume the authored interaction correctly without reference to Angular or React Native presentation.

Client `Behavior Wired` proves that the shared `AgentChatEngine` and the specific platform can consume that server contract, dispatch the intended handler, return the modeled completion, and apply the resumed agent response.

Once Server is tracked separately, client `Behavior Wired` should no longer infer whether the server contract exists. It may require Server 5/5 as a prerequisite and then focus its source evidence on the shared client engine and platform path.

## Runtime evidence

Runtime remains the final end-to-end proof and is not duplicated in the Server 5/5 score.

Runtime evidence should prove the complete path:

`model -> registered directive -> server invocation -> ClientDirective -> AgentChatEngine -> platform handler -> ClientDirectiveResult -> server continuation -> model`

For fire-and-forget interactions, omit the result/continuation leg.

## Reference specimen: Accept or Reject Terms and Conditions

Authored interaction:

`auth-model/client-interactions/accept-reject-terms-and-conditions.json`

Canonical directive key:

`client.terms-and-conditions`

This interaction requires a terminal result and no response value. The registered server definition uses the result-bearing invocation path and allows exactly the authored terminal outcomes:

- `accepted`
- `rejected`
- `canceled`
- `failed`

Current source-backed server evidence is:

`Transport Exists ✓   Definition Registered ✓   Invocation Wired ✓   Completion Wired ✓   Contract Conforms ✓`

or:

`Server 5/5`

The reference implementation proves the canonical outbound/inbound transport, registered Terms definition, model-facing catalog and invocation tool, durable `AgentSessionTurn` pending state, correlated result validation, internal model-call resume, and exact no-response-value authored contract. Runtime evidence remains a separate final stage and is not implied by Server 5/5.

## Reconciliation rule

When reconciling one Client Interaction to green:

1. validate authored Definition;
2. validate Auth Bindings when applicable;
3. reconcile Server 5/5;
4. reconcile Angular Web 3/3;
5. reconcile React Native 3/3;
6. run runtime evidence; and
7. project final green state.

Server implementation evidence must be refreshed against current repository commits whenever material AGN-000040 runtime code or the interaction-specific directive implementation changes.
