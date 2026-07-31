# Scenario Catalog V1 Archive

The original scenario catalog remains in `auth-model/scenarios` as a frozen legacy reference.

It modeled many server-domain transitions and orchestration steps as scenarios. That work remains useful for transition, invariant, telemetry, and server-proof analysis, but it is no longer the authoritative `AppUserTestScenario` catalog for UI automation.

The authoritative V2 catalog begins in `auth-model/scenarios-v2` and is built behavior-first. Each V2 scenario represents exactly one user action from one starting view to one deterministic resulting view and user state.

The first V2 pilot contains two linear behaviors:

- Password Sign-In Success
- Password Sign-In Invalid Password

The V1 files are intentionally not rewritten or silently reclassified. Git history preserves the exact catalog state that led to this reset.
