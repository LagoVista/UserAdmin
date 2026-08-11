# Category, Behavior, and Scenario Field Review

## Purpose

This document reviews the existing behavior-category catalog, Behavior V2, and Scenario V2 contracts as they exist today.

The goal is to tighten the current model, not redesign it. The default classification is **Keep**. A field is changed only when its current meaning is ambiguous, its references cannot be reliably resolved, it duplicates another canonical expression, or its schema disagrees with actual consumers.

Classifications:

- **Keep** - clear durable role.
- **Clarify** - useful field whose semantic boundary needs stronger documentation.
- **Validate** - keep the shape and strengthen schema or graph checks.
- **Consolidate** - possible duplicate expression; preserve until consumer review proves consolidation safe.
- **Migrate** - current shape cannot express the intended contract; expected to be rare.

## Behavior Category Catalog

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `$schema` | Schema contract for the category catalog. | **Keep / Validate** | Schema location is contract metadata. |
| `schemaVersion` | Catalog JSON contract version. | **Keep** | Separate from category reconciliation state. |
| `key` | Canonical identity of the category catalog document. | **Keep** | Fixed as `auth.catalog.behavior-categories`. |
| `name` | Human-readable catalog name. | **Keep** | Not an identity. |
| `inventoryStatus` | Review status of the category inventory itself. | **Keep / Clarify** | Describes whether the category set has been reconciled, not category readiness and not runtime status. |
| `categories[]` | Canonical behavior-category inventory. | **Keep / Validate** | Category keys must be unique. JSON Schema `uniqueItems` does not by itself guarantee unique category keys when objects differ in other properties, so graph/tool validation should check key uniqueness explicitly. |
| `categories[].key` | Catalog-scoped category identity such as `password-sign-in`. | **Keep / Validate** | Intentionally not a global `auth.*` key. Behavior and Scenario `categoryKey` fields must resolve against this catalog. |
| `categories[].name` | Human-readable category name. | **Keep** | Never a reference key. |
| `categories[].behaviorKeys[]` | Typed references to Behaviors that belong to the category. | **Keep / Validate** | Every active reference must resolve to one Behavior whose `categoryKey` points back to this category. Bidirectional agreement should be validated. |
| `categories[].progress` | Category-level authored reconciliation roll-up. | **Keep / Clarify / Validate** | Authored review state only. It must not be inferred from C# or UI runtime results. Roll-up should agree with active behavior/scenario reconciliation. |
| `sourceReferences[]` | Provenance for the category inventory. | **Keep** | Historical/review evidence, not runtime pass/fail. |

## Behavior V2

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `$schema` | Behavior JSON Schema contract. | **Keep / Validate** | Compatibility-sensitive contract metadata. |
| `schemaVersion` | Behavior JSON contract version. | **Keep** | Current V2 value is `2.0`. |
| `key` | Canonical global Behavior identity. | **Keep / Validate** | Must be unique and begin `auth.behavior.`. |
| `name` | Human-readable behavior name. | **Keep** | Not an identity. |
| `summary` | Concise statement of the behavior. | **Keep** | Semantic documentation. |
| `version` | Semantic authored-definition version. | **Keep / Clarify** | Changes when the behavior definition materially changes; distinct from schema version. |
| `maturity` | Acceptance/lifecycle state of the behavior definition. | **Keep / Clarify** | Independent of reconciliation `progress` and runtime evidence. |
| `progress` | Reviewed downstream authored reconciliation for scenarios/presentation/implementation/tests. | **Keep / Clarify / Validate** | Explicit authored status. Runtime results never mutate it automatically. |
| `categoryKey` | Typed reference to the behavior category catalog. | **Keep / Validate** | Must resolve to exactly one catalog category, which should include this Behavior in `behaviorKeys[]`. |
| `scenarioKeys[]` | Ordered composition of atomic scenarios that realizes the behavior. | **Keep / Validate** | Order is meaningful. Current schema should be tightened to reject duplicate scenario keys while preserving order. Every active scenario must resolve and belong to the same category unless an explicit cross-category composition rule is introduced. |
| `entryViewKey` | Declared starting surface of the composed behavior. | **Keep / Validate** | Useful behavior summary. Validate that it agrees with the first active scenario's `startViewKey` when scenario composition is present. Do not remove merely because it can be derived. |
| `finalViewKey` | Declared final surface of the composed behavior. | **Keep / Validate** | Useful behavior summary. Validate that it agrees with the last active scenario's `expectedViewKey` where applicable. |
| `finalOutcome` | Human-readable semantic outcome that distinguishes this behavior from sibling outcome behaviors. | **Keep** | Important because behaviors are outcome-specific. |
| `supportedPlatforms[]` | Execution surfaces on which the behavior is supported (`server`, `web`, `android`, `ios`, `vtm`). | **Keep / Clarify / Validate** | These are execution capabilities, not source-code implementation projection owners. Should be consistent with the union of active scenario evidence requirements/capabilities. |
| `sourceReferences[]` | Evidence/provenance supporting the behavior definition. | **Keep** | References should remain typed by `sourceType`. |
| `definitionHash` | Optional canonical definition hash. | **Keep / Clarify** | Useful for freshness/proof correlation where present; consumers should not assume the file must persist a hash if tooling can calculate it. |

