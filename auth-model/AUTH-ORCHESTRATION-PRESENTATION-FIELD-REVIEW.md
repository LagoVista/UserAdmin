# Authentication Orchestration and Presentation Field Review

This review covers the existing Journey, Conversation Type, and Presentation Binding contracts using the same minimal-change rubric as the other authentication-model reviews.

The goal is to preserve useful orchestration and adaptation semantics while preventing these layers from becoming alternate sources of authentication behavior.

## Core boundary

The authoritative behavioral chain remains:

**State / Invariant -> Action -> Transition -> Scenario -> Behavior**

Journeys, Conversations, and Presentation Bindings compose, route, or adapt that behavior. They must not redefine guards, state changes, canonical outcomes, or security policy.

## Journey

### `key`, `name`, `summary`, `version`, `maturity`

**Keep**

Standard authored-definition identity and lifecycle metadata.

### `scenarioKeys[]`

**Keep + Validate**

Ordered composition of scenarios into a larger workflow.

Tightening rules:

1. scenario keys must be unique within one Journey;
2. every key must resolve to the scenario catalog that the Journey version was authored against;
3. scenario order is meaningful;
4. adjacent scenarios should form a coherent chain where that can be validated;
5. Journey composition must not alter the semantics of the referenced scenarios.

### Current legacy/V2 boundary

Existing Journeys such as `auth.journey.password.registration` currently reference the retained legacy `auth-model/scenarios/` catalog, not the active `scenarios-v2/` category-to-green catalog.

Those references resolve and the Journey is still `maturity: proposed`.

**Decision: Keep as historical/proposed authored material.**

Do not silently rewrite Journey scenario references during V2 category reconciliation. A later explicit Journey reconciliation may move a Journey to V2 scenarios when the equivalent workflow has been deliberately reviewed.

### `entryPredicate`, `completionPredicate`

**Keep + Clarify**

These describe Journey applicability and completion in semantic state vocabulary. They do not replace Scenario preconditions/postconditions or Transition guards.

As elsewhere in the model, predicate text is semantic unless an explicit executable grammar owns it.

### `alternateJourneyKeys[]`

**Keep + Validate**

Typed references to alternative authored Journeys. Every active reference must resolve and duplicates are invalid.

## Conversation Type

### Purpose

**Keep**

Conversation Types are bounded orchestration/presentation definitions. They may explain, collect permitted non-secret context, select authored Journeys, route to another bounded Conversation, invoke allowed Actions through deterministic mechanisms, and resume after secure interactions.

They must not become a second authentication policy engine.

### `goals[]`

**Keep + Validate**

Local goal vocabulary. Goal keys must be unique within the Conversation.

### `supportedJourneyKeys[]`

**Keep + Validate**

Journeys the Conversation may orchestrate. These references must resolve to the Journey catalog the Conversation was authored against.

### `routedConversationKeys[]`

**Keep + Validate**

Other bounded Conversations this Conversation may route into. These are distinct from supported Journeys and should remain.

The HR onboarding router demonstrates the distinction: it owns no workflow Journey itself and instead routes to bounded child Conversations.

### `scenarioKeys[]`

**Keep, but treat as a compatibility/reconciliation edge**

This field identifies lower-level scenarios the Conversation directly understands or participates in. It is not inherently duplicate with `supportedJourneyKeys[]` because one names composed workflows and the other names atomic scenarios.

However, current Conversation definitions such as `auth.conversation.user-registration` reference the retained legacy scenario catalog while V2 category reconciliation uses `scenarios-v2/`.

Those legacy references currently resolve and the Conversations remain proposed.

**Do not rewrite them opportunistically.** Reconcile the Conversation layer to V2 only as a deliberate future activity.

### `entryPredicate`, `completionPredicate`

**Keep + Clarify**

Conversation applicability/lifecycle semantics only. They cannot weaken the entry/completion rules of referenced Journeys, Scenarios, Actions, or Transitions.

### `informationRequirements[]`

**Keep + Validate**

This is a valuable explicit boundary for what the conversation may obtain and how it may be collected.

Tightening rules:

1. local requirement keys are unique;
2. every `requiredForGoalKeys[]` item resolves to a local goal;
3. `collection: secure-component` is required for secrets where the contract declares secret sensitivity;
4. Conversation collection rules must never authorize conversational capture of credentials prohibited elsewhere in the model.

