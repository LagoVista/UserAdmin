# OAuth / OIDC Authorization Server Production Hardening Checklist

Status: audit baseline for production readiness

Audited repository: `LagoVista/UserAdmin`

Audited baseline commit: `deb4c1c2cd4ad95d2310c0046192ae0e408201ce`

Primary implementation:

- `src/LagoVista.AspNetCore.AuthorizationServer`
- `src/LagoVista.UserAdmin.Models/Auth/OAuthClientApplication.cs`
- `src/LagoVista.UserAdmin/Managers/OAuthClientApplicationManager.cs`
- `src/LagoVista.UserAdmin.Repos/Repos/Security/OAuthClientApplicationRepo.cs`
- `docs/oauth-authorization-code-smoke-test.md`
- `docs/grafana-oidc-pilot.md`

## Purpose

This document is the production-readiness checklist for the NuvIoT OAuth 2.0 / OpenID Connect authorization-server capability.

The current implementation is a valid development vertical slice: authorization code flow, S256 PKCE, discovery, token issuance, local token validation, UserInfo, UserAdmin-backed OAuth client configuration, and a browser smoke-test path are in place. It is not yet ready to be treated as a general-purpose production identity provider.

The most important architectural fact is that the server intentionally uses OpenIddict **degraded mode**. In degraded mode, OpenIddict does not own the application store and disables features that normally depend on `OpenIddict.Core`, including built-in `client_id` / `client_secret` / redirect-URI validation, reference-token behavior, and token revocation. UserAdmin therefore becomes responsible for every production behavior that depends on client, authorization, or token persistence.

The production gate is not simply “make the smoke test pass.” The production gate is that every security-sensitive responsibility disabled by degraded mode is either implemented and tested in UserAdmin or deliberately moved back under OpenIddict Core/custom stores.

## Current strengths

- [x] Authorization code is the only enabled OAuth flow.
- [x] Global PKCE is required by the server.
- [x] Public clients are required by the model to use PKCE.
- [x] The authorization-request validator requires S256 rather than allowing `plain`.
- [x] Redirect URI comparison is exact ordinal matching.
- [x] Client status is checked before authorization and token exchange.
- [x] Requested scopes are restricted to the client's allow-list.
- [x] Requested resource is restricted to the client's allow-list.
- [x] Authorization responses are produced through OpenIddict rather than hand-built code/token responses.
- [x] Authorization-code integrity and PKCE verifier validation remain delegated to OpenIddict.
- [x] Access tokens can be audience/resource constrained.
- [x] UserInfo requires the OpenIddict validation authentication scheme.
- [x] Profile/email UserInfo claims are scope-gated.
- [x] Client secrets are stored through `ISecureStorage` rather than in the OAuth client document.
- [x] OAuth client reads clear `ClientSecret` before returning the model.
- [x] Active confidential clients are rejected by model validation if no secret exists.
- [x] Development documentation clearly calls out that confidential-client authentication and production certificates are not complete.
- [x] OpenIddict 7.6.0 is the current stable 7.x line used by this service; do not move production to the 8.x preview line.

## Production blockers: P0

These items must be complete before the service is considered production-ready.

### 1. Make the degraded-mode architecture an explicit decision

- [ ] Decide whether production will continue using OpenIddict degraded mode.
- [ ] If degraded mode remains, document UserAdmin as the authoritative store for OAuth clients, durable grants/authorizations, consent, refresh-token lineage, revocation state, and any reference-token state that is introduced.
- [ ] If those responsibilities should instead be owned by OpenIddict, implement an OpenIddict Core integration/custom store rather than partially recreating Core semantics in event handlers.
- [ ] Record the decision in an ADR under `/docs` and make degraded mode impossible to enable accidentally without the required UserAdmin services.
- [ ] Add startup self-validation that fails closed when required production services are missing.

**Acceptance:** the team can answer exactly where each OAuth/OIDC security state lives, how it is recovered, and which component is authoritative.

