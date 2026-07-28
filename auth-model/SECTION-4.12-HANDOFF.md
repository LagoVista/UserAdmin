# Section 4.12 Handoff: Traditional Form Onboarding

## Status

Complete for the first evidence-backed web binding set.

## Decision

Traditional forms are presentation bindings over canonical actions, transitions, journeys, and scenarios. A route, view, field, or button does not define authentication behavior. It only supplies inputs, invokes an operation, or presents the resulting state.

Guided VTM and traditional UI therefore share the same canonical model. Presentation differences may change sequencing, explanation, layout, and secure-component handoffs, but must not introduce alternate identity, invitation, workspace, or session rules.

## Added presentation bindings

- `auth.presentation.web.password-registration`
- `auth.presentation.web.password-email-verification`
- `auth.presentation.web.invitation-review`

## Evidence-backed mappings

### Password registration

- Route: `/auth/register`
- View ID: `auth.register.new`
- Fields: first name, last name, email, password, confirm password
- Submit finder: `action:register`
- Current operation: `ServiceContext.clients.user.createRegister`
- Expected route: `/auth/user/email/confirm/sent`

The current submit operation is composite. It gathers inputs for and performs work corresponding to multiple canonical actions behind one endpoint. The presentation binding records that seam rather than redefining the canonical model around the endpoint.

### Email verification

- Route: `/auth/user/email/confirm`
- View ID: `auth.confirming-email`
- Input: verification token from confirmation-link state
- Canonical action: `auth.action.password.verify-email`
- Expected success route: `/auth/user/email/confirmed`

### Invitation review

- Route: `/auth/invite/accept/:id`
- Input: invitation ID from the route
- Current operation: `UserService.getInvitation`
- Canonical action: `auth.action.invitation.validate`
- Existing-account, new-account, and provider choices remain navigation or identity-establishment paths. They do not accept the invitation or prove identity.

## Presentation invariants

1. A UI route or view is not canonical authentication state.
2. A button may invoke only behavior permitted by the bound canonical action.
3. Secret fields remain secure UI inputs and are never conversational memory.
4. Expected routes and views are projections of authoritative post-state.
5. Navigation-only choices do not require fake server transitions.
6. Composite endpoints must not collapse the canonical action boundaries in the model.
7. Semantic IDs and finders are stable evidence hooks, not business rules.
8. Traditional and guided experiences must converge on the same transition outcomes.

## Implementation alignment

1. Add stable semantic IDs to all auth screens, fields, and actions that currently lack them.
2. Decompose `createRegister` internally or expose an orchestration boundary that preserves canonical action-level logging, validation, and idempotency.
3. Replace legacy invitation routes that currently point to non-canonical login paths with the shared current auth journey.
4. Keep invitation validation, identity selection, acceptance, and workspace switching separate.
5. Add presentation bindings incrementally as routes and semantic IDs are verified from source.
6. Validate expected routes against the future session projection model in Section 4.13.

## Next major area

Section 4.13: Session projection and routing.
