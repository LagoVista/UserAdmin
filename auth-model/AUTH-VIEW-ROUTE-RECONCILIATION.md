# AuthView and AuthRoute Reconciliation Ledger

## Authority

Canonical AuthView and AuthRoute definitions under `auth-model/auth-views` and `auth-model/auth-routes` are the source of truth.

`CommonLinks`, Angular `provideAuthRoutes()`, Angular component identifiers, React Native routes, React Native test identifiers, runtime AuthView entities, and generated test scenarios are projections that must reconcile to these definitions.

No backward-compatible route or identifier aliases are required.

## Implementation order

1. Update `CommonLinks` so every active canonical route has exactly one member or helper.
2. Update Angular `provideAuthRoutes()` to use the canonical route paths.
3. Update Angular component `ViewId`, control IDs, action IDs, and `data-testid` values.
4. Re-export and validate runtime AuthView projections.
5. Reconcile React Native screens, routes, controls, actions, and test IDs.
6. Regenerate or repair AppUserTestScenario projections.

## Canonical route families established

- `/auth/welcome`
- `/auth/continue/...`
- `/auth/sign-in/...`
- `/auth/password/reset/...`
- `/auth/magic-link/...`
- `/auth/oauth/...`
- `/auth/email-verification/...`
- `/auth/registration/...`
- `/auth/invitation/...`
- `/auth/passkey/enrollment/...`
- `/auth/passkey/management`
- `/auth/mfa/totp/...`

## Known route replacements

| Existing route | Canonical route |
| --- | --- |
| `/auth/mfa/top/confirm` | `/auth/mfa/totp/confirm` |
| `/auth/oauth/accessdenied` | `/auth/oauth/access-denied` |
| `/api/auth/magiclink/sent` | `/auth/magic-link/sent` |
| `/auth/continue/email/unable` | `/auth/sign-in/unable` |
| `/auth/forgot` | `/auth/password/reset/request` |
| `/auth/forgot/sent` | `/auth/password/reset/sent` |
| `/auth/reset/{code}` | `/auth/password/reset/{code}` |
| `/auth/user/email/confirmed` | `/auth/email-verification/confirmed` |
| `/auth/user/email/couldnotconfirm` | `/auth/email-verification/failed` |
| `/auth/email/confirm/sent` | `/auth/email-verification/sent` |
| `/auth/register` | `/auth/registration/create-account` |
| `/auth/user/register` | `/auth/registration/complete-profile` |
| `/auth/invite/accept/{inviteid}` | `/auth/invitation/{invitation-id}` |
| `/auth/invite/accepted` | `/auth/invitation/accepted` |
| `/auth/invite/failed` | `/auth/invitation/failed` |
| `/auth/passkey/enroll/start` | `/auth/passkey/enrollment/start` |
| `/auth/passkey/enroll/confirm` | `/auth/passkey/enrollment/confirm` |
| `/auth/passkey/manage` | `/auth/passkey/management` |

## Identifier rules

- View IDs use dotted semantic keys.
- Route IDs use `auth.route...` dotted semantic keys.
- Control and action IDs use kebab-case.
- Semantic finders use `field:`, `label:`, `status:`, `display:`, or `action:` prefixes.
- Mutable display copy must not be used as an identifier.
- Route parameters are not controls.
- Provider action IDs describe user intent rather than OAuth plumbing.