### 2. Remove global anonymous-client behavior for confidential clients

Current configuration calls `AcceptAnonymousClients()` while the UserAdmin model supports `ClientType = confidential`.

- [ ] Do not advertise confidential-client support until real client authentication is implemented.
- [ ] Implement token-endpoint client authentication for confidential clients.
- [ ] At minimum support correctly validated `client_secret_basic` or `client_secret_post` if compatibility requires shared secrets.
- [ ] Prefer adding `private_key_jwt` for production service-to-service confidential clients where feasible.
- [ ] Retrieve secret material from `ISecureStorage` only for comparison; never copy it into logs, token claims, response DTOs, or persisted client documents.
- [ ] Use a constant-time secret comparison or an established credential-verification primitive.
- [ ] Reject confidential-client token requests that omit client authentication.
- [ ] Reject public clients that attempt to authenticate with a client secret.
- [ ] Bind authorization codes and refresh tokens to the authenticated client.
- [ ] Add positive and negative tests for every enabled client-authentication method.
- [ ] Either remove `AcceptAnonymousClients()` or prove, with tests and custom handlers, that anonymous treatment applies only to clients intentionally modeled as public.

**Acceptance:** a caller knowing only a confidential `client_id` cannot successfully use the token endpoint.

### 3. Replace development certificates with managed production signing keys

`AuthorizationServerOptions.UseDevelopmentCertificates` currently defaults to `true`.

- [ ] Change the production-safe default so development certificates are opt-in, not opt-out.
- [ ] Require explicit production signing credentials at startup.
- [ ] Use dedicated asymmetric signing credentials, separate from the HTTPS/TLS certificate.
- [ ] If token encryption is enabled, use a separate encryption key/certificate from the signing key.
- [ ] Store private keys in the platform's production secret/key system rather than source control or application settings.
- [ ] Define key ownership and administrator access.
- [ ] Define key backup/recovery where appropriate.
- [ ] Define scheduled rotation and emergency rotation procedures.
- [ ] Support overlapping old/new signing keys during rollover so previously issued tokens remain verifiable until expiry.
- [ ] Verify discovery/JWKS publishes the correct public signing keys and never exposes private material.
- [ ] Alert on signing-certificate expiration well before expiry.
- [ ] Add a disaster-recovery exercise for loss/rotation of signing credentials.

**Acceptance:** every production instance shares a deliberate, recoverable signing-key set and rolling deployment/restart does not unexpectedly invalidate or fork issuer keys.

### 4. Implement durable consent and authorization grants

`OAuthClientApplication.RequireConsent` exists but is not consumed by the authorization server. The current controller issues authorization after authentication without a consent decision.

- [ ] Implement an authorization/consent service and durable authorization grant model.
- [ ] Honor `RequireConsent`.
- [ ] Show the user the client identity, requested scopes, and meaningful resource/audience before consent.
- [ ] Never trust client-provided display metadata without sanitization.
- [ ] Support explicit deny and return the correct OAuth error.
- [ ] Persist exactly which scopes/resources the user granted.
- [ ] Prevent silent expansion of a previous grant when a client later asks for additional scopes/resources.
- [ ] Define when first-party clients may skip consent and make that an explicit server-side trust policy, not merely a client-editable flag.
- [ ] Define grant revocation by user and by administrator.
- [ ] Implement `prompt=consent` semantics if OIDC clients will rely on them.
- [ ] Implement safe handling of `prompt=none` before claiming complete OIDC behavior.

**Acceptance:** consent-required clients cannot obtain a grant without an auditable user decision and previously granted permissions cannot be silently widened.

### 5. Implement revocation semantics before issuing long-lived credentials

A revocation endpoint constant exists but no revocation endpoint is configured or implemented. Degraded mode does not provide built-in revocation state.

