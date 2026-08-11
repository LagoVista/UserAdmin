# AuthView and AuthRoute Field Review

## Purpose

This document reviews the existing AuthView and AuthRoute JSON contracts as they are implemented today.

The goal is not to redesign them. The working assumption is that the current contracts are substantially correct and should be preserved. The review identifies where meaning should be clarified, where validation should be tightened, and where duplicated expressions may eventually be consolidated after all consumers are understood.

Use the following classifications:

- **Keep** - the field has a clear durable purpose and current consumers rely on its meaning.
- **Clarify** - the field is useful, but its semantic boundary should be documented more precisely.
- **Validate** - keep the field and add stronger local or graph validation.
- **Consolidate** - two or more fields appear to state the same fact; do not remove either until compatibility is proven.
- **Migrate** - the current shape cannot cleanly express the intended contract. This classification requires concrete evidence and is intentionally rare.

A field rename, schema rename, directory rename, or removal is a contract change and must be checked against C#, Aptix, Angular, React Native, authored JSON, and runtime identity/evidence consumers before it is performed.

## AuthView

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `$schema` | Declares the JSON Schema used to validate the authored document. | **Keep / Validate** | Schema path is part of the authored contract. Do not rename casually. |
| `schemaVersion` | Identifies the JSON contract version. | **Keep** | Distinct from semantic/runtime identity. Consumers may branch on this in the future even where they do not today. |
| `viewId` | Canonical identity of the AuthView. | **Keep / Validate** | Must be unique among AuthViews. It is never interchangeable with `routeId`. C# resolves AuthViews by this field; Aptix Auth Views also loads views explicitly by this field. |
| `name` | Human-readable display name. | **Keep** | Not an identity and should never be used for references. |
| `description` | Human-readable explanation of the surface. | **Keep** | Optional descriptive material. |
| `category` | Broad presentation-family classification such as `entry`, `password`, `recovery`, or `passkey`. | **Keep / Clarify** | This is not the behavior-category key used by Behavior/Scenario documents. The current name is acceptable if this distinction is documented and tooling treats it only as presentation grouping. No rename is required. |
| `routeId` | Typed reference from an AuthView to its canonical AuthRoute when the view is routable. | **Keep / Validate** | Must resolve against AuthRoute identity, never AuthView identity. For `routeType: view`, graph validation should confirm the route points back to the same `viewId`. |
| `status` | Current authored lifecycle/currentness of this presentation definition (`proposed`, `active`, `deprecated`, `retired`). | **Keep / Clarify** | This is the existing AuthView lifecycle contract. Do not replace it merely to align with another document family's `maturity` vocabulary. Deprecated/retired definitions are historical rather than active reconciliation members. |
| `controls[]` | Canonical controls owned by the AuthView. | **Keep / Validate** | Validate unique `id` values and unique semantic finders within a view. Scenario inputs should resolve to compatible declared controls. |
| `controls[].id` | Stable semantic identity of a control within its owning AuthView. | **Keep / Validate** | View-scoped, not globally keyed. Must not be inferred from display name. |
| `controls[].finder` | Platform-neutral executable locator contract for a control. | **Keep / Validate** | Angular and React Native implementations conform to this semantic value. Validate finder uniqueness and compatible namespace/control type where practical. |
| `controls[].controlType` | Declares the semantic kind of UI control. | **Keep / Validate** | Useful for implementation reconciliation and input compatibility validation. |
| `controls[].required` | Declares required input/presentation semantics. | **Keep / Clarify** | Describes canonical UI semantics, not necessarily HTML/native implementation mechanics. |
| `controls[].sensitivity` | Declares information sensitivity. | **Keep** | Important for credentials/secrets and durable security review. |
| `controls[].visibilityCondition` | Human-readable condition under which the control is expected to be visible. | **Keep / Clarify** | Currently prose guidance, not an executable expression language. Tooling must not pretend it is a machine predicate. |
| `actions[]` | Canonical user-visible actions owned by the AuthView. | **Keep / Validate** | Scenario actions should resolve by `id` and finder against the starting AuthView. |
| `actions[].id` | Stable semantic identity of a view action within its AuthView. | **Keep / Validate** | View-scoped identity. It is not automatically a global `auth.action.*` logical action. |
| `actions[].finder` | Platform-neutral executable locator for the view action. | **Keep / Validate** | Must resolve consistently in Angular/RN implementations and runnable scenarios. |
| `actions[].actionType` | Presentation form of the affordance (`button`, `link`, `native-capability`). | **Keep** | Presentation fact, separate from canonical server/logical action semantics. |
| `actions[].visibilityCondition` | Human-readable visibility guidance for the action. | **Keep / Clarify** | Descriptive, not an executable policy expression. |
| `platforms.web` | AuthView implementation projection for the Web/Angular implementation. | **Keep / Clarify / Validate** | Runtime Web execution is separate from implementation reconciliation. Existing field name is stable and consumed by Aptix. |
| `platforms.mobile` | AuthView implementation projection for the shared mobile/React Native implementation. | **Keep / Clarify / Validate** | One RN implementation may later be executed separately on iOS and Android. Do not duplicate authored projection records merely to mirror runtime platforms. |
| `platforms.*.status` | Implementation completeness of that projection. | **Keep** | Distinct from AuthView authored `status` and from runtime pass/fail. |
| `platforms.*.repository/path/component/implementation` | Provenance/location of the platform implementation. | **Keep / Validate** | Prefer these fields as the detailed implementation location because they live with projection conformance. |
| `platforms.*.conformance` | Reviewed implementation-conformance record. | **Keep / Tighten validation** | Useful existing structure. A later tightening may add freshness information only if needed; do not redesign it preemptively. |
| `source.runtimeEntityId` | Compatibility identity used by the C# scenario hydration layer for runtime AuthView headers. | **Keep / Clarify** | Not removable today. C# consumes it and falls back to a different deterministic ID when absent, which would change existing runtime identity. |
| `source.runtimeSha256Hex` | Historical/runtime projection hash associated with the AuthView. | **Keep pending consumer review** | No active non-authored consumer has yet been demonstrated in this review. Do not remove until runtime/evidence provenance is fully traced. |
| `source.webComponent` | Convenience reference to Web component identity. | **Consolidate candidate** | Often duplicates `platforms.web.component` or `platforms.web.implementation`. No active Aptix/C# reader has been identified, but removal still requires full compatibility review. |
| `source.mobileComponent` | Convenience reference to mobile/RN implementation identity. | **Consolidate candidate** | Often duplicates `platforms.mobile.component` or `platforms.mobile.implementation`. Preserve until compatibility review is complete. |
| `notes[]` | Human review notes and reconciliation context. | **Keep** | Useful provenance when the note does not belong in a stronger typed field. |

