# Client Interaction Runtime Tests

These tests prove Client Interaction behavior in a purpose-built client-only harness. The test host is expected to be running at `http://localhost:4200/client-interaction-test`.

The harness does not call the Aptix Agent backend. It injects a canned `AgentExecuteResponse` shape, routes it through shared `nuviot/ai-client` Client Interaction helpers, renders the real Angular handler from `softwarelogistics/nuviot-ui-shared`, performs the authored interaction, and captures the `AgentExecuteRequest.clientDirectiveResults` shape that would be submitted.

## Run

From this directory:

```text
npm install
npm run install:browsers
npm run test:angular
```

## Evidence

Each run writes detailed evidence to:

`auth-model/implementation/client-interaction-runtime/angular-web/`

The custom reporter also updates only the runtime fields in:

`auth-model/implementation/client-interaction-conformance/angular-web.json`

Specifically:

- `runtimeEvidence`
- `runtimeEvidenceReferences`
- manifest `generatedUtc`

Source-inspection fields such as `exists`, `controlsConform`, and `behaviorWired` are not changed by Playwright.

## Test naming

Use this title convention so evidence is associated with the authored interaction:

`<interactionKey> :: <test-name>`

Example:

`client.interaction.terms-and-conditions :: accept`