- [ ] Decide whether access tokens remain short-lived self-contained JWTs or whether reference/introspectable tokens are required.
- [ ] Define the maximum time a revoked user/client can continue using an already-issued self-contained access token.
- [ ] Keep access-token lifetimes short enough to meet that requirement.
- [ ] Implement token revocation before refresh tokens are enabled.
- [ ] Implement user-session / authorization-grant revocation.
- [ ] Implement client-wide revocation when client status becomes `revoked` or `disabled`.
- [ ] Define behavior when a user's account is disabled, deleted, or security state changes.
- [ ] If immediate access-token invalidation is required, add an introspection/reference-token strategy or a resource-server revocation check instead of assuming a signed JWT can be recalled.
- [ ] Ensure revocation operations are idempotent.
- [ ] Ensure revocation does not reveal whether an arbitrary token exists.

**Acceptance:** operations has a documented and tested way to terminate future token issuance immediately and bound the lifetime of already-issued credentials.

### 6. Do not enable refresh tokens until rotation/replay detection exists

The model already exposes `RefreshTokenLifetimeDays` and constants contain `refresh_token`, but the server currently allows only authorization code flow.

- [ ] Keep refresh-token grant disabled until the complete lifecycle exists.
- [ ] Risk-assess which clients actually need offline access.
- [ ] Require explicit `offline_access` consent where appropriate.
- [ ] Bind refresh tokens to the client, granted scopes, resource/audience, authorization, and subject.
- [ ] For public clients, implement refresh-token rotation or sender-constrained refresh tokens.
- [ ] Persist refresh-token family/lineage so replay of an invalidated token can revoke the active family.
- [ ] Re-evaluate user/client status during refresh.
- [ ] Prevent a refresh from expanding scopes or resources beyond the original grant.
- [ ] Enforce absolute and inactivity expiration policies.
- [ ] Add administrative and user revocation paths.
- [ ] Add replay-detection telemetry and alerting.

**Acceptance:** a stolen old refresh token cannot be replayed indefinitely and refresh never bypasses current account/client policy.

### 7. Harden OAuth client registration data

Current model validation checks presence and duplicates but does not validate URI syntax/security properties or constrain arbitrary grant/scope/resource strings.

- [ ] Parse and validate every redirect URI at registration/update time.
- [ ] Reject fragments in redirect URIs.
- [ ] Reject URI user-info components.
- [ ] Reject wildcard redirect URIs.
- [ ] Require HTTPS for web redirect URIs.
- [ ] Permit HTTP only for explicitly supported native loopback redirect scenarios.
- [ ] Decide whether native loopback clients need RFC 8252 dynamic-port behavior; current exact matching is safe for fixed ports but does not implement the dynamic-port exception.
- [ ] Validate post-logout redirect URIs with equivalent rigor before end-session support is added.
- [ ] Validate `LogoUrl`, `PrivacyPolicyUrl`, and `TermsOfServiceUrl` as safe HTTPS URLs before rendering them on a consent screen.
- [ ] Replace arbitrary grant-type strings with a server-controlled allow-list.
- [ ] Validate scopes against a server-side registered scope catalog rather than accepting any non-empty string.
- [ ] Validate resources/audiences against a server-side resource catalog.
- [ ] Relax `AllowedResources` being mandatory for OIDC-only clients; absence of a resource should be a deliberate supported case, not a placeholder value.
- [ ] Validate access-token lifetime against server-defined minimum/maximum bounds.
- [ ] Validate future refresh-token lifetime against server-defined bounds.
- [ ] Normalize `ClientId` rules and define allowed length/character set.
- [ ] Make `ClientId` uniqueness race-safe at the persistence layer or use a deterministic unique storage key. The current query-then-create pattern is not sufficient as a hard uniqueness guarantee under concurrent creation.

**Acceptance:** malformed or dangerous client metadata cannot be persisted even if a UI or API caller bypasses normal operator expectations.

### 8. Lock down issuer, HTTPS, proxy, and host handling

