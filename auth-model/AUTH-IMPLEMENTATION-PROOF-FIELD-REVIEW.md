# Implementation and Proof Field Review

## Purpose

This document reviews the existing Action, Transition, Flow, Handler, Endpoint Binding, and Test Binding contracts.

The goal is to preserve the current implementation model while making ownership and cross-layer conformance unambiguous. Repeated references are not automatically duplication. A repeated value is useful when each layer independently asserts its own relationship to one canonical definition and tooling can validate those assertions against each other.

Classifications:

- **Keep** - clear durable role.
- **Clarify** - useful field whose boundary needs stronger documentation.
- **Validate** - keep the shape and strengthen cross-document checks.
- **Consolidate** - same ownership fact appears more than once; preserve until proven safe to collapse.
- **Migrate** - existing shape cannot express intended semantics; expected to be rare.

## Canonical Action

The Action answers: **what operation is allowed to be attempted, under what constraints, and with what permitted effects?**

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `key` | Canonical action identity (`auth.action.*`). | **Keep / Validate** | Global stable identity. |
| `name`, `summary` | Human-readable semantic definition. | **Keep** | Not identity. |
| `version`, `maturity` | Authored-definition lifecycle. | **Keep / Clarify** | Independent of runtime evidence. |
| `requiredInputs[]` | Semantic inputs required by the action. | **Keep / Validate** | These are logical action inputs, not UI controls. Scenario/AuthView bindings project user input into them. |
| `guards[]` | Conditions under which the action may be attempted. | **Keep / Validate** | Guard expressions are semantic predicates. Their relationship to state dimensions/invariants should remain explicit. |
| `permittedMutations[]` | Canonical state areas the action is allowed to mutate. | **Keep / Validate** | Important security boundary. Referenced keys must resolve. |
| `requiredEffects[]` | Effects that must occur when applicable. | **Keep / Validate** | Semantic obligation, not implementation receipt. |
| `forbiddenEffects[]` | Effects that must not occur. | **Keep / Validate** | Security/behavior boundary. |
| `invariantKeys[]` | Typed invariant references that remain enforced. | **Keep / Validate** | Must resolve to canonical invariants. |
| `sourceReferences[]`, `definitionHash` | Provenance/freshness metadata. | **Keep** | Standard authored-definition support. |

## Canonical Transition

The Transition answers: **given an action and starting state, which deterministic modeled outcome/state change occurs?**

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `key` | Canonical transition identity (`auth.transition.*`). | **Keep / Validate** | Stable global identity. |
| `name`, `summary`, `version`, `maturity` | Standard authored-definition semantics. | **Keep** | No redesign needed. |
| `categoryKey` | Optional behavior-category association. | **Keep / Validate** | Same catalog identity domain as Behavior/Scenario category keys. Validation should use the catalog rather than a drifting regex. |
| `priority` | Ordering/disambiguation where multiple transitions could match. | **Keep / Clarify** | Only meaningful when competing transitions share an action/source domain. Validator should reject ambiguous accepted transitions where priority/guards do not resolve ambiguity. |
| `sourceState` | Starting-state predicate for the transition. | **Keep / Validate** | Canonical state condition, distinct from a UI Scenario precondition projection. |
| `actionKey` | Typed reference to the canonical Action that triggers this transition family. | **Keep / Validate** | Must resolve to exactly one Action. |
| `guards[]` | Additional predicates selecting this outcome. | **Keep / Validate** | Distinguish transition-selection guards from Action-level permission guards. |
| `destinationTransform[]` | Canonical state mutations performed by this transition. | **Keep / Validate** | Dimension keys must resolve; mutations should be permitted by the Action and remain invariant-safe. |
| `requiredEffects[]`, `forbiddenEffects[]` | Outcome-specific effects/forbidden effects. | **Keep / Validate** | Validate compatibility with the Action's broader effect contract. |
| `invariantKeys[]` | Canonical invariants relevant to this transition. | **Keep / Validate** | Must resolve. |

## Authentication Flow

