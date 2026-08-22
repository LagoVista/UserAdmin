# Client Interaction Automated Testing

## Purpose

This playbook defines the lightweight client-side runtime proof used by Client Interactions and Auth Interactions.

The goal is not to reproduce a full production journey. The automated test supplies a canned Agent response to a purpose-built client test host, exercises the real shared Client Interaction path, captures the continuation request that would have been submitted to Aptix, and writes durable evidence back into this repository.

The server is intentionally not involved in this test layer. Server implementation is reconciled separately through `CLIENT-INTERACTION-SERVER-CONFORMANCE.md` and `server.json`.

## Ownership

Authoritative test scripts and generated runtime evidence live in `LagoVista/UserAdmin`.

Shared platform-neutral Client Interaction projection and continuation helpers live in:

`nuviot/ai-client`

Angular presentation is exercised through the real Client Interaction handlers from:

`softwarelogistics/nuviot-ui-shared`

The current Angular test host is provided by `softwarelogistics/ml-workbench-ui` at:

`http://localhost:4200/client-interaction-test`

React Native will use the same authored fixtures, result semantics, and evidence contract when its test host is added.

## Test boundary

The automated client test proves this path:

`canned AgentExecuteResponse -> ai-client projection -> platform ClientInteractionHost -> real interaction handler -> user action -> ai-client continuation builder -> captured AgentExecuteRequest`

It does not call the Aptix server, OpenAI, authentication endpoints, or other production orchestration.

Interaction-specific backend calls made by a handler are replaced by deterministic in-memory test behavior in the purpose-built host when needed.

## Angular runner

Scripts live under:

`auth-model/scripts/client-interactions/`

With the test host already running at `http://localhost:4200`, execute:

```text
cd auth-model/scripts/client-interactions
npm install
npm run install:browsers
npm run test:angular
```

The first two setup commands are normally required only when dependencies or the Playwright browser installation are missing.

## Evidence

Angular runtime evidence is written under:

`auth-model/implementation/client-interaction-runtime/angular-web/`

The runner writes:

- `latest.json` as the platform-level index of the most recent run
- one `<interactionKey>.json` file containing detailed test results for each executed interaction

The runner also updates only the runtime evidence fields for the matching interaction in:

`auth-model/implementation/client-interaction-conformance/angular-web.json`

The following conformance fields may be updated by the automated runner:

- `runtimeEvidence`
- `runtimeEvidenceReferences`
- manifest `generatedUtc`

The runner MUST NOT change `exists`, `controlsConform`, `behaviorWired`, source evidence, inspected commit SHA, authored interaction truth, or server conformance.

## Evidence semantics

`runtimeEvidence = passed` means the automated client test for that interaction passed in the most recent recorded run.

`runtimeEvidence = failed` means at least one automated client test for that interaction failed in the most recent recorded run.

`runtimeEvidence = not-run` means there is no current recorded automated client proof.

Detailed evidence SHOULD record the test host, execution timestamp, individual test names, result status, duration, and failure message when applicable.

## Reference specimen

The first automated specimen is:

`client.interaction.terms-and-conditions`

The initial Angular test proves the Accept path by:

1. opening `/client-interaction-test`;
2. supplying a canned Agent response containing `client.terms-and-conditions` and a stable test `DirectiveId`;
3. confirming the authored Terms version, View Terms, Accept, and Reject surfaces render;
4. clicking Accept;
5. confirming the platform reports `accepted`;
6. confirming the captured continuation contains the original `DirectiveId`;
7. confirming `Action = client.terms-and-conditions`;
8. confirming `Result = accepted`; and
9. confirming the continuation is submitted through `clientDirectiveResults` without an additional response value.

Additional terminal outcomes should be added as small independent tests rather than by expanding setup into a full user journey.

## Relationship to Client Interaction -> Green

Automated client runtime evidence is one dimension of `CLIENT-INTERACTION-TO-GREEN.md`.

Source conformance and runtime proof remain distinct:

- Server 5/5 proves the Agent-side contract.
- Angular/RN 3/3 prove source implementation and shared client wiring.
- Automated runtime evidence proves the real platform handler can consume a canned directive and produce the correct bounded continuation.

The Aptix Client Interactions visualizer reads these artifacts from the `auth-model` workspace and should show both status and detailed evidence for the selected interaction.

## Adding a new interaction test

For a new interaction:

1. create or reuse a canned Agent response shaped around the authored `directiveKey`;
2. keep setup deterministic and local to the test host;
3. exercise the real platform `ClientInteractionHost` and real handler;
4. use authored semantic finders for controls and actions;
5. assert the bounded continuation result, including `DirectiveId`, `Action`, outcome, and permitted value shape;
6. name Playwright tests as `<interactionKey> :: <test-name>` so the evidence reporter can group them; and
7. allow the reporter to publish the current pass/fail evidence.

A new interaction should require very little harness code. If substantial workflow setup is required, that logic likely belongs in a different integration or journey test layer.