- [ ] Require an explicit absolute HTTPS issuer in production.
- [ ] Fail startup if the issuer is missing or HTTP outside a development environment.
- [ ] Confirm reverse-proxy forwarded headers are trusted only from known proxies/networks.
- [ ] Prevent Host-header injection from affecting issuer, redirects, discovery, or generated absolute URLs.
- [ ] Confirm TLS termination and internal proxy hops preserve the expected scheme.
- [ ] Enable HSTS at the production edge/host as appropriate.
- [ ] Confirm authorization-server endpoints are never exposed over plain HTTP in production.
- [ ] Verify discovery metadata always uses the canonical external issuer.

**Acceptance:** changing the inbound Host/forwarded headers cannot make the authorization server mint metadata or redirects for an attacker-controlled origin.

### 9. Define access-token claim and audience policy

- [ ] Define the minimum common claims in access tokens.
- [ ] Do not include email/profile claims unless a target API actually requires them.
- [ ] Do not include secret, credential, internal security, or unnecessary PII claims.
- [ ] Require audience/resource validation at every protected API.
- [ ] Do not treat possession of a valid token from the issuer as sufficient if the token was minted for another resource server.
- [ ] Define scope-to-API authorization policies and enforce scopes inside APIs, not only when issuing tokens.
- [ ] Define organization/tenant authorization separately from identity; a valid subject token must not imply access to every organization.
- [ ] Review whether `client_id` should remain a custom access-token claim or use a standard authorized-party representation where interoperability benefits.
- [ ] Document the deliberate decision to disable access-token encryption. Signed JWTs are commonly acceptable, but their contents are readable by token holders and infrastructure that receives them.

**Acceptance:** every API validates issuer, signature, lifetime, audience/resource, and required scopes/tenant authorization.

### 10. Add protocol-level integration and adversarial tests

- [ ] Create a dedicated authorization-server test suite; do not rely on the PowerShell happy-path smoke test alone.
- [ ] Test discovery metadata.
- [ ] Test a successful public-client authorization-code + S256 flow.
- [ ] Test unknown, disabled, and revoked clients.
- [ ] Test redirect URI exact mismatch, including trailing slash, host, port, scheme, query, encoding, case-sensitive path, and malicious external URI cases.
- [ ] Test missing PKCE.
- [ ] Test `plain` PKCE rejection.
- [ ] Test incorrect code verifier.
- [ ] Test PKCE downgrade attempts.
- [ ] Test authorization-code one-time use/replay.
- [ ] Test authorization code cannot be redeemed by a different client.
- [ ] Test token exchange redirect-URI continuity where applicable.
- [ ] Test requested scope outside the client's allow-list.
- [ ] Test requested resource outside the client's allow-list.
- [ ] Test multiple-resource rejection while that remains the server policy.
- [ ] Test expired authorization codes.
- [ ] Test malformed protocol parameters and duplicate parameters.
- [ ] Test authenticated user missing a stable subject.
- [ ] Test UserInfo with and without `profile` and `email` scopes.
- [ ] Test access token with wrong issuer, audience, signature, and expiry against protected APIs.
- [ ] Test consent allow/deny/previous-grant behavior once consent is implemented.
- [ ] Test confidential-client missing/wrong/correct credentials once implemented.
- [ ] Test client-secret rotation and old-secret rejection.
- [ ] Test refresh-token rotation/replay before refresh is enabled.
- [ ] Test revocation semantics before revocation is advertised.
- [ ] Add interoperability tests with at least one independent OIDC client implementation such as Grafana.
- [ ] Add regression tests for every production security bug found after launch.

**Acceptance:** CI exercises both happy-path protocol behavior and known OAuth attack classes.

## Production hardening: P1

These should normally be complete for first production rollout, even if they do not individually block a tightly controlled pilot.

### 11. Centralize authorization-request policy validation

The same policy checks currently exist in both `OAuthAuthorizationRequestValidationHandler` and `AuthorizationController`.

