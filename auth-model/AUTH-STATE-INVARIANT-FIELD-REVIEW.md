# Authentication State and Invariant Field Review

This review applies the minimal-change normalization rules from `AUTH-MODEL-COMPATIBILITY-MAP.md` to the existing authentication state, composite-state, invariant, Action, and Transition contracts.

The default decision is **Keep**. A field is tightened only when its current meaning is ambiguous, duplicated, or not mechanically enforceable.

## Review rubric

- **Keep** - the field has one clear meaning and should remain.
- **Clarify** - the field is useful but its contract should be stated more precisely.
- **Validate** - the current shape is right; tooling/schema should enforce relationships more strongly.
- **Consolidate** - two fields appear to express the same fact; do not remove either until all consumers are traced.
- **Migrate** - the current shape cannot cleanly express the intended model. No finding in this review currently requires migration.

## State Dimension

### `key`

**Keep + Validate**

Canonical identity of the state dimension. Must be unique and use the `auth.dimension.*` namespace.

### `name`, `summary`

**Keep**

Human-facing definition metadata.

### `version`, `maturity`

**Keep**

Definition lifecycle metadata. These do not represent runtime state.

### `authority`

**Keep + Clarify**

The existing values have useful, distinct meanings:

- `canonical` - durable authentication truth;
- `session-projection` - derived runtime/session capability;
- `presentation-context` - presentation/routing context that must not redefine authentication policy.

Do not collapse these into one generic state concept.

### `valueType`

**Keep + Clarify**

The schema permits `enumeration`, `boolean`, `string`, `integer`, `timestamp`, `reference`, and `set`.

Current authored dimensions are predominantly vocabulary-style dimensions, but current usage is not sufficient reason to narrow the schema. The field should continue to describe the semantic value shape of the dimension.

### `requiredForCanonicalSnapshot`

**Keep + Validate**

This is a useful declaration of snapshot ownership. A `session-projection` or `presentation-context` dimension should not become required canonical truth without an explicit model decision.

### `values[]`

**Keep + Validate**

The values are the canonical vocabulary for enumeration-like dimensions.

Tightening rules:

1. `values[].key` must be unique within the owning dimension.
2. A value key is local to its dimension and is not a global `auth.*` definition key.
3. The same display name may not be used as an alternate identity.
4. Tooling must resolve state expressions against the owning dimension vocabulary where a machine-readable expression parser exists.

The current schema does not prevent duplicate value keys because object-array uniqueness is not keyed by `values[].key`; graph/semantic validation should enforce it.

### `values[].terminal`

**Keep + Clarify**

A terminal value is terminal for the modeled lifecycle of that dimension, not necessarily terminal for the user's entire authentication journey.

### `sourceReferences`, `definitionHash`

**Keep**

Standard provenance/freshness metadata.

## Composite State Catalog

### `regions[]`

**Keep + Validate**

Regions are organizational groupings. They do not redefine state semantics.

Tightening rules:

1. region keys must be unique;
2. every `dimensionKeys[]` reference must resolve to a State Dimension;
3. duplicate dimension keys inside one region are invalid;
4. active dimensions should normally appear in exactly one region unless an explicit exception documents intentional overlap.

### `dependencies[]`

**Keep + Validate**

Dependencies are coarse relationships between dimensions. They do not replace Invariants.

The existing relationship vocabulary is intentionally small: `constrains`, `requires`, `derives`, `selects`, `claims`, and `authorizes`.

Tightening rules:

1. `fromDimensionKey` and `toDimensionKey` must resolve to existing State Dimensions;
2. identical `(from, to, relationship)` tuples must not be duplicated;
3. a dependency is descriptive graph structure, not an executable substitute for an invariant rule.

This separation is important. For example, the composite catalog may say one dimension `requires` another while the precise legal-state rule remains in an Invariant.

## Invariant

### `key`, `name`, `summary`, `version`, `maturity`

**Keep**

Standard authored-definition identity and lifecycle metadata.

### `kind`

**Keep**

`state`, `transition`, and `side-effect` distinguish three genuinely different rule scopes.

### `severity`

**Keep**

Separates invalid model/runtime conditions from diagnostic warnings.

### `appliesWhen.expression`

**Keep + Clarify**

This is a semantic predicate, not currently a fully typed executable expression language. It states when the invariant is relevant.

Do not silently treat arbitrary expression text as machine-enforceable syntax unless an explicit parser/grammar owns that contract.

