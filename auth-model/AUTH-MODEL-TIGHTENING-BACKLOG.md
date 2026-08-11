# Authentication Model Tightening Backlog

This backlog converts the completed top-down field reviews into an ordered, minimal-change implementation plan.

The governing rule is simple:

> Prefer validation and clarification over contract changes. Prefer compatibility-preserving fixes over migrations. Change JSON shape only when the existing shape cannot express one unambiguous meaning.

## 2026-08-11 checkpoint

The foundational tightening pass is complete enough to move on from model auditing.

Completed:

- Aptix loads canonical identity by document type rather than guessing among `key`, `viewId`, and `routeId`.
- Deprecated scenarios are excluded from active readiness/proof rollups while remaining available as historical definitions.
- Aptix performs typed graph validation across Category -> Behavior -> Scenario -> AuthView/AuthRoute presentation relationships.
- Behavior scenario references are schema-enforced as unique.
- Behavior and Scenario `categoryKey` syntax is aligned to the catalog contract.
- Scenario input/visible finder syntax is tightened to the established presentation vocabulary.
- Invariant `relatedDimensionKeys[]` is typed to `auth.dimension.*`.
- Canonical AuthView `routeId` is required, matching the C# Git loader and current authored inventory.
- Password Sign-In authored Test progress is reconciled independently from execution status.
- Password Sign-In AuthViews were checked against Angular and React Native implementations, and confirmed implementation drift was corrected where semantics were unambiguous.
- Welcome and Continue Email now carry concrete Web/Mobile conformance receipts.
- Password Entry accurately records remaining conformance uncertainty instead of claiming false green status.

Intentionally unresolved on `auth.continue.email.password`:

1. whether the canonical `email` control is intended to remain editable on the password-entry surface or represents carried/read-only identity context;
2. the exact navigation semantics of the canonical `cancel` action versus platform Back behavior.

The retained runtime inventory confirms that both the email field and cancel action historically existed, but does not define their intended semantics. They must not be guessed or silently reconciled from one platform implementation.

These two questions do not justify additional model redesign. Resolve them when the generated-screen/editing boundary is defined or when an explicit authored behavior requires them.

The remaining backlog below is retained as the broader hardening inventory. Items outside the completed Password Sign-In specimen should be addressed incrementally as each category or subsystem is reconciled, rather than delaying the next architectural step.

## Priority 0 - Fix validator identity handling

These are correctness bugs in tooling, not model redesign.

1. **Aptix Auth Implementation loader must load identity by document type.**
   - Behavior / Scenario / Test Binding -> `key`
   - AuthView -> `viewId`
   - AuthRoute -> `routeId`
   - Never infer identity from whichever property happens to exist.

2. **Deprecated definitions must be excluded from active readiness/runnable rollups.**
   - Keep them loadable for historical inspection.
   - Do not let deprecated Password Sign-In pilots create active graph errors.

3. **Reference validation must be typed.**
   - `routeId` resolves AuthRoute.
   - `viewId`, `startViewKey`, `expectedViewKey` resolve AuthView/app view as applicable.
   - `categoryKey` resolves the category catalog.
   - `actionKey` resolves Action.
   - `transitionKeys` resolve Transitions.
   - `invariantKeys` resolve Invariants.
   - `dimensionKey` resolves State Dimension.

These changes should happen before broad authored-data edits because they determine whether reported defects are real.

## Priority 1 - Strengthen graph validation without changing JSON shape

### Category / Behavior / Scenario

- category keys unique within catalog;
- category `behaviorKeys[]` unique and resolvable;
- Behavior `scenarioKeys[]` unique and resolvable;
- Behavior `entryViewKey` agrees with first active scenario start view when populated;
- Behavior `finalViewKey` agrees with final active scenario expected view when populated;
- Behavior/Scenario `categoryKey` use one shared semantic validation rule;
- Scenario start/expected views resolve;
- Scenario action `id` + `finder` resolves on start AuthView;
- Scenario input finders resolve to compatible controls;
- `expectedVisibleFinders[]` resolve to canonical controls/actions/status/display finders where applicable.

### AuthView / AuthRoute

- load using explicit canonical identity field;
- AuthView `routeId` resolves AuthRoute when present;
- view/route binding is bidirectionally consistent;
- control IDs unique within view;
- action IDs unique within view;
- control/action finders unique where semantic ambiguity would result;
- action finder suffix agrees with action ID;
- control finder namespace is compatible with control type;
- platform conformance references remain independent of runtime execution evidence.