- [ ] Remove duplicated protocol/client-policy validation after the custom OpenIddict handler coverage is proven.
- [ ] Keep the controller focused on user interaction: login, consent, principal construction, and sign-in.
- [ ] Keep protocol validation in OpenIddict event handlers so every authorization request follows the same security path.
- [ ] Add tests that prove controller passthrough cannot bypass handler validation.

### 12. Make handler ordering maintainable

Current custom handlers use `Int32.MaxValue - 100_000`.

- [ ] Anchor custom handler order relative to a documented OpenIddict built-in handler descriptor instead of a magic near-maximum integer where possible.
- [ ] Add a comment explaining exactly which built-in validations must occur before/after each custom handler.
- [ ] Add an integration test that would fail if a future OpenIddict upgrade changed the required pipeline semantics.

### 13. Secret lifecycle and storage consistency

- [ ] Define client-secret entropy/length requirements if UserAdmin allows operator-supplied secrets.
- [ ] Prefer server-generated high-entropy client secrets with one-time display if the product UX permits it.
- [ ] Never allow a stored secret to be read back.
- [ ] Add explicit secret rotation instead of overloading general client update semantics.
- [ ] Allow a controlled overlap window if external confidential clients require zero-downtime secret rotation.
- [ ] Make secure-storage creation + client persistence failure-safe. If persistence fails after a new secret is stored, remove the orphaned secret.
- [ ] Make client deletion + secure-storage cleanup failure observable/retriable so secrets are not silently orphaned.
- [ ] Audit secret create/rotate/delete operations without logging secret material.

### 14. Client administration authorization and audit trail

- [ ] Confirm only appropriately privileged organization/security administrators can create, activate, rotate, revoke, or delete OAuth clients.
- [ ] Require stronger authorization for activating a client than merely editing display metadata if feasible.
- [ ] Record who created a client and who changed status, redirect URIs, scopes, resources, grant types, token lifetimes, PKCE policy, and consent policy.
- [ ] Record before/after security-relevant values without recording secrets.
- [ ] Consider four-eyes approval for highly privileged production clients.
- [ ] Prevent a tenant administrator from registering resources/scopes belonging to another tenant unless explicitly authorized.

### 15. Rate limiting and abuse protection

- [ ] Rate-limit `/connect/authorize` by IP/session/client with thresholds that do not break legitimate login flows.
- [ ] Rate-limit `/connect/token` aggressively enough to limit credential/code guessing.
- [ ] Rate-limit `/connect/userinfo` and any future introspection/revocation endpoints.
- [ ] Distinguish protocol errors from infrastructure failures in metrics.
- [ ] Add abuse detection for repeated invalid client credentials, redirect mismatches, invalid codes, and PKCE failures.
- [ ] Ensure rate-limit error responses do not leak whether a particular client/user/token exists.
- [ ] Ensure upstream WAF/proxy limits preserve standards-compliant OAuth POST bodies.

### 16. Browser security headers and authorization-page hygiene

- [ ] Apply `Referrer-Policy: no-referrer` or an equivalently strict policy to authorization/consent responses.
- [ ] Do not load third-party scripts, pixels, analytics, fonts, or images on pages that can contain authorization-request details unless there is a reviewed reason.
- [ ] Apply a restrictive Content Security Policy.
- [ ] Protect login/consent pages against framing (`frame-ancestors` / equivalent) unless an explicit embedded-auth use case is approved.
- [ ] Review cookie `Secure`, `HttpOnly`, `SameSite`, lifetime, and domain settings for the authentication session used during authorization.
- [ ] Make login-return URLs local/validated so the authentication challenge cannot become an open redirect.

### 17. Observability and security telemetry

