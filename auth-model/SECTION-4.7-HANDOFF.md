# Section 4.7 Handoff: Native-Provider Registration

Status: Complete

## Objective completed

The model now defines native Apple and Google provider registration and existing-user resolution through the shared PendingIdentity airlock.

## Core decisions

- The provider subject identifier is the stable primary provider identity key.
- Provider email is supporting evidence and is never sufficient by itself to resolve or link a durable identity.
- Registered email, verified email, provider email, and invited email remain distinct values even when equal.
- A pre-existing provider link may resolve directly to exactly one durable identity.
- An unlinked provider subject requires explicit selection and confirmation before linking to an existing durable identity.
- Provider linking is atomic and idempotent.
- New durable-user creation occurs only after provider proof, profile completeness, and duplicate-user checks pass.
- Durable-user creation, home-workspace creation, and authenticated-session establishment remain separate deterministic actions.

## Definitions added

### Actions

- auth.action.native-provider.capture-assertion
- auth.action.native-provider.resolve-linked-subject
- auth.action.native-provider.select-link-target
- auth.action.native-provider.link-to-user
- auth.action.native-provider.create-user
- auth.action.native-provider.create-home-workspace
- auth.action.native-provider.establish-session

### Transitions

- auth.transition.native-provider.capture-assertion
- auth.transition.native-provider.resolve-linked-subject
- auth.transition.native-provider.select-link-target
- auth.transition.native-provider.link-to-user
- auth.transition.native-provider.create-user
- auth.transition.native-provider.create-home-workspace
- auth.transition.native-provider.establish-session

### Journeys

- auth.journey.native-provider.existing-user
- auth.journey.native-provider.new-user-registration

### Scenarios

- auth.scenario.native-provider.email-mismatch-preserved
- auth.scenario.native-provider.resolve-linked-user
- auth.scenario.native-provider.select-link-target
- auth.scenario.native-provider.link-existing-user
- auth.scenario.native-provider.create-new-user
- auth.scenario.native-provider.create-home-workspace
- auth.scenario.native-provider.establish-session

## Security coverage

- Provider email mismatch does not overwrite registered or invited email.
- Provider subject resolution cannot select a user by email alone.
- A provider subject cannot link to multiple users.
- Existing-user linking requires explicit target selection.
- Duplicate durable-user creation is rejected.
- PendingIdentity cannot receive normal application authorization.

## Next major area

Section 4.8: Passkey registration and authentication.
