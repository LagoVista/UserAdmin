# Section 4.2 Handoff

Status: Complete

Scope completed:

- Defined 15 required canonical dimensions that compose identity state.
- Defined Session Capability as a derived session projection.
- Defined Presentation Context as a non-authoritative interaction-channel projection.
- Established exact snapshot versus partial predicate semantics.
- Established that invalid cross-dimension combinations are expressed through invariants rather than a giant state enumeration.
- Established normalized canonical snapshot fingerprinting by stable dimension key.

Decisions made:

- Identity remains one singular logical state even though it is represented by multiple dimensions.
- Pending Identity Status and Pending Identity Flow are separate dimensions.
- Home Workspace, Current Workspace, Workspace Membership, and Invitation Status are separate facts.
- Email ownership is distinct from asserted, provider-supplied, and invited email values.
- Session capability is derived and excluded from the canonical identity fingerprint.
- Presentation context never changes legal actions, guards, effects, or post-state.

Invariants added or changed:

- No authored invariant definitions were added in this section.
- `state/README.md` records cross-dimension combinations that Section 4.3 must formalize as invariants.

Definitions created or updated:

- 17 state dimension definitions under `auth-model/state/`
- `auth-model/state/README.md`
- `auth-model/model-manifest.json`
- `AUTH-MODEL-GUIDE.md`

Implementation files inspected or changed:

- Inspected `PendingIdentity.cs`.
- Inspected `AuthTenantStateSnapshot.cs`.
- Inspected `AppUserPasskeyManager.cs`.
- Considered the supplied `AuthStateService.GetAuthSession` implementation.
- No runtime implementation files changed.

Scenarios completed:

- None. Scenario authoring begins after dimensions, invariants, actions, and transitions are established.

Evidence completed:

- Definitions were authored against the Section 4.1 state-dimension schema.
- Runtime schema-validation tooling remains future work.

Known DDR conflicts:

- None formally recorded yet. The dimensions remain proposed until reviewed through concrete flows.

Known implementation conflicts:

- Current session routing compresses several dimensions into A/R/E/O/M and overlaps Registered with ProfileComplete.
- Existing organization behavior may not yet enforce the home-workspace model.

Open questions:

- Whether credential proof should later split ceremony proof from durable credential enrollment.
- Whether workspace membership requires an additional role/permission projection.
- Whether account lockout and disabled status should remain one dimension or separate after recovery modeling.

Completion criteria remaining:

- None for Section 4.2.

Recommended next action:

- Begin Section 4.3, Invariant catalog, converting the guide's foundational rules and the invalid-combination examples into authored invariant definitions.