- [ ] Add structured security events for authorization request accepted/rejected.
- [ ] Add structured events for token exchange accepted/rejected.
- [ ] Include correlation/trace ID, client ID, endpoint, normalized failure reason, and outcome.
- [ ] Do not log authorization codes, access tokens, refresh tokens, client secrets, PKCE verifiers, or raw credential headers.
- [ ] Avoid logging full authorization URLs if they can contain sensitive state/request data.
- [ ] Add counters for success, invalid client, invalid redirect, invalid scope, invalid target, invalid code, PKCE failure, consent deny, and server failure.
- [ ] Alert on abnormal error-rate spikes and repeated security failures.
- [ ] Define log retention appropriate for security investigations.

### 18. Health checks and operational readiness

- [ ] Add a health check that verifies required key material is loaded.
- [ ] Add a health check for the OAuth client store.
- [ ] Add a health check for `ISecureStorage` where confidential clients depend on it.
- [ ] Decide whether discovery should remain available when backing client storage is degraded.
- [ ] Establish SLOs for authorization and token endpoints.
- [ ] Define dependency timeout/retry policy; do not allow a Cosmos/secret-store outage to hang browser authorization indefinitely.
- [ ] Test rolling deployment across multiple replicas.
- [ ] Test clock skew between replicas and resource servers.
- [ ] Synchronize production hosts with a reliable time source.

### 19. Client-store consistency and caching

- [ ] Decide whether `OAuthClientPolicyResolver` will cache client policy.
- [ ] If caching is added, define very short/explicit invalidation semantics for client disable/revoke, redirect changes, scope changes, and secret rotation.
- [ ] Never allow a long cache TTL to defeat emergency client revocation.
- [ ] Make client-ID uniqueness atomic or otherwise race-safe.
- [ ] Add optimistic concurrency/version checking to OAuth client updates so simultaneous administrators cannot silently overwrite security configuration.

### 20. Dependency and patch management

- [ ] Stay on stable OpenIddict 7.x while 8.x is preview.
- [ ] Define how OpenIddict security releases are monitored and applied.
- [ ] Monitor .NET 9 security servicing and define the runtime upgrade path before end of support.
- [ ] Enable dependency vulnerability scanning in CI.
- [ ] Pin package versions intentionally and review transitive dependency changes.
- [ ] Re-run the complete OAuth adversarial suite before every OpenIddict major/minor upgrade.

## OIDC completeness: P1/P2 depending on consumers

The current capability is sufficient for a narrow OIDC-style login pilot through authorization code + UserInfo, but these items must be completed before claiming broad OIDC Provider compatibility.

### 21. Define the supported OIDC contract

- [ ] Decide whether UserAdmin is formally an OpenID Provider or only an OAuth authorization server that exposes selected OIDC-compatible features.
- [ ] If claiming OIDC Provider support, explicitly test the `openid` scope and ID-token issuance/validation behavior.
- [ ] Define supported response modes/types.
- [ ] Define and test `nonce` behavior where applicable.
- [ ] Define `auth_time`, `max_age`, `prompt`, and authentication-context behavior required by consumers.
- [ ] Validate discovery metadata against the actual implemented feature set.
- [ ] Do not advertise endpoints/grants/auth methods that are not implemented.
- [ ] Run an OIDC conformance suite before broad third-party integration.

### 22. End-session/logout

- [ ] Do not advertise end-session behavior until designed and tested.
- [ ] Validate post-logout redirect URIs exactly.
- [ ] Define whether logout terminates only the RP session, the UserAdmin login session, grants, or some combination.
- [ ] Protect logout against cross-client redirect abuse.
- [ ] Decide whether front-channel/back-channel logout is required for infrastructure applications.

### 23. Resource-server metadata and MCP

The existing smoke-test document intentionally defers MCP protected-resource metadata.

- [ ] Define protected-resource metadata requirements before MCP integration is advertised.
- [ ] Ensure MCP/resource APIs validate the same issuer/audience/scope contract as conventional APIs.
- [ ] Keep OAuth authorization-server metadata and protected-resource metadata as separate, standards-correct responsibilities.

## Optional advanced controls: P2

Evaluate these based on threat model and consumer profile rather than enabling them mechanically.