### `allowedActionKeys[]`, `prohibitedActionKeys[]`

**Keep + Validate**

These are explicit orchestration permissions, not duplicate Action definitions.

Every reference must resolve to an Action. The same Action should not appear in both lists.

An allowed Action remains subject to its own guards, Transitions, Invariants, and implementation rules.

### `decisionPoints[]`

**Keep**

Human-readable orchestration decisions. They may select among authored paths but cannot invent new state outcomes.

### `confirmationRequirements[]`

**Keep + Validate**

Useful binding between conversational orchestration and state-changing Actions.

Every `requiredBeforeActionKeys[]` entry must resolve to an Action permitted by the surrounding contract.

### `secureInteractions[]`

**Keep**

This is an important architectural boundary: secrets are collected by deterministic secure components rather than conversational context.

The Conversation may provide safe context and receive sanitized outcomes, but it does not own credential semantics.

### `lifecycle`

**Keep**

Pause/resume/abandonment/expiration are conversation-orchestration semantics and are distinct from authentication state lifecycle.

### `supportedChannels[]`

**Keep + Clarify**

Execution/presentation channels supported by the Conversation. This is not implementation-projection ownership and is not runtime evidence.

### `presentationExpectations[]`, `exampleInteractions[]`

**Keep**

These are explanatory/presentation guidance. They are not executable authentication rules.

## Presentation Binding

### Purpose

**Keep, with careful scope**

Presentation Bindings exist to connect presentation mechanics to canonical Actions or UI-only navigation without inventing authentication transitions.

The existing split is useful:

- `canonical-action` - a presentation invokes an existing logical Action;
- `ui-navigation` - presentation moves between views without canonical authentication state mutation.

### `actionKey` / `navigationKey`

**Keep + Validate**

Exactly one relationship is required according to binding kind. References must resolve to the applicable authored definition.

### view/route/finder fields

**Keep + Consolidation Review**

Presentation Bindings were authored when stable Git AuthView child identities were less mature. They therefore carry route, view, field/action IDs, semantic finders, and implementation references.

Do not remove these fields simply because newer AuthView definitions now carry stronger control/action identities.

Instead, for each active Presentation Binding:

1. identify which fields are still consumed;
2. validate them against current AuthView/AuthRoute truth;
3. classify exact duplicate expressions as compatibility aliases only after consumers are traced;
4. remove nothing until the compatibility map proves it is safe.

### `platform`

**Keep + Clarify**

This describes the presentation binding's target execution/presentation surface. It must not be confused with AuthView implementation projection records or UI Runtime evidence.

## Legacy/proposed orchestration policy

The current repository intentionally contains older Journey, Conversation, and Presentation authored material alongside the active V2 category-to-green model.

This is acceptable when the boundary is explicit:

1. legacy references must still resolve within their retained catalog;
2. proposed legacy orchestration does not participate in active V2 category readiness unless explicitly reconciled;
3. validators should distinguish active-graph defects from historical/proposed-layer debt;
4. migration to V2 references is deliberate and behavior-preserving, never automatic string substitution;
5. no legacy authored intent is deleted merely because a newer scenario representation exists.

## Concrete tightening candidates

No redesign is required by this review.

Useful tightening work is:

1. enforce unique Journey `scenarioKeys[]`;
2. resolve Journey scenario and alternate-Journey references against the correct catalog;
3. enforce unique Conversation local keys for goals and other keyed child collections;
4. resolve Conversation journey, routed-conversation, scenario, and Action references;
5. detect Actions appearing in both allowed and prohibited lists;
6. resolve information-requirement goal references;
7. resolve confirmation Action references;
8. validate Presentation Binding references against AuthView/AuthRoute/Action/navigation truth;
9. explicitly classify Journey/Conversation references to retained legacy scenarios as proposed compatibility material rather than active V2 graph failures.

## Compatibility rule

Do not rename Journey, Conversation, Presentation Binding, scenario, route, view, or action fields as part of this tightening pass.

Any future contract shape change must first trace C#, Aptix, platform implementation, schema, and authored-file consumers under `AUTH-MODEL-COMPATIBILITY-MAP.md`.
