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

## Next decisions

The next contract decisions should be resolved before schema or authored-data mutation:

1. relationship between definition `maturity` and authored reconciliation progress;
2. precise meaning of authored `tests: complete` versus C# Flow execution evidence;
3. active versus deprecated definition participation in graph validation and roll-ups;
4. schema strategy for the `mobile` -> `reactNative` projection rename;
5. whether implementation reconciliation needs an authored definition/version/hash receipt per projection.