- [ ] DPoP sender-constrained access tokens for higher-risk public clients.
- [ ] mTLS for high-assurance confidential machine clients.
- [ ] `private_key_jwt` as the preferred confidential-client method instead of long-lived shared secrets.
- [ ] Pushed Authorization Requests (PAR) for clients requiring integrity-protected authorization requests.
- [ ] Issuer identification in authorization responses / mix-up defenses where clients can use multiple authorization servers.
- [ ] Fine-grained per-client signing/token policy when interoperability requires it.
- [ ] Introspection/reference tokens for APIs requiring near-immediate revocation.

## Required pre-production test matrix

Before first production enablement, run and retain results for all rows below.

| Area | Required result |
| --- | --- |
| Discovery | Canonical HTTPS issuer and only implemented metadata advertised |
| Public auth-code client | Successful S256 PKCE login/token/UserInfo flow |
| Unknown client | Rejected |
| Disabled client | Rejected |
| Revoked client | Rejected |
| Redirect mismatch | Rejected before user is redirected to untrusted target |
| Missing PKCE | Rejected |
| `plain` PKCE | Rejected |
| Wrong verifier | Rejected |
| Code replay | Rejected |
| Code used by wrong client | Rejected |
| Invalid scope | Rejected |
| Invalid resource | Rejected |
| Expired code | Rejected |
| Wrong token audience | API rejects token |
| Wrong token issuer | API rejects token |
| Expired token | API rejects token |
| Tampered token | API rejects token |
| Missing required API scope | API rejects token |
| Consent-required client | No issuance before allow; deny is standards-correct |
| Confidential client | Missing/wrong credential rejected; correct credential accepted |
| Client disabled after token issuance | Future issuance stops immediately; existing-token behavior matches documented TTL/revocation design |
| Key rollover | New tokens use new key while tokens signed by prior valid key continue to validate during overlap |
| Multi-replica deployment | Same issuer/key behavior from every replica |
| Dependency outage | Fails closed with controlled response; no policy bypass |
| Logs | No secrets, codes, tokens, or PKCE verifiers captured |

## Suggested implementation sequence

### Phase A: production security boundary

- [ ] Architectural decision on degraded mode vs OpenIddict Core/custom stores.
- [ ] Production signing-key configuration and startup validation.
- [ ] URI/client registration hardening.
- [ ] Dedicated authorization-server integration tests.
- [ ] Centralize request validation in OpenIddict handlers.
- [ ] Rate limiting, security logging, browser headers.

At the end of Phase A, a **public-client, authorization-code + S256, short-lived-access-token** production pilot can be considered if consent requirements are intentionally limited to pre-trusted first-party clients and that exception is explicitly documented.

### Phase B: general interactive clients

- [ ] Durable consent/authorization grants.
- [ ] User/admin grant revocation.
- [ ] Full OIDC contract and conformance testing as required by consumers.
- [ ] Logout/end-session if required.

### Phase C: confidential clients

- [ ] Confidential-client authentication.
- [ ] Secret/key rotation lifecycle.
- [ ] Prefer `private_key_jwt` for new high-value confidential clients.

### Phase D: offline/long-lived access

- [ ] Refresh-token policy.
- [ ] Rotation/replay detection or sender constraint.
- [ ] Durable token/grant revocation.
- [ ] Optional introspection/reference tokens for immediate-revocation workloads.

## Production readiness definition

The authorization server can be marked production-ready only when all applicable P0 items are checked and the following statements are true:

- [ ] Production never uses OpenIddict development certificates.
- [ ] The issuer and signing-key lifecycle are documented and operationally owned.
- [ ] No enabled client type can bypass its required client authentication.
- [ ] Every redirect URI reaching a browser is validated against hardened registration rules.
- [ ] PKCE S256 is enforced for public clients and tested against downgrade/replay scenarios.
- [ ] Consent behavior matches the stored client policy and cannot silently widen grants.
- [ ] The effect of disabling a user/client is explicitly defined for both future and already-issued tokens.
- [ ] Refresh tokens are either disabled or fully protected against replay.
- [ ] Every production resource server validates issuer, signature, lifetime, audience/resource, and authorization scopes/tenant policy.
- [ ] Security-relevant actions and failures are observable without leaking credentials/tokens.
- [ ] Multi-replica deployment, key rollover, dependency failure, and disaster-recovery paths have been exercised.
- [ ] The adversarial test suite is green in CI.

