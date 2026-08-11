# Authentication Presentation Element Contract

## Purpose

This document defines the normalized contract for AuthView controls, AuthView actions, semantic finders, view identity, visibility guidance, scenario presentation assertions, and the remaining role of presentation bindings.

It refines `AUTH-MODEL-CONTRACT.md` and `AUTH-VIEW-ROUTE-V2-DESIGN.md` without changing existing authored JSON yet.

The goal is to preserve the useful semantics already captured in AuthViews and scenarios while removing duplicated or ambiguous presentation metadata before the v2 schemas are authored.

## View identity

An AuthView is identified only by its canonical `viewId`.

Runtime clients expose that identity through the existing screen-root contract:

- root locator: `[data-testid="auth-screen"]`
- semantic identity attribute: `data-screen-id`
- semantic identity value: canonical `viewId`

A unique `screen:*` finder is therefore not required to identify an AuthView.

### Rules

1. `viewId` is the semantic screen identity.
2. Angular and React Native implementations expose the canonical `viewId` through the shared screen-root contract.
3. Runtime runners observe `data-screen-id` and compare it to the expected canonical view identity.
4. A `screen:*` finder must not duplicate the role of `viewId` in the normalized model.
5. The `screen` finder namespace should be removed from v2 unless a separate concrete use case is discovered during migration.

## Control identity

A control is owned by exactly one AuthView.

Its canonical local identity is:

`<viewId> + controls[].id`

The local `id` is stable within the owning AuthView. The same local ID may appear on another view when the local concept is intentionally the same.

Examples:

- `auth.continue.email` + `email`
- `auth.continue.email.password` + `password`
- `auth.continue.email.password` + `validation-error`

### Control finder

The control `finder` is the executable semantic locator used by Angular, React Native, scenarios, and runtime automation.

The finder is not a second independent identity. It is an executable projection of the control identity and semantic finder kind.

Normalized rules:

1. the finder suffix must equal the control `id`;
2. canonical control finder kinds are `field`, `label`, `status`, and `display`;
3. input-like controls use `field:<id>`;
4. status controls use `status:<id>`;
5. labels use `label:<id>`;
6. non-input observable presentation values use `display:<id>`;
7. a control finder that points to a different local ID is invalid;
8. a platform implementation may use additional technical selectors internally, but it must expose the canonical semantic finder.

The current scenario schema also accepts `control:<id>` for inputs even though AuthView finders do not define a `control` namespace. The normalized scenario schema should remove that ambiguity and resolve scenario inputs through canonical AuthView control finders.

## Action identity

An AuthView action is a user-invokable affordance owned by exactly one AuthView.

Its canonical local identity is:

`<viewId> + actions[].id`

Examples:

- `auth.continue.email` + `continue`
- `auth.continue.email.password` + `sign-in`
- `auth.sign-in.unable` + `try-again`

The local AuthView action is not the same thing as a globally canonical `auth.action.*` authentication action.

A local action may:

- invoke a canonical authentication action;
- perform UI-only navigation;
- invoke a native capability;
- perform another presentation operation that does not redefine authentication behavior.

### Action finder

Every user-invokable AuthView action uses:

`action:<action-id>`

Normalized rules:

1. `actions[].finder` must equal `action:<actions[].id>`;
2. AuthView action IDs are scoped to the owning view;
3. scenarios invoke the AuthView action on their `startViewKey`;
4. scenario `action.id` and `action.finder` must resolve to the same declared AuthView action;
5. Angular and React Native must expose the same canonical action finder for equivalent semantic actions.

Keeping both `id` and `finder` is intentional: `id` provides semantic graph identity while `finder` provides the executable cross-platform locator. Validators enforce their deterministic relationship.

## Control type and finder kind

`controlType` describes presentation semantics. Finder kind describes how the element participates in deterministic automation.

They are related but are not identical concepts.

For example, `text-input`, `email-input`, `password-input`, and `code-input` all normally use the `field` finder kind.

The v2 schema should validate compatible control-type/finder-kind combinations rather than infer control identity from `controlType` alone.

## Visibility semantics

The current AuthView property `visibilityCondition` is human-readable prose. Existing examples include conditions such as a form being valid for submission or an error being present after rejection.

That information is useful, but it is not currently a machine-evaluable predicate and must not be treated as one.

### Normalized rule

The v2 AuthView schema should rename this descriptive property to `visibilityGuidance` or another name that clearly communicates non-executable guidance.

Rules:

1. visibility guidance documents when a control/action should normally be visible;
2. it may be used during human implementation reconciliation;
3. it is not evaluated by the scenario runner;
4. it does not replace scenario-specific expected visibility assertions;
5. if executable visibility predicates are needed later, they must be represented by a separate typed condition/reference contract rather than encoding a private expression language inside free-form strings.

Existing v1 `visibilityCondition` values are preserved during migration and translated without semantic loss.

## Scenario inputs

