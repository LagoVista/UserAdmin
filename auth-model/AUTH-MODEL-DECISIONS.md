# Authentication Model Normalization Decisions

This log records accepted semantic decisions made while normalizing the Git-authoritative authentication model. These decisions refine `AUTH-MODEL-CONTRACT.md` and drive later schema, tooling, and authored-data migration.

Normalization is intentionally incremental. Existing authored data remains valid until the migration step for the affected contract is performed.

## D001 - Category keys remain catalog-scoped

**Status:** Accepted

Authentication behavior categories keep their existing subject-action identifiers such as:

- `password-sign-in`
- `password-recovery`
- `passkey-registration`

A category key is the canonical identity of a category **within the behavior category catalog**. It is not a globally typed `auth.*` definition key.

### Rationale

The existing category catalog, Behavior V2 schema, and Scenario V2 schema already model categories this way. Behavior and scenario definitions reference the same catalog-scoped `categoryKey`.

Changing categories to identifiers such as `auth.category.password-sign-in` would require widespread reference churn without improving type safety, because the containing field already declares the referenced type.

### Rules

1. Category keys are unique within `behavior-category-catalog.json`.
2. `Behavior.categoryKey` and `Scenario.categoryKey` resolve only against that catalog.
3. Category keys use the existing subject-action kebab-case syntax.
4. General `auth.*` stable-key rules apply to globally keyed authored definitions, not catalog-scoped category entries.
5. Tooling must treat `categoryKey` as a typed category reference rather than attempting to resolve it in the global authored-key namespace.

No authored category or category reference migration is required for this decision.

## D002 - Implementation targets and runtime platforms are different concepts

**Status:** Accepted

The model distinguishes **implementation projection targets** from **runtime execution platforms**.

### Implementation projection targets

These identify codebases/projections that implement the canonical model:

- **Server/CommonLinks** - server route and authentication projections
- **Angular** - web UI implementation
- **React Native** - shared mobile UI implementation

A React Native screen or route is one implementation projection even when the same implementation is executed on both iOS and Android.

### Runtime execution platforms

These identify independently executable evidence surfaces:

- `server`
- `web`
- `ios`
- `android`
- `vtm`, where applicable

Web, iOS, and Android remain distinct runtime proof surfaces because platform behavior may differ even when iOS and Android share React Native source.

### Current `mobile` field

Existing AuthView and AuthRoute schemas use `platforms.mobile` for the React Native implementation projection. That data is valid and must be preserved during migration.

The target semantic name for this projection is **React Native**, not generic Mobile. A later schema-alignment step may rename the authored projection property from `mobile` to `reactNative`. That change must be versioned or migrated deliberately; tooling should support the existing field until active authored definitions have moved.

### Rules

1. Do not duplicate a shared React Native implementation record merely to create iOS and Android implementation records.
2. Reconcile the React Native implementation once against the canonical AuthView/AuthRoute/control/action/finder contract.
3. Execute and record UI runtime evidence separately for iOS and Android when each is required by the scenario.
4. Angular implementation reconciliation and Web runtime execution are related but separate statuses.
5. React Native implementation reconciliation and iOS/Android runtime execution are related but separate statuses.
6. `Behavior.supportedPlatforms` and `Scenario.evidenceRequirements` describe supported/required execution surfaces, not source-code projection ownership.

### Intended presentation

A normalized view may conceptually show:

- **Implementation:** Angular ✅ | React Native ✅
- **UI Runtime:** Web ✅ | iOS ○ | Android ○

The runtime row does not imply three independent client implementations.

## D003 - Maturity, reconciliation progress, and execution evidence are independent axes

**Status:** Accepted

The model keeps three concepts separate:

1. **Maturity** describes the lifecycle/acceptance state of the authored definition itself.
2. **Reconciliation progress** describes whether dependent authored layers have been reviewed and agreed as canonical.
3. **Execution evidence** reports whether the current implementation passes C# Flow or UI Runtime proof.

None of these statuses silently updates another.

### Rules

