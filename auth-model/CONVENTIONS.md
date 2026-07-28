# Authentication Model Conventions

## Stable keys

Keys use lowercase kebab-case segments separated by periods.

Pattern:

`^auth\.[a-z][a-z0-9-]{1,63}(\.[a-z][a-z0-9-]{1,63})+$`

Rules:

- Keys begin with `auth.` and include a definition-type segment.
- Segments start with a lowercase letter.
- Segments contain lowercase letters, digits, and single hyphens.
- Segments do not contain consecutive hyphens or end with a hyphen.
- A key is a permanent identity and is never reused for a different meaning.

Recommended prefixes:

- `auth.dimension.*`
- `auth.invariant.*`
- `auth.action.*`
- `auth.transition.*`
- `auth.journey.*`
- `auth.scenario.*`
- `auth.conversation.*`
- `auth.presentation.*`
- `auth.decision.*`
- `auth.unresolved.*`

## Definition maturity

Allowed maturity values:

- `proposed`: discovered or drafted but not yet reviewed
- `reviewed`: reviewed for semantic correctness
- `approved`: accepted as current authored truth
- `implemented`: corresponding implementation is believed complete
- `verified`: required evidence passes against the current definition hash
- `deprecated`: retained for history but no longer current

Maturity is monotonic in normal operation except when a definition changes materially. A material change may move `implemented` or `verified` back to `reviewed` or `approved` and makes prior evidence stale.

## Definition versions

- `schemaVersion` identifies the JSON schema contract used by the file.
- `version` is a positive integer for the semantic definition.
- Increment `version` whenever behavior, applicability, requirements, guards, expected effects, or referenced truth changes materially.
- Editorial changes that do not alter meaning do not require a version increment.

## Source references

Every authored definition includes one or more source references when evidence exists.

A source reference contains:

- `sourceType`: `ddr`, `code`, `test`, `discussion`, `decision`, or `external`
- `reference`: stable repository path, DDR identifier, test symbol, decision key, or durable URL
- optional `section`
- optional `commit`
- optional `note`

Source references explain provenance. They do not automatically establish authority.

## Canonical normalization and hashing

The canonical definition hash is SHA-256 over UTF-8 encoded canonical JSON.

Canonical JSON rules:

1. Exclude the `definitionHash` property itself.
2. Preserve JSON value types exactly.
3. Sort all object properties by ordinal property name.
4. Preserve array order unless the schema explicitly declares the array unordered.
5. For unordered string/reference sets, sort by ordinal value before serialization.
6. Serialize without insignificant whitespace.
7. Use JSON literals `true`, `false`, and `null`.
8. Use invariant numeric formatting.
9. Normalize strings to Unicode NFC.
10. Do not normalize, trim, or recase authored string values except where the schema explicitly requires it.

The resulting lowercase hexadecimal digest is stored as `definitionHash` in generated/runtime projections. Authored source files may omit it and allow validation tooling to compute it.

## References

- References use stable keys only.
- All references must resolve within the current manifest unless explicitly marked external.
- Circular references are permitted only where the schema explicitly allows them.
- Journeys may reference scenarios in ordered sequence.
- Scenarios reference one action and may reference one transition.
- Conversation types may reference journeys, scenarios, actions, and routed conversation types, but cannot redefine their guards, mutations, effects, or postconditions.
- Presentation bindings reference logical actions, scenarios, conversations, views, and platforms without redefining behavioral truth.

## Conversation definitions

- A conversation type is a guided orchestration contract, not a source of authentication truth.
- Conversation turns that collect, clarify, explain, or route information are not authentication transitions.
- A conversation may select or coordinate canonical journeys based on authoritative state and declared predicates.
- State-changing behavior occurs only through referenced logical actions and deterministic transitions.
- Passwords, TOTP values, passkey material, provider tokens, recovery codes, and equivalent secrets are collected only by secure components outside the conversational context.
- Conversation state must be durable enough to support pause and resume without making the transcript authoritative.
- Guided and traditional presentations must invoke the same logical actions and produce the same post-state for equivalent valid inputs.

### Distinctive characteristics and example interactions

- `distinctiveCharacteristics` describes the qualities that make a conversation materially different from other conversation types.
- `exampleInteractions` provides illustrative stream sequences for human review, visualization, design, and future test planning.
- Example interactions do not define executable actions, transition guards, validation rules, or postconditions.
- Example entries may represent user messages, agent messages, secure interactions, controls, and user-relevant system events.
- A system event may be appended while the chapter is inactive when an authoritative business event changes something the user would reasonably expect to see later.
- System-authored stream entries must remain projections of authoritative records and should identify the authoritative reference type when known.
- Examples should demonstrate the characteristic behavior of the conversation rather than attempt to enumerate every branch.
- Internal retries, transport events, queue activity, and other implementation noise should not appear as user-facing example stream entries.

## Validation levels

### Schema validation

Every file validates against its declared JSON schema.

### Referential validation

Every internal key reference resolves to exactly one authored definition of the expected type.

### Semantic validation

At minimum:

- all definition keys are globally unique
- versions are positive integers
- deprecated definitions are not used by approved definitions unless explicitly allowed
- an action declares at least one permitted state mutation or required side effect
- a transition references exactly one action
- a scenario references exactly one action
- a scenario changes at least one state variable or produces at least one required effect
- a journey contains at least one scenario
- a conversation type does not directly define state mutations, transition guards, or authentication postconditions
- secure information requirements use secure-component collection rather than conversation collection
- presentation bindings do not introduce alternate guards or postconditions
- maturity and evidence status remain distinct concepts

### Determinism validation

For any applicable transition, identical normalized source state, action, context, and inputs must select one destination transformation and one required-effects set. Ambiguous overlapping approved transitions are invalid unless an explicit priority or mutually exclusive guard proves determinism.