### State / Invariants

- State Dimension keys unique;
- `values[].key` unique within each dimension;
- composite region keys unique;
- composite dimension references resolve;
- duplicate dependency tuples rejected;
- Invariant `relatedDimensionKeys[]` resolve specifically to State Dimensions;
- Action/Transition invariant references resolve;
- Transition Action reference resolves;
- Transition destination dimension references resolve;
- ambiguous duplicate transforms against one dimension rejected.

### Implementation / proof

- Flow handler references resolve;
- Flow/Handler/Endpoint/Test Binding transition sets are independently validated;
- Endpoint handler references resolve;
- Test Binding handler/endpoint/transition references resolve;
- repeated transition sets are treated as conformance assertions, not duplicate ownership.

### Orchestration / presentation

- Journey scenario references resolve against the catalog the Journey is authored against;
- Journey scenario keys unique;
- alternate Journey references resolve;
- Conversation goal keys and other local keyed collections unique;
- Conversation Journey, Conversation, Scenario, and Action references resolve;
- allowed/prohibited Action lists may not overlap;
- information requirement goal references resolve;
- confirmation Action references resolve;
- Presentation Binding references resolve against Action/navigation/AuthView/AuthRoute truth.

## Priority 2 - Schema strengthening that preserves current documents

Only make schema edits that improve validation without renaming fields or invalidating correct authored intent.

High-confidence candidates:

1. add uniqueness enforcement where JSON Schema can safely express it;
2. tighten typed key prefixes where a field promises one definition type, especially Invariant `relatedDimensionKeys[]`;
3. align Behavior and Scenario `categoryKey` syntax with the canonical category-catalog rule;
4. tighten `expectedVisibleFinders[]` from arbitrary strings to the established finder vocabulary if current authored data conforms;
5. clarify semantic descriptions for prose predicates/visibility conditions so they are not mistaken for executable policy.

Where JSON Schema cannot enforce keyed uniqueness or repository-wide resolution, leave the JSON shape alone and enforce the invariant in graph validation.

## Priority 3 - Reconcile confirmed authored defects one category at a time

Use Password Sign-In first.

After validator false positives are removed:

1. identify only remaining real graph defects;
2. reconcile AuthViews/AuthRoutes against Angular and React Native implementations;
3. reconcile CommonLinks/routes where applicable;
4. reconcile authored Test phase without conflating current execution;
5. run C# Flow proof;
6. run UI Runtime proof on required Web/iOS/Android surfaces;
7. mark authored progress only after the reviewed contract is actually complete.

Do not mass-edit unrelated categories while establishing this specimen.

## Priority 4 - Compatibility aliases / duplicate-expression cleanup

These are candidates, not permission to delete fields.

Examples include implementation provenance repeated between platform records and `source.*` convenience fields.

For each candidate:

1. identify every C#, Aptix, runtime, and platform consumer;
2. determine which field is canonical and which is compatibility/provenance;
3. add validation that duplicate expressions agree;
4. migrate consumers deliberately if consolidation is worthwhile;
5. remove an alias only after no supported consumer depends on it.

`source.runtimeEntityId` is explicitly **not** a cleanup candidate today because the C# runtime consumes it and deterministic fallback IDs do not match existing stored IDs.

## Priority 5 - Deferred / explicit future reconciliation

Do not mix these into current V2 category work:

- Journey migration from retained legacy scenarios to V2 scenarios;
- Conversation migration from retained legacy scenario references to V2;
- Presentation Binding consolidation after stable AuthView child identities and all consumers are fully traced;
- any AuthView/AuthRoute schema-version redesign;
- any renaming of `web`, `mobile`, `category`, `status`, schema filenames, definition roots, or canonical keys.

These require their own compatibility decision if they ever become necessary.

## First implementation slice

When implementation begins, the safest order is:

1. fix Aptix document-type-aware identity loading;
2. exclude deprecated scenarios from active validation/rollups;
3. add typed graph validation for AuthView/AuthRoute/Scenario relationships;
4. re-open Password Sign-In and inspect the now-clean error set;
5. make only the authored-data changes proven necessary by that error set.

This sequence tightens the current model before changing the model.