The Flow answers: **what stable application-level authentication operation groups these entry actions, handlers, and canonical outcomes?**

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `key` | Canonical flow identity (`auth.flow.*`). | **Keep / Validate** | Stable authored identity. |
| `requestContract`, `resultContract` | Application-level typed request/result contracts. | **Keep / Validate** | These deliberately repeat handler contract names as a conformance assertion. They should agree with referenced handlers, not be removed. |
| `handlerKeys[]` | Typed handler bindings implementing the flow. | **Keep / Validate** | Must resolve. |
| `transitionKeys[]` | Canonical outcomes the flow is allowed to emit. | **Keep / Validate** | This is flow ownership of outcome set, not duplicate transition definition. |
| `entryActionKeys[]` | Canonical actions entering the flow. | **Keep / Validate** | Must resolve and should agree with the transition/action graph. |
| `supportedAudiences[]` | Audiences for which the flow is exposed. | **Keep / Clarify** | Application policy/classification, not runtime test platform. |
| `notes`, `sourceReferences` | Provenance/context. | **Keep** | No issue. |

## Flow Handler Binding

The Handler Binding answers: **which concrete implementation class/method implements the flow behavior and which canonical outcomes can it emit?**

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `key` | Canonical handler-binding identity (`auth.handler.*`). | **Keep / Validate** | Stable implementation binding key. |
| `repository`, `projectPath`, `sourcePath` | Source-code location. | **Keep / Validate** | Concrete provenance, not duplicated by the Flow. |
| `interfaceName`, `typeName`, `methodName` | Concrete implementation symbols. | **Keep / Validate** | Tooling should eventually verify symbols where practical. |
| `requestContract`, `resultContract` | Concrete handler contracts. | **Keep / Validate** | Should agree with owning/referencing Flow contracts. Repetition is a useful conformance assertion. |
| `transitionKeys[]` | Outcomes this handler may emit/classify. | **Keep / Validate** | Should be compatible with the owning Flow transition set. |
| `dependencies[]` | Concrete dependency surface used by the handler. | **Keep / Clarify** | Implementation provenance, not canonical domain ownership. |
| `notes`, `sourceReferences`, `definitionHash` | Standard provenance/freshness metadata. | **Keep** | No redesign needed. |

## Endpoint Binding

The Endpoint Binding answers: **which concrete HTTP endpoint exposes the flow/handler and what public transport contract does it use?**

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `key` | Canonical endpoint-binding identity (`auth.endpoint.*`). | **Keep / Validate** | Stable implementation binding key. |
| `repository`, `projectPath`, `sourcePath` | Concrete REST implementation location. | **Keep / Validate** | Useful implementation provenance. |
| `controllerType`, `methodName` | Concrete endpoint symbols. | **Keep / Validate** | Should match source. |
| `httpMethods[]`, `routeTemplates[]` | Public HTTP exposure. | **Keep / Validate** | Generated/public contracts should agree with these values. |
| `requestContract`, `responseContract` | Public transport contracts. | **Keep / Validate** | The response contract may legitimately wrap/change the internal Flow result contract, so it is not a duplicate alias. |
| `handlerKey` | Typed handler relationship. | **Keep / Validate** | Must resolve. The endpoint may delegate through a flow service while still naming the handler whose canonical behavior it exposes. |
| `transitionKeys[]` | Canonical outcomes reachable through this endpoint. | **Keep / Validate** | Independent endpoint conformance assertion. Should be compatible with handler/flow sets. |
| `projectionNotes`, `sourceReferences`, `definitionHash` | Projection-specific context and provenance. | **Keep** | Appropriate boundary for implementation notes. |

## Test Binding

The Test Binding answers: **which real tests prove which implementation path and canonical outcomes, and what proof obligations must those tests satisfy?**

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `key` | Canonical test-binding identity (`auth.test-binding.*`). | **Keep / Validate** | Authored proof specification identity, not a test-run identity. |
| `testLevel` | Level/kind of proof. | **Keep / Clarify** | Distinguishes flow/handler/endpoint/UI proof expectations. |
| `repository`, `projectPath`, `sourcePath` | Test source location. | **Keep / Validate** | Concrete proof provenance. |
| `framework`, `testTypeName`, `testMethodNames[]` | Runnable test implementation symbols. | **Keep / Validate** | Should be machine-verifiable where possible. |
| `handlerKeys[]` | Handlers exercised/proven by the tests. | **Keep / Validate** | Typed proof relationship. |
| `endpointBindingKeys[]` | Endpoints exercised/proven by the tests. | **Keep / Validate** | Typed proof relationship. |
| `transitionKeys[]` | Canonical outcomes covered by the test binding. | **Keep / Validate** | This is sufficient to associate server-interacting Scenarios through their own transition references when that relationship is unambiguous. |
| `proofObligations[]` | Named semantic obligations the test binding promises to prove. | **Keep / Validate** | These define authored Tests completeness; latest run success is separate C# Flow evidence. |
| `notes`, `sourceReferences`, `definitionHash` | Standard proof provenance/context. | **Keep** | No redesign needed. |