## Audit notes tied to the current implementation

1. `Startup.cs` enables degraded mode and `AcceptAnonymousClients()`, enables authorization code flow, requires PKCE, and registers custom authorization/token request validators.
2. `OAuthAuthorizationRequestValidationHandler` validates active client, authorization-code grant permission, exact redirect URI, scopes, at most one resource, resource allow-list, and S256 PKCE, then marks the redirect URI trusted for OpenIddict.
3. `OAuthTokenRequestValidationHandler` currently checks only authorization-code grant, active client, and grant permission. It does not validate a confidential-client secret.
4. `AuthorizationController` currently repeats much of the authorization request policy validation. This should be simplified only after the handler path has strong integration coverage.
5. `OAuthClientApplication` models public/confidential clients, active/disabled/revoked status, secrets, redirect URIs, grant types, scopes, resources, PKCE, consent, and token lifetimes. Its current custom validation primarily checks presence/duplicates and a few public/confidential invariants; it does not yet enforce hardened URI/scope/resource/lifetime policy.
6. `OAuthClientApplicationManager` stores client secrets through `ISecureStorage` and strips the clear secret before returning models. Secret verification is not yet used by the authorization server.
7. `OAuthClientApplicationRepo` enforces client-ID uniqueness using a query in manager workflow rather than an atomic persistence uniqueness guarantee.
8. `AuthorizationServerOptions` currently defaults `UseDevelopmentCertificates = true` and `DisableAccessTokenEncryption = true`. The first is unsafe as a production default; the second requires an explicit token-privacy decision rather than being treated as encryption-equivalent security.
9. `AuthorizationServerConstants` already names revocation and refresh-token concepts, but current server registration does not enable them. Keep them disabled until the lifecycle described above exists.
10. `UserInfoController` properly gates name/email by requested scopes; `OAuthWhoAmIController` is a smoke-test/debug endpoint and should not become an accidental production information-disclosure surface.

## Standards and implementation references

Use these as the baseline when completing this checklist:

- OAuth 2.0 Security Best Current Practice, RFC 9700: https://www.rfc-editor.org/rfc/rfc9700
- Proof Key for Code Exchange (PKCE), RFC 7636: https://www.rfc-editor.org/rfc/rfc7636
- OAuth 2.0 for Native Apps, RFC 8252: https://www.rfc-editor.org/rfc/rfc8252
- OAuth Authorization Server Metadata, RFC 8414: https://www.rfc-editor.org/rfc/rfc8414
- OAuth Token Revocation, RFC 7009: https://www.rfc-editor.org/rfc/rfc7009
- OAuth Token Introspection, RFC 7662: https://www.rfc-editor.org/rfc/rfc7662
- DPoP, RFC 9449: https://www.rfc-editor.org/rfc/rfc9449
- OpenID Connect Core 1.0: https://openid.net/specs/openid-connect-core-1_0.html
- OpenIddict degraded-mode overview: https://documentation.openiddict.com/introduction
- OpenIddict signing/encryption credential guidance: https://documentation.openiddict.com/configuration/encryption-and-signing-credentials
- OpenIddict assertion-based client authentication: https://documentation.openiddict.com/configuration/assertion-based-client-authentication

## Rule for checking items off

Do not check an item solely because code exists. An item is complete when:

1. the behavior is implemented;
2. negative/security tests exist;
3. deployment configuration is production-safe;
4. operational ownership and recovery are documented where applicable; and
5. the corresponding end-to-end scenario has been exercised against a production-like deployment.