1. `proposed` and `reviewed` definitions may be worked on and reconciled, but they are not yet accepted canonical truth.
2. `approved`, `implemented`, and `verified` all represent an accepted semantic definition for compatibility with the current vocabulary.
3. `implemented` does not imply `progress.implementation == complete`.
4. `verified` does not imply current C# Flow or UI Runtime evidence is passing or fresh.
5. Reconciliation progress never promotes maturity automatically.
6. Test execution never promotes maturity or authored progress automatically.
7. A definition materially changed after approval must be deliberately moved back to an appropriate maturity and its dependent proof considered stale.
8. A definition with `maturity: deprecated` is governed by D005.

### Authored-green implication

An active definition can contribute to an Authored-green roll-up only when:

- its semantic definition is accepted (`approved`, `implemented`, or `verified` under the current vocabulary); and
- every authored reconciliation phase required at that level is `complete`.

This means a definition may have all reconciliation phases complete while still requiring a deliberate maturity promotion from `proposed` or `reviewed` before it is considered fully canonical.

The existing `implemented` and `verified` maturity values are retained during normalization. A later schema decision may simplify the maturity vocabulary, but no mass rewrite is required now.

## D004 - Authored Tests specify proof; C# Flow and UI Runtime execute proof

**Status:** Accepted

The authored `tests` reconciliation phase means **the required test specification and proof obligations have been reviewed and agreed as canonical**. It does not mean the latest test run passed.

### Scenario-level Tests complete

For an active scenario, `progress.tests` may be `complete` when all required proof specifications for that scenario are reconciled.

At minimum:

1. the scenario itself is sufficiently deterministic to be runnable: starting view, action, inputs, expected outcome, state expectations, and evidence requirements are defined where applicable;
2. required UI execution surfaces are declared in `evidenceRequirements`;
3. when `serverInteraction.required == true`, required C# proof obligations are represented by a reconciled test binding that can be associated to the scenario through its canonical handler/endpoint/transition relationships;
4. client-only scenarios do not require an artificial C# test binding merely to make authored Tests complete;
5. any additional category-specific proof obligation is explicitly captured rather than inferred from a passing run.

### Roll-up

- Behavior `tests` is complete when the authored Tests phase is complete for every active scenario used by that behavior.
- Category `tests` is complete when the authored Tests phase is complete for every active behavior/scenario in that category.

### Execution remains separate

- **C# Flow** answers whether the required server integration/flow proof currently passes.
- **UI Runtime** answers whether the required UI executions currently pass on Web, iOS, and/or Android.
- A test run may fail tomorrow without changing authored `tests: complete`.
- A definition change may make prior execution evidence stale without changing authored Tests unless the proof specification itself must be reconciled again.

### Schema/tooling follow-up

The current Password Sign-In test binding is transition/handler/endpoint oriented and does not contain explicit `scenarioKeys`. During schema alignment we should decide whether to add explicit scenario references to test bindings or retain typed graph resolution through canonical transitions. Tooling must not use test execution success as a shortcut for this authored reconciliation decision.

## D005 - Deprecated definitions are historical, not active graph members

**Status:** Accepted

Deprecated authored definitions remain in Git as provenance and historical evidence but do not participate in active readiness, runnable inventories, or status roll-ups.

### Rules

1. Validators may load deprecated definitions so they can be inspected and historically resolved.
2. Deprecated scenarios are excluded from active behavior/category scenario inventory unless explicitly requested for historical analysis.
3. Deprecated definitions are not offered as normal runnable scenarios.
4. Active accepted definitions must not depend on deprecated definitions unless an explicit compatibility exception documents why.
5. A dangling reference inside an entirely deprecated historical subgraph may be reported as historical debt rather than blocking active authored green.
6. A reference from an active definition to a missing or deprecated required definition is an active graph defect.

This decision directly removes deprecated Password Sign-In pilot scenarios from the active runnable/status inventory without deleting their authored history.

## Next decisions

The next contract decisions should be resolved before broad authored-data mutation:

1. schema strategy for the `mobile` -> `reactNative` projection rename;
2. whether implementation reconciliation needs an authored definition/version/hash receipt per projection;
3. whether test bindings should gain explicit `scenarioKeys` for direct proof ownership;
4. exact schema/validator mechanism for document-type-aware identity and typed graph validation.