## Repeated transition references are deliberate conformance assertions

Password Sign-In provides the reference specimen.

The following layers each declare the same three canonical outcomes:

- Flow `auth.flow.password-sign-in`
- Handler `auth.handler.password-sign-in`
- Endpoint `auth.endpoint.password-sign-in`
- Test Binding `auth.test-binding.password-sign-in`

The values are:

- `auth.transition.password-sign-in.success`
- `auth.transition.password-sign-in.rejected`
- `auth.transition.password-sign-in.locked-out`

These repeated references should remain because each document answers a different question:

- **Flow:** which outcomes belong to the application operation?
- **Handler:** which outcomes can this implementation emit?
- **Endpoint:** which outcomes are reachable through this HTTP surface?
- **Test Binding:** which outcomes does this proof cover?

The canonical Transition definitions remain the one source of truth for what each transition means. The repeated keys are independently checkable relationships to that truth.

A validator should compare these sets and report unexplained widening/narrowing rather than removing the fields.

## Scenario-to-test proof does not currently require another direct reference

An earlier normalization idea proposed adding `scenarioKeys[]` to Test Bindings.

This review does **not** recommend adding that field unless a concrete ambiguous case is demonstrated.

The existing graph is already typed:

`Scenario.serverInteraction.transitionKeys[] -> Transition.key <- TestBinding.transitionKeys[]`

For Password Sign-In, each active server Scenario names one canonical transition, and the test binding names the transition set it proves. Aptix can already resolve proof through those references.

Adding a second direct Scenario-to-TestBinding edge would create another authored expression that could drift from the transition relationship. That would run counter to the normalization goal unless the current graph proves insufficient for a real case.

Therefore:

- keep transition-based proof ownership as the default;
- validate it strongly;
- only add another reference when an actual proof relationship cannot be represented unambiguously through the current graph.

## Cross-layer invariants to tighten

These checks use the existing fields.

1. Every Flow `handlerKeys[]`, `transitionKeys[]`, and `entryActionKeys[]` reference resolves.
2. Every Handler `transitionKeys[]` reference resolves and is compatible with the Flow(s) that reference the handler.
3. Flow and Handler request/result contracts agree unless an explicit adapter boundary documents why they differ.
4. Every Endpoint `handlerKey` resolves.
5. Endpoint transition keys are compatible with the referenced Handler/Flow transition set.
6. Endpoint request/response contracts agree with the actual public controller signature/generated contract.
7. Every Test Binding handler/endpoint/transition reference resolves.
8. Test Binding transition coverage is compatible with the implementation path it claims to prove.
9. Scenario server transition requirements can resolve to at least one authored Test Binding when C# proof is required.
10. Runtime evidence may prove a Test Binding/transition/scenario, but it never mutates the authored Test Binding or authored progress automatically.
11. Deprecated implementation/proof bindings remain historical and are excluded from active readiness unless explicitly referenced by an accepted active definition.

## Current recommendation

Keep the existing Action → Transition → Flow → Handler → Endpoint → Test Binding structure.

It is intentionally opinionated and gives us unusually strong traceability from semantic behavior to concrete source code and proof.

The tightening work should focus on:

1. typed reference resolution;
2. cross-layer contract equality/compatibility checks;
3. transition-set agreement checks;
4. symbol/source existence checks where tooling can perform them reliably;
5. proof-obligation coverage;
6. removing only references or duplicate convenience fields that have no distinct ownership meaning and no remaining consumer.

The repeated transition keys are not four definitions of a transition. They are four independently verifiable statements about one transition definition.