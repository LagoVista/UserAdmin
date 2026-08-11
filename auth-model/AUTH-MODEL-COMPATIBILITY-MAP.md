# Authentication Model Compatibility Map

## Purpose

This document is a safety map for the ongoing authentication-model review.

The authentication model is mature and already expresses most of the required domain. The current effort is a **methodical tightening of the existing model**, not a green-field redesign and not an attempt to create a generic authentication-modeling framework.

The preferred order of work is:

1. understand the existing semantic intent;
2. identify duplicate expressions, ambiguous ownership, dangling references, or consumer assumptions;
3. clarify the contract where possible;
4. tighten validation where possible;
5. change an existing JSON contract only when the current shape cannot express the intended meaning without ambiguity.

A slightly opinionated existing shape with one precise meaning is preferred over a more abstract replacement.

## Change test

Before changing a JSON property name, schema filename, directory name, identity field, or reference shape, answer all of the following:

- What does the existing field mean today?
- Who owns it?
- Which authored documents use it?
- Which C# code consumes it?
- Which Aptix code consumes it?
- Which Angular or React Native implementation depends on it?
- Does runtime projection/evidence depend on it?
- Can the ambiguity be removed through documentation or validation instead of migration?
- If a migration is still required, can compatibility be preserved while consumers move together?

No contract rename should be considered isolated until this dependency check is complete.

## Compatibility classifications

Use these classifications during review:

- **Keep** - one clear meaning already exists; no contract change needed.
- **Clarify** - shape is adequate but meaning or ownership needs documentation.
- **Validate** - shape is adequate but tooling must stop guessing and enforce the existing contract.
- **Consolidate** - two or more fields express the same fact and create drift; preserve one authoritative expression after consumer analysis.
- **Migrate** - current shape cannot represent the intended contract cleanly; coordinated authored/C#/Aptix/client migration is justified.

`Migrate` is the last resort, not the default destination.

## Current consumer map

### Auth model authored JSON

Current structural roots include:

- `behavior-category-catalog.json`
- `behaviors-v2/`
- `scenarios-v2/`
- `auth-views/`
- `auth-routes/`
- `actions/`
- `transitions/`
- `presentation/`
- `implementation/`
- `schemas/`

`model-manifest.json` records these roots and schema locations, but existing consumers do not uniformly resolve their paths through the manifest. Several consumers currently use literal directory names.

**Compatibility implication:** renaming a root directory requires consumer changes even if `model-manifest.json` is updated.

### C# canonical scenario loader

`AppUserTestingDslRepo` currently loads the Git archive directly from `master` and uses literal path markers:

- `/auth-model/scenarios-v2/`
- `/auth-model/auth-views/`

It hydrates scenarios from these current properties:

- `key`
- `runtimeEntityId`
- `name`
- `summary`
- `startViewKey`
- `expectedViewKey`
- `action.id`
- `action.finder`
- `inputs[].finder`
- `inputs[].name`
- `inputs[].value`
- `preconditions.state`
- `postconditions.state`
- `expectedAuthLogEvents`

It hydrates the AuthView map from:

- `viewId`
- `name`
- optional `source.runtimeEntityId`

If `source.runtimeEntityId` is absent, the loader deterministically derives an entity ID from `viewId`.

**Compatibility implications:**

- scenario-property renames are direct C# contract changes;
- `viewId` is a direct C# contract;
- removal of `source.runtimeEntityId` may change runtime entity identity for existing views whose persisted ID differs from the deterministic fallback;
- schema filenames themselves are not currently used by this loader;
- deprecated scenarios are currently discovered from the directory by path/extension and require explicit filtering if they should be excluded from active runtime inventory.

### Aptix Authentication Implementation panel

`AuthImplementationPanel` currently watches and loads literal roots:

- `behaviors-v2`
- `scenarios-v2`
- `auth-views`
- `auth-routes`
- `implementation/tests`

It relies directly on current properties including:

- `key`
- `categoryKey`
- `scenarioKeys`
- `viewId`
- `routeId`
- `startViewKey`
- `expectedViewKey`
- `action.id`
- `action.finder`
- AuthView `actions[]`
- `serverInteraction.required`
- `serverInteraction.transitionKeys`
- test-binding `scenarioKeys`
- test-binding `transitionKeys`
- authored `progress`
- scenario `evidenceRequirements`

The current generic loader attempts to infer identity by checking identifier-like properties. This is the source of the recent AuthView/AuthRoute identity defect because both document types can contain both `viewId` and `routeId`.

**Compatibility implication:** this is primarily a **Validate** problem. The existing JSON model already distinguishes AuthView identity (`viewId`) from AuthRoute identity (`routeId`). The loader should be made document-type-aware rather than redesigning those documents.

### Aptix Authentication Views panel

`AuthViewsPanel` is directly typed against the current AuthView/AuthRoute shape.

AuthView fields include:

- `viewId`
- `name`
- `description`
- `category`
- `routeId`
- `status`
- `controls[]`
- `actions[]`
- `platforms`
- `source`
- `notes`

AuthRoute fields include:

- `routeId`
- `name`
- `path`
- `routeType`
- `status`
- `viewId`
- `platforms`
- `source`
- `notes`