## Scenario V2

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `$schema` | Scenario JSON Schema contract. | **Keep / Validate** | Compatibility-sensitive metadata. |
| `schemaVersion` | Scenario JSON contract version. | **Keep** | Current V2 value is `2.0`. |
| `key` | Canonical global Scenario identity. | **Keep / Validate** | Must be unique and begin `auth.scenario.`. |
| `runtimeEntityId` | Stable compatibility identity used by the C# testing/runtime layer. | **Keep / Clarify** | Required by `AppUserTestingDslRepo`. Do not derive or replace it casually. |
| `name` | Human-readable scenario name. | **Keep** | Not identity. |
| `summary` | Concise deterministic scenario description. | **Keep** | Human-readable semantic contract. |
| `version` | Semantic authored-definition version. | **Keep / Clarify** | Distinct from schema version and runtime run identity. |
| `maturity` | Acceptance/lifecycle state of the scenario definition. | **Keep / Clarify** | Independent of authored reconciliation progress and execution evidence. Deprecated scenarios remain historical and should be excluded from active roll-ups/runnable inventory. |
| `progress` | Authored reconciliation for presentation/implementation/tests. | **Keep / Clarify / Validate** | Explicit Git-authored review status, not execution status. |
| `categoryKey` | Typed reference to the behavior category catalog. | **Keep / Validate** | Same identity domain as Behavior `categoryKey`. Current Scenario schema uses a looser pattern than the catalog/Behavior schema; validation should be aligned without changing the field or its values. |
| `startViewKey` | Canonical starting surface. | **Keep / Validate** | `auth.*` values must resolve to AuthView. `app.*` values represent application surfaces outside the AuthView catalog and need a clearly understood resolver/boundary. |
| `action.id` | View-scoped semantic action identity invoked by the scenario. | **Keep / Validate** | Must resolve to an action on `startViewKey`. |
| `action.finder` | Executable semantic finder for the action. | **Keep / Validate** | Must resolve to the same AuthView action as `action.id`; the two fields must not drift independently. |
| `preconditions.expression` | Human-readable statement of required starting state. | **Keep / Clarify** | Explanatory semantic contract. It is not parsed as the runtime setup model. |
| `preconditions.state` | Machine-readable state projection used to establish/verify the starting condition. | **Keep / Validate** | Complements the expression rather than duplicating it. The two should be semantically consistent. |
| `inputs[]` | Values the runner applies before invoking the scenario action. | **Keep / Validate** | Each finder should resolve to an applicable AuthView control when the input belongs to an AuthView. |
| `inputs[].finder` | Semantic executable locator of the input control. | **Keep / Tighten validation** | Current schema permits `field:` and `control:` while AuthView canonical finder vocabulary does not define `control:`. No authored Scenario use of `control:` has been demonstrated in this review. Resolve the vocabulary inconsistency before adding new uses; do not invent a second locator namespace unnecessarily. |
| `inputs[].name` | Human-readable input name. | **Keep** | Logging/review aid, not identity. |
| `inputs[].valueType` | Describes how the authored/runtime value should be interpreted. | **Keep / Validate** | Especially important for secret-reference and reference values. |
| `inputs[].required` | Whether this scenario requires the input. | **Keep** | Scenario execution requirement, distinct from whether the owning AuthView control is universally required. Their relationship should be validated where applicable. |
| `inputs[].value` | Concrete or symbolic runtime value reference. | **Keep / Clarify** | Secret references are identifiers, not secret material. |
| `inputs[].example` | Optional authoring/example value. | **Keep / Clarify** | Documentation/example aid, not runtime truth unless a runner explicitly chooses it. |
| `serverInteraction.required` | Whether this atomic scenario requires server-side authentication behavior. | **Keep / Validate** | Drives C# Flow applicability. Client-only navigation scenarios remain valid without server proof. |
| `serverInteraction.intent` | Human-readable statement of the server interaction. | **Keep** | Semantic documentation. |
| `serverInteraction.transitionKeys[]` | Typed references to canonical state transitions exercised/required by the scenario. | **Keep / Validate** | When server interaction is required, transition ownership should be explicit and references must resolve. An empty list may be valid only if there is a deliberate server interaction with no canonical transition, which should be rare and explainable. |
| `expectedViewKey` | Expected resulting surface. | **Keep / Validate** | `auth.*` values resolve to AuthView; `app.*` values need the application-surface resolver/boundary. |
| `expectedVisibleFinders[]` | Semantic elements expected to be visible on the resulting surface. | **Keep / Tighten validation** | Current schema accepts arbitrary non-empty strings. Existing usage should be inventoried and, where these are canonical AuthView finders, graph validation should require them to resolve on `expectedViewKey`. Do not tighten the pattern until any legitimate non-AuthView values are understood. |
| `postconditions.expression` | Human-readable statement of expected resulting state. | **Keep / Clarify** | Semantic explanation. |
| `postconditions.state` | Machine-readable resulting state projection. | **Keep / Validate** | Used by server/runtime evaluation. Must remain consistent with the expression. |
| `expectedAuthLogEvents[]` | Expected server audit/authentication events. | **Keep / Validate** | Relevant when server behavior is expected; should map to known event vocabulary where one exists. Empty is legitimate for UI-only scenarios. |
| `evidenceRequirements[]` | Required execution proof surfaces. | **Keep / Validate** | This is the canonical source for required server/Web/iOS/Android/VTM runtime proof. It is not implementation projection ownership. |
| `sourceReferences[]` | Provenance for scenario semantics and implementation/test evidence. | **Keep** | Preserve typed source roles. |
| `definitionHash` | Optional canonical definition hash. | **Keep / Clarify** | Useful for evidence freshness when present/computed. |