## AuthRoute

| Field | Semantic role | Classification | Review notes |
| --- | --- | --- | --- |
| `$schema` | Declares the route JSON Schema. | **Keep / Validate** | Treat schema path changes as contract changes. |
| `schemaVersion` | JSON contract version. | **Keep** | Do not conflate with route identity. |
| `routeId` | Canonical identity of the AuthRoute. | **Keep / Validate** | Must be unique among AuthRoutes. Aptix must load this field explicitly for AuthRoute documents. |
| `name` | Human-readable route name. | **Keep** | Never use as identity. |
| `description` | Human-readable description. | **Keep** | Optional descriptive material. |
| `path` | Canonical authored route path. | **Keep / Validate** | Platform route projections should reconcile to this canonical path or document the platform-specific mapping explicitly. |
| `routeType` | Semantic route role (`view`, `handler`, `redirect`, `entry`, `logout`). | **Keep / Validate** | Enables graph rules, especially whether `viewId` is expected. |
| `viewId` | Typed reference from a route to its AuthView when the route renders a view. | **Keep / Validate** | Never route identity. For `routeType: view`, it should resolve to an AuthView that points back through `routeId`. |
| `parameters[]` | Canonical route parameter contract. | **Keep / Validate** | Validate uniqueness and platform projection conformance. |
| `parameters[].id` | Route-local parameter identity. | **Keep / Validate** | Route-scoped. |
| `parameters[].required` | Whether the route requires the parameter. | **Keep** | Canonical route semantics. |
| `parameters[].sensitivity` | Information sensitivity of the parameter. | **Keep** | Important for security review and avoiding unsafe transport. |
| `status` | Current authored lifecycle/currentness of the route definition. | **Keep / Clarify** | Existing route lifecycle contract. No need to replace merely for vocabulary consistency with unrelated document types. |
| `commonLinks` | Projection of the canonical AuthRoute into the CommonLinks/server route surface. | **Keep / Validate** | This has a distinct job and is not the route's identity. It should reconcile path/member/value against the canonical route. |
| `platforms.web` | Web/Angular route projection. | **Keep / Clarify / Validate** | Implementation projection, not runtime Web pass/fail. |
| `platforms.mobile` | shared mobile/RN route projection. | **Keep / Clarify / Validate** | Implementation projection, not separate iOS/Android runtime records. |
| `platforms.*.status` | Completeness of that route projection. | **Keep** | Separate from authored route `status`. |
| `platforms.*.repository/path/route/component/implementation` | Platform implementation provenance/location. | **Keep / Validate** | Natural home for route projection details. |
| `platforms.*.conformance` | Reviewed conformance of the implementation route to canonical route semantics. | **Keep / Tighten validation** | Existing `checkedAgainst` vocabulary is useful. |
| `source.commonLinksMember` | Convenience CommonLinks member reference. | **Consolidate candidate** | May duplicate `commonLinks.member`; preserve until consumers are proven not to rely on it. |
| `source.angularComponent` | Convenience Angular component reference. | **Consolidate candidate** | May duplicate `platforms.web.component` / `implementation`. |
| `source.mobileRoute` | Convenience RN/mobile route reference. | **Consolidate candidate** | May duplicate `platforms.mobile.route` / `implementation`. |
| `notes[]` | Human review notes and reconciliation context. | **Keep** | Appropriate for context not represented by typed fields. |