The panel also currently exposes platform filtering using `web` and `mobile`.

**Compatibility implication:** renaming `category`, `status`, `platforms.web`, `platforms.mobile`, `source`, `viewId`, or `routeId` is a coordinated Aptix contract change. These names should not be changed merely because another name might be more elegant.

### Aptix general authentication-model loader

`AuthModelLoader` locates the model through the literal filename:

- `auth-model/model-manifest.json`

It then loads several model areas from literal roots such as:

- `state/`
- `invariants/`
- `transitions/`
- `conversations/`

The current loader reads the manifest as model metadata but does not use the manifest as a universal indirection layer for all file roots.

**Compatibility implication:** `model-manifest.json` and directory names are part of the practical consumer contract today, even where the manifest also records the same paths.

### Angular authentication UI

The current Angular authentication implementation uses the existing semantic test contract directly in markup.

For the password screen, the implementation exposes:

- root `data-testid="auth-screen"`
- `data-screen-id="auth.continue.email.password"`
- `field:email`
- `field:password`
- `status:validation-error`
- action finders such as `action:sign-in`, `action:cancel`, `action:forgot-password`, and `action:start-over`

**Compatibility implication:** canonical view IDs and semantic finders are already implementation contracts. Changing them is not merely a JSON refactor.

A mismatch between authored controls/actions and current Angular markup should normally be treated as a reconciliation finding first, not as evidence that the canonical shape needs redesign.

### React Native authentication UI

React Native maintains shared semantic authentication test IDs that include canonical-looking view IDs, fields, labels, and actions.

This is valuable because it demonstrates the same basic existing contract is already projected into the mobile implementation.

Some current values show historical naming drift, which should be reconciled against the canonical authored identifier rather than solved by adding another identifier vocabulary.

**Compatibility implication:** prefer one canonical semantic identity and bring the implementation projection into conformance where appropriate.

## Schema filename policy

Schema filenames are referenced by authored `$schema` values and by `model-manifest.json` schema metadata.

Current C# scenario hydration and the two inspected Aptix presentation panels do not appear to use schema filenames to deserialize their documents. That does **not** make schema renames free.

Before renaming a schema file:

1. update every authored `$schema` reference;
2. update `model-manifest.json` schema metadata;
3. search C#, Aptix, CI/scripts, documentation, and external tooling for the filename;
4. decide whether old schema locations need to remain as compatibility aliases during migration.

Default: **do not rename a schema file unless the existing name itself creates a real semantic problem.**

## Field-name policy

Field names are durable contracts once C#, Aptix, or client projections consume them.

A field should not be renamed simply to improve terminology when:

- its existing meaning can be documented precisely;
- its type already makes the reference target clear;
- consumers already depend on it;
- a validator can remove ambiguity without changing the shape.

Prefer tightening the resolver over renaming the reference.

Examples:

- Keep `AuthView.viewId` as AuthView identity.
- Keep `AuthRoute.routeId` as AuthRoute identity.
- Treat `AuthView.routeId` strictly as an AuthRoute reference.
- Treat `AuthRoute.viewId` strictly as an AuthView reference.
- Fix loaders that infer identity rather than renaming either field.

## Current review posture on earlier design ideas

Earlier normalization notes explored possible v2 presentation schemas and property renames. Those notes are useful design exploration, but **they are not authorization to redesign the current contracts**.

For the current review:

- `web` / `mobile` remain valid existing implementation projection names unless a concrete defect requires migration;
- AuthView `category` remains valid unless its overloading causes an actual unresolved ambiguity that documentation/type context cannot fix;
- AuthView/AuthRoute `status` remains valid unless overlapping lifecycle semantics cause a concrete correctness problem;
- `source` metadata remains in place until every consumer and runtime-identity implication is understood;
- finder vocabulary remains in place unless an entry is proven unused and removal creates measurable value;
- adding new cross-reference fields is considered only when the relationship cannot be reliably derived or validated from existing canonical references.

Any future contract change must be justified by a concrete finding in the semantic/consumer map.

## Preferred tightening targets

Based on the current dependency trace, the highest-value low-churn targets are:

1. make Aptix loaders document-type-aware so AuthView and AuthRoute identity are never inferred;
2. enforce bidirectional AuthView/AuthRoute reference integrity using the existing fields;
3. exclude deprecated definitions from active roll-ups/runnable inventories while preserving them historically;
4. validate scenario actions and input finders against the existing AuthView controls/actions;
5. reconcile client implementation drift against the existing canonical IDs/finders;
6. clarify authored progress versus C# Flow versus UI Runtime without changing the authored JSON execution boundary;
7. inspect duplicated provenance/reference fields and remove only genuinely redundant expressions after consumer analysis.

These changes strengthen the model primarily by making its existing meaning enforceable.

## Long-term rule

The authentication model should have **one authoritative expression for each important fact**.

Durability comes from:

- stable identities;
- typed references;
- explicit ownership;
- minimal semantic aliases;
- validators that never guess;
- downstream implementations that reconcile to canonical truth;
- compatibility-aware evolution when a real migration is necessary.

The goal is not the most abstract model. The goal is that a maintainer years from now can inspect any field and determine exactly what it means, what it references, and who depends on it.