Scenario inputs bind to controls on the scenario's starting AuthView.

Normalized rules:

1. every input finder must resolve to exactly one compatible control on `startViewKey`;
2. the finder suffix resolves the local control ID;
3. an input finder must use an input-capable finder kind, normally `field`;
4. a scenario must not invent a finder not declared by the canonical AuthView;
5. value source/type semantics remain owned by the scenario because they describe execution data rather than presentation identity.

## Scenario action

A scenario invokes exactly one AuthView action.

Normalized validation resolves:

`Scenario.startViewKey -> AuthView -> actions[] -> Scenario.action`

Both `action.id` and `action.finder` must match the same AuthView action.

This is intentionally redundant at authoring time because it gives both a human-readable semantic action identity and a directly executable locator. The validator prevents those values from diverging.

## Expected visible elements

The current Scenario V2 field `expectedVisibleFinders[]` is valuable because it describes runtime-observable presentation outcomes directly.

It should remain finder-oriented rather than being replaced by display names or local IDs.

However, normalized graph validation must make it typed rather than accepting arbitrary strings.

Rules:

1. when `expectedViewKey` resolves to an AuthView, every expected visible finder must resolve to a control or action declared by that expected AuthView;
2. action finders resolve against `actions[]`;
3. field, label, status, and display finders resolve against `controls[]`;
4. duplicate expected finders are invalid;
5. expected-visible assertions do not imply that every declared AuthView element must be visible;
6. conditional controls/actions are asserted only in scenarios whose state makes them expected;
7. when `expectedViewKey` points outside the authored AuthView domain, such as an application-owned destination, AuthView element resolution is not required unless that external surface has its own typed contract.

This turns `expectedVisibleFinders[]` from a loose string list into a validated executable assertion without changing its useful shape.

## AuthView action versus logical authentication action

The model intentionally contains two action layers:

### AuthView action

A local user-facing affordance such as:

- `sign-in`
- `continue`
- `forgot-password`
- `try-again`

It is scoped to an AuthView and has an `action:*` finder.

### Logical authentication action

A globally keyed state-changing operation such as `auth.action.*`.

It owns authentication semantics and participates in transitions.

A view action may bind to a logical action, but their identifiers must not be conflated.

UI-only navigation actions legitimately have no logical authentication action because moving between screens does not necessarily mutate authentication state.

## Presentation binding normalization

Presentation bindings were introduced when runtime AuthView child identities were not yet stable. They provided a durable bridge among route/view evidence, semantic finders, UI operations, canonical actions, and UI-only navigation.

The V2 scenario/AuthView/AuthRoute graph now owns much of that information directly.

Therefore presentation bindings should be retained only when they add a relationship that is not already canonical elsewhere.

### Information already owned elsewhere

The normalized model already owns:

- source view: Scenario `startViewKey`;
- invoked local action: Scenario action + AuthView action;
- expected view: Scenario `expectedViewKey`;
- source/destination route identity: AuthView/AuthRoute graph;
- semantic finder: AuthView control/action;
- platform component/route provenance: Angular/React Native implementation projection;
- runtime platform requirement: Scenario `evidenceRequirements`.

A presentation binding should not duplicate those values merely to repeat them per platform.

### Relationships a binding may still add

A normalized binding is justified when it explicitly connects a local AuthView action to something outside the view/scenario presentation graph, for example:

- a canonical `auth.action.*` logical action;
- an implementation operation/handler that is not otherwise represented by the implementation projection;
- a distinct UI-navigation semantic relationship when that relationship remains useful independently of the scenario.

The current presentation-binding schema should therefore be reviewed after the AuthView/AuthRoute v2 schemas are finalized. It should not drive the new presentation model backward toward duplicated platform-specific data.

## Validation invariants

For active normalized definitions:

1. AuthView controls have unique local IDs.
2. AuthView actions have unique local IDs.
3. AuthView control finders are unique within the view.
4. AuthView action finders are unique within the view.
5. control finder suffix equals control ID.
6. action finder equals `action:<action-id>`.
7. scenario inputs resolve to canonical start-view controls.
8. scenario action ID and finder resolve to the same canonical start-view action.
9. expected visible finders resolve against the expected AuthView when applicable.
10. Angular and React Native implementations expose the same semantic identities/finders.
11. `viewId` is observed through `data-screen-id`; no duplicate screen identity system is introduced.
12. descriptive visibility guidance is never evaluated as authentication or navigation truth.

## Migration notes

The v1 data already contains strong semantic finders and local control/action IDs. Migration should preserve those values wherever they already agree.

The migration process should report mismatches before rewriting them. A mismatch is evidence of drift that needs reconciliation, not permission for tooling to silently choose one side.

Password Sign-In remains the first normalization specimen. Its shared Welcome, Continue with Email, Sign In with Password, Rejected, and Locked Out surfaces provide enough controls, actions, navigation, server interaction, and expected-visible assertions to validate this contract before applying it broadly.