### `rule`

**Keep + Clarify**

Canonical semantic rule expressed in model vocabulary. The rule is the human-readable normative statement. Machine validation should rely on typed references and explicit executable checks where implemented rather than pretending free-form text is executable policy.

### `failureMeaning`

**Keep**

Explains why violation matters. This is useful durable context and not duplicate rule text.

### `relatedDimensionKeys[]`

**Keep + Tighten Validation**

This field promises references to State Dimensions, but the current invariant schema accepts any `auth.*` key.

The semantic contract should be exactly:

- every item uses the `auth.dimension.*` namespace;
- every item resolves to an existing State Dimension;
- duplicate references are invalid.

This is a schema/validator tightening, not a data-model redesign.

### `sourceReferences`, `definitionHash`

**Keep**

Standard provenance/freshness metadata.

## Action Relationship to State

Actions declare the legal operation boundary. They do not define one specific resulting state.

### `permittedMutations[]`

**Keep + Validate**

This is intentionally broader than a Transition's `destinationTransform`. It declares which canonical model elements an action is allowed to mutate.

Every typed mutation reference should resolve. Where the reference is intended to name a State Dimension, validators should enforce that specific type rather than merely accepting a generic `auth.*` key.

### `guards[]`

**Keep + Clarify**

These are semantic predicates describing action preconditions. As with invariant predicates, free-form expression text must not be mistaken for a fully executable policy language unless explicitly implemented as one.

### `requiredEffects[]`, `forbiddenEffects[]`

**Keep**

These are normative side-effect boundaries and are distinct from state mutation.

### `invariantKeys[]`

**Keep + Validate**

Every reference must resolve to an active Invariant. Active Actions should not depend on deprecated or missing Invariants without an explicit compatibility exception.

## Transition Relationship to State

Transitions remain the deterministic state-change records.

### `sourceState`

**Keep + Clarify**

Semantic predicate defining the applicable starting state. It complements, rather than duplicates, the referenced Action guards.

### `actionKey`

**Keep + Validate**

Must resolve to exactly one active Action.

### `guards[]`

**Keep**

Transition-specific outcome guards. They may be narrower than the Action's general legality guards.

### `destinationTransform[]`

**Keep + Validate**

This is the machine-oriented description of state mutation for the outcome.

Tightening rules:

1. every `dimensionKey` resolves to a State Dimension;
2. duplicate transform entries for the same dimension require explicit semantics or are rejected as ambiguous;
3. operations must be compatible with the referenced dimension's value shape where that can be validated;
4. referenced enumeration values should resolve to the target dimension vocabulary when the value expression is a direct canonical value.

### `requiredEffects[]`, `forbiddenEffects[]`

**Keep**

Transition-specific effect contract. These may refine the broader Action effect boundaries.

### `invariantKeys[]`

**Keep + Validate**

Every reference must resolve. The transition must not contradict its referenced Invariants.

## Repetition that is intentional

Several concepts appear at more than one layer and should **not** be consolidated merely because the strings repeat.

- Action guards define whether the logical operation is permitted.
- Transition source/guards define whether one specific deterministic outcome applies.
- Composite dependencies describe coarse relationships between dimensions.
- Invariants define legal cross-dimension rules.
- Action mutation/effect boundaries define what may happen.
- Transition transforms/effects define what did happen for one modeled outcome.

These are independent conformance assertions and provide useful cross-checks.

## Concrete tightening candidates

No contract rename or model migration is justified by this review. The useful next changes are validator/schema-strengthening candidates:

1. enforce unique `StateDimension.values[].key` values;
2. enforce unique composite region keys;
3. resolve all region/dependency dimension references;
4. detect duplicate composite dependency tuples;
5. narrow `Invariant.relatedDimensionKeys[]` semantically to `auth.dimension.*` and resolve them;
6. resolve Action/Transition `invariantKeys[]`;
7. resolve Transition `actionKey` and `destinationTransform[].dimensionKey`;
8. validate direct transition enumeration values against their owning dimensions where possible;
9. detect ambiguous duplicate transforms against one dimension;
10. keep free-form predicate/rule text explicitly semantic unless and until an executable grammar is deliberately introduced.

## Compatibility rule

These tightenings must be introduced without renaming existing JSON fields, schema files, definition roots, or canonical keys unless a separate compatibility review proves every C#, Aptix, and runtime consumer has been updated deliberately.

The target remains the current model with fewer opportunities for drift, not a replacement model.
