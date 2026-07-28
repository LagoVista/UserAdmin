# Section 4.3 Handoff

Status: Complete

Scope completed:

- Added an explicit composite-state catalog that groups the 17 dimensions into seven logical regions.
- Added cross-dimension dependency edges for identity, workspace, invitation, claim, account, MFA, recovery, session, and presentation relationships.
- Authored the twenty foundational invariants from `AUTH-MODEL-GUIDE.md`.
- Separated state, transition, and side-effect invariants.
- Added catalog schemas for composite-state relationships and invariant collections.

Decisions made:

- Dimension definitions remain the vocabulary layer.
- `composite-state-catalog.json` owns region membership and descriptive dependency edges.
- Invariants own legal and illegal cross-dimension combinations.
- Transitions will own deterministic changes across dimensions.
- Session Capability remains a derived projection constrained by canonical account, MFA, recovery, and workspace state.
- Presentation Context selects an adapter and cannot redefine authorization or post-state behavior.
- Catalog files are allowed where a definition family benefits from one coherent reviewed set; each embedded invariant still has its own stable key and semantic version.

Invariants added or changed:

- Twenty foundational invariants were added in `invariants/foundational-invariants.json`.
- They cover home workspace, workspace access and switching, guest value, PendingIdentity, invitation resolution, email identity distinctions, duplicate prevention, transition atomicity and determinism, presentation equivalence, disabled accounts, and recovery.

Definitions created or updated:

- `state/composite-state-catalog.json`
- `invariants/foundational-invariants.json`
- `schemas/composite-state-catalog.schema.json`
- `schemas/invariant-catalog.schema.json`
- `model-manifest.json`

Implementation files inspected or changed:

- No implementation files changed.
- Existing `PendingIdentity` semantics remain an implementation source for PendingIdentity invariants.

Scenarios completed:

- None. Scenario authoring begins with Section 4.4.

Evidence completed:

- Definition structure and references were reviewed manually.
- Automated JSON Schema and semantic validation tooling remains future work.

Known DDR conflicts:

- None resolved in this section.

Known implementation conflicts:

- Existing organization behavior may not yet satisfy exactly-one-home-workspace and registration-completes-in-home-workspace invariants.
- Existing session routing may not yet satisfy guest access and disabled-account projection invariants.

Open questions:

- Exact predicate expression language for future executable invariant evaluation.
- Whether catalogs will later be mechanically split into one file per invariant for tooling or review ergonomics.
- Exact effect-key taxonomy for atomicity, idempotency, and user-visible workspace switching.

Completion criteria remaining:

- None for the authored Section 4.3 foundation. Implementation verification comes in later sections.

Recommended next action:

- Begin Section 4.4, Guest and claimable-value flow.
- Inspect current guest/session/chat ownership implementation before authoring actions, transitions, scenarios, and the first journey.