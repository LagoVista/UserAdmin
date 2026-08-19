# Grafana OIDC Pilot

This pilot makes the existing NuvIoT authorization server usable as the identity provider for Grafana before applying the same pattern to cluster administration tools such as Headlamp.

## Goal

Prove this browser flow with a real NuvIoT user account:

```text
Grafana
  -> NuvIoT /connect/authorize
  -> existing NuvIoT login
  -> authorization code + PKCE
  -> NuvIoT /connect/token
  -> NuvIoT /connect/userinfo
  -> Grafana session
```

This is an infrastructure/operator identity pilot. It does not replace the existing NuvIoT application authentication model.

## Authorization server support

The authorization server now exposes:

```text
/.well-known/openid-configuration
/connect/authorize
/connect/token
/connect/userinfo
```

The Grafana pilot uses authorization code flow with S256 PKCE. The authorization endpoint accepts zero or one OAuth resource. Existing clients that send a registered resource retain their current validation behavior; OIDC clients such as Grafana can omit the resource parameter.

The standard `profile` and `email` scopes are registered by the authorization server. When requested, the resulting access token/userinfo response exposes the NuvIoT user's stable subject, display name, and email address.

## Grafana client registration

Create an active OAuth client with values equivalent to:

```text
Name: NuvIoT Dev Grafana
ClientId: nuviot-dev-grafana
ClientType: public
Status: active
RequirePkce: true
RequireConsent: false
RedirectUris:
  https://dev-grafana.nuviot.com/login/generic_oauth
AllowedGrantTypes:
  authorization_code
AllowedScopes:
  openid
  profile
  email
AllowedResources:
  <one valid placeholder/resource value required by the current model validation>
```

The redirect URI comparison is exact.

The current client model still requires at least one `AllowedResources` entry even though Grafana does not need to send a `resource` parameter. That model constraint can be relaxed later if OIDC-only clients become common.

## Grafana configuration shape

Grafana Generic OAuth should use the NuvIoT issuer endpoints and PKCE:

```ini
[auth.generic_oauth]
enabled = true
name = NuvIoT
allow_sign_up = true
use_pkce = true
scopes = openid profile email
client_id = nuviot-dev-grafana
auth_url = https://<nuviot-authority>/connect/authorize
token_url = https://<nuviot-authority>/connect/token
api_url = https://<nuviot-authority>/connect/userinfo
login_attribute_path = sub
name_attribute_path = name
email_attribute_path = email
```

Keep the existing Grafana local administrator login enabled during the pilot as the recovery path.

## Important current limitation

The authorization server currently runs OpenIddict in degraded mode with `AcceptAnonymousClients()`. Confidential-client secret validation is not yet implemented. For the first Grafana proof, treat Grafana as a PKCE-protected public client and do not claim confidential-client support.

Before broader production use, production signing-key/certificate ownership and recovery must also be made explicit. Development certificates are not an acceptable production key-management strategy.

## Validation sequence

1. Build and deploy the updated authorization-server package/host to development.
2. Confirm `/.well-known/openid-configuration` advertises the userinfo endpoint.
3. Register the `nuviot-dev-grafana` OAuth client.
4. Enable Generic OAuth in development Grafana while keeping local login available.
5. Sign out of Grafana and select the NuvIoT login option.
6. Authenticate with a normal NuvIoT user.
7. Confirm Grafana creates/resolves the user using the stable `sub` claim and receives name/email.
8. Verify local Grafana administrator login still works as rollback.

Once this succeeds, use the same NuvIoT identity-provider foundation for the Headlamp/Kubernetes authorization design.