## Existing graph invariants to tighten

These rules require no new top-level model shape.

1. Every `Category.behaviorKeys[]` reference resolves to an active Behavior whose `categoryKey` points back to that category.
2. Every active Behavior `categoryKey` resolves to the catalog and the catalog contains that Behavior.
3. Every active Scenario `categoryKey` resolves to the catalog.
4. Every Behavior `scenarioKeys[]` reference resolves to an active Scenario, and duplicate scenario references are rejected.
5. Behavior scenario ordering is preserved and validated as a connected sequence where one scenario's expected view should feed the next scenario's starting view.
6. Behavior `entryViewKey` agrees with the first scenario start view when both are present.
7. Behavior `finalViewKey` agrees with the final scenario expected view when both are present.
8. Scenario `action.id` and `action.finder` resolve together to one action on the starting AuthView.
9. Scenario inputs resolve to compatible canonical controls when those inputs belong to an AuthView.
10. Scenario `serverInteraction.transitionKeys[]` resolve to canonical transitions.
11. Scenario `expectedVisibleFinders[]` resolve to declared elements on the expected AuthView whenever the expected surface is an AuthView.
12. Scenario runtime evidence requirements remain separate from Behavior/AuthView implementation projection metadata.
13. Deprecated Behaviors/Scenarios remain historical and do not participate in active readiness or runnable inventories.

## Concrete schema-tightening candidates

The following are small contract alignments, not redesign proposals:

### Behavior `scenarioKeys[]` uniqueness

The array is an ordered decomposition, but the current Behavior V2 schema does not specify `uniqueItems: true`. A behavior should not accidentally execute/reference the same atomic Scenario twice unless the model explicitly introduces repetition semantics. Current behavior semantics do not do so.

### Shared category-key validation

Behavior `categoryKey`, Scenario `categoryKey`, and catalog category `key` all refer to the same identity domain but currently use slightly different regex constraints. They should share one semantic validation rule or be graph-validated against the catalog rather than drifting independently.

### Scenario input finder vocabulary

Scenario inputs currently permit both `field:*` and `control:*`, while canonical AuthView finders define `field:*` but not `control:*`. No current authored need for `control:*` has been demonstrated. The safest next step is inventory plus validation, not creation of another finder convention.

### Expected visible finder resolution

`expectedVisibleFinders[]` currently permits arbitrary strings. Before changing its schema, validate existing values and determine whether every active value is intended to be a canonical semantic finder. If yes, the existing field can simply be tightened; no replacement property is needed.

## Compatibility observations

The C# `AppUserTestingDslRepo` directly consumes current Scenario fields including `key`, `runtimeEntityId`, `startViewKey`, `expectedViewKey`, `action.id`, `action.finder`, input finders/values, precondition state, postcondition state, and expected auth-log events.

Aptix's Auth Implementation panel directly consumes Category/Behavior/Scenario fields and directory roots. Field renames therefore require coordinated consumer changes and are not part of routine normalization.

## Current recommendation

Keep the existing Category, Behavior V2, and Scenario V2 shapes.

Tighten them in this order:

1. make typed reference resolution authoritative;
2. align category-key validation;
3. enforce Behavior scenario uniqueness and sequence consistency;
4. validate Scenario actions/inputs/finders against AuthViews;
5. validate expected visible finders against expected AuthViews;
6. exclude deprecated definitions from active inventories;
7. only then consider whether any truly redundant authored field can be consolidated.

The existing structure already expresses the authentication journey well. The work here is to make it impossible for the same structure to quietly mean two different things.