## Existing graph invariants to tighten

These rules do not require a new JSON shape.

1. AuthView documents are indexed by `viewId`; AuthRoute documents are indexed by `routeId`. Loaders must be told the document type and must never infer identity from whichever property happens to be present.
2. Every active routable AuthView with `routeId` must resolve to an AuthRoute.
3. Every active `routeType: view` AuthRoute with `viewId` must resolve to an AuthView.
4. For a paired view route, `AuthView.routeId` and `AuthRoute.viewId` must agree bidirectionally.
5. Control IDs must be unique within a view.
6. Action IDs must be unique within a view.
7. Control/action semantic finders must be unique within the applicable view where ambiguity would make automation nondeterministic.
8. Scenario `startViewKey` and auth-valued `expectedViewKey` must resolve to canonical AuthViews.
9. Scenario `action.id` and `action.finder` must resolve to the same action on the scenario's starting AuthView.
10. Scenario input finders must resolve to compatible controls on the applicable AuthView.
11. Deprecated definitions remain available as history but do not participate in active readiness/runnable rollups.
12. Projection conformance and runtime execution are separate. An implementation may be reconciled while its latest Web/iOS/Android runtime execution is failing or unevaluated.

## Concrete duplicate-expression example

The current Password Sign-In AuthRoute `auth.route.continue.email.password` contains both:

- `platforms.web.implementation: EmailLoginPasswordComponent`
- `source.angularComponent: EmailLoginPasswordComponent`

and both:

- `platforms.mobile.implementation: app/auth/continue/email/password.tsx`
- `source.mobileRoute: /auth/continue/email/password`

These fields are not automatically deleted. The review marks them as consolidation candidates because the projection records are the stronger typed home for implementation provenance. We will first prove whether any C#, Aptix, generator, runtime, or external consumer still depends on the `source.*` convenience fields.

## Compatibility observations already confirmed

### C# scenario hydration

`AppUserTestingDslRepo` currently depends on:

- the `scenarios-v2/` and `auth-views/` directory roots;
- scenario `key`, `runtimeEntityId`, `startViewKey`, `expectedViewKey`, action ID/finder, and input finders;
- AuthView `viewId`;
- AuthView `source.runtimeEntityId` when present.

Therefore these names/locations are compatibility-sensitive.

### Aptix Auth Implementation panel

The panel currently depends on the category/behavior/scenario/view/route/test-binding directory roots and current property names. Its generic identity inference for AuthView/AuthRoute is a tooling defect: it must explicitly load AuthViews by `viewId` and AuthRoutes by `routeId` rather than forcing the JSON model to accommodate guessing.

### Aptix Auth Views panel

The panel has explicit TypeScript contracts for current AuthView/AuthRoute fields including `viewId`, `routeId`, `category`, `status`, `platforms`, `source`, controls, and actions. A field rename is therefore a coordinated consumer change, not a documentation-only cleanup.

## Current recommendation

Do not create a replacement AuthView/AuthRoute contract simply for aesthetic consistency.

The existing contracts should be tightened in this order:

1. document field semantics and ownership;
2. fix document-type-aware loading in Aptix;
3. strengthen graph validation around existing fields;
4. reconcile Password Sign-In against Angular and React Native using the existing field vocabulary;
5. identify duplicated expressions that have no remaining consumers;
6. consolidate only those proven duplicates with an explicit compatibility migration.

This keeps the durable model opinionated and unambiguous without turning normalization into a green-field redesign.