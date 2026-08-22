# Client Interaction Final Sign-Off

## Purpose

Final sign-off is intentionally separate from implementation conformance.

`Server 5/5`, `Angular 3/3`, and `React Native 3/3` describe whether the implementation exists and conforms to the authored contract. They do not by themselves prove that the interaction has been deliberately verified.

A Client Interaction reaches final sign-off for a platform only when both gates below pass.

## Gate 1 - Client Test

The client test is an executable platform proof owned by `LagoVista/UserAdmin`.

For Angular Web the current runner lives in:

`auth-model/scripts/client-interactions/`

The runner targets the purpose-built test host at:

`http://localhost:4200/client-interaction-test`

The test injects a canned Agent response, exercises the real shared `ai-client` projection and continuation helpers, renders the real platform Client Interaction handler, performs the user action, and captures the canonical continuation request.

Runtime evidence is written to:

`auth-model/implementation/client-interaction-runtime/<platform>/`

A passing client test is required for final sign-off.

## Gate 2 - Server Code Review

The server code review is a source-backed review of the interaction definition plus the generic Client Directive infrastructure it depends on.

Evidence is stored in:

`auth-model/implementation/client-interaction-server-review/server.json`

Schema:

`auth-model/schemas/client-interaction-server-review.schema.json`

The baseline checklist verifies:

1. directive key matches the authored contract;
2. response mode is correct;
3. allowed outcomes exactly match authored outcomes;
4. value contract matches;
5. definition is registered;
6. invocation uses the approved generic Client Directive infrastructure;
7. continuation validates directive correlation;
8. pending state is persisted correctly;
9. duplicate completion cannot execute twice;
10. mixed/malformed continuation lanes are rejected;
11. completion resumes the intended suspended model call; and
12. legacy/internal tool transport does not leak to the host client.

Additional checks SHOULD be added when an interaction introduces server behavior not covered by this baseline.

## Unit tests

Per-interaction unit tests are not automatically required.

Add them when the interaction has material unique server behavior that can fail independently of the generic Client Directive framework. Do not create tests merely to re-assert declarative constants that are already directly reviewable.

Existing tests may be listed as supporting evidence without claiming they were executed.

When an executed test is useful as formal evidence, use the existing `AptixEvidence` instrumentation used by UserAdmin authentication tests so the result can be harvested into Aptix evidence views.

## Combined sign-off artifact

The Client Interaction Playwright reporter combines the client test result with the current server code-review result and writes:

`auth-model/implementation/client-interaction-signoff/<platform>/<interaction-key>.json`

and:

`auth-model/implementation/client-interaction-signoff/<platform>/latest.json`

The combined status is `passed` only when:

`Client Test = passed AND Server Code Review = passed`

This combined artifact is the preferred visualizer input for final sign-off.

## Reference specimen - Accept Terms and Conditions

Interaction:

`client.interaction.terms-and-conditions`

The current server review is 12/12 and is pinned to the reviewed `LagoVista/AI` and `LagoVista/Core` commits.

No additional Terms-specific unit tests were added because its unique server implementation is declarative; the material continuation behavior is generic Client Directive infrastructure. Existing definition/Core contract tests remain supporting evidence.

Run the Angular proof from:

`auth-model/scripts/client-interactions`

with:

`npm run test:angular`

A successful run writes both runtime evidence and the combined final-signoff artifact.
