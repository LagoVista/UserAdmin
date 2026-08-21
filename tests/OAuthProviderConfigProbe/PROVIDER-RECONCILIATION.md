# OAuth Provider Manual Reconciliation

This page is the manual counterpart to `Test-OAuthProviderConfig.ps1`.

The PowerShell probe verifies that the effective NuvIoT configuration is present and that public provider endpoints are reachable. Provider-side registration state is intentionally reconciled manually because the administrative APIs and permissions differ substantially between providers.

## Reconciliation workflow

For the target deployment (`localdev`, `dev`, or `live`):

1. Retrieve the effective configuration from Config Server.
2. Open the provider administration page below.
3. Locate the application whose Client ID matches the configured Client ID.
4. Verify the configured secret/credential is still valid. Never copy secret values into this document, logs, tickets, or source control.
5. Verify the provider callback/redirect URL exactly matches the URL expected by NuvIoT for that deployment.
6. Verify the application is enabled and that its requested sign-in/product permissions are still appropriate.

## Microsoft

**Provider console:** https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade

Configuration keys:

- `OAuth:Microsoft:ClientId`
- `OAuth:Microsoft:Secret`
- `OAuth:Microsoft:SecretId`

Verify:

- Application (client) ID matches `OAuth:Microsoft:ClientId`.
- The credential identified by `OAuth:Microsoft:SecretId` exists and is not expired.
- The expected NuvIoT callback is present under the Web platform redirect URIs.
- Supported account types are still what NuvIoT expects.

Microsoft Entra stores OAuth/OIDC application registrations under **Entra ID > App registrations**.

## Google

**Provider console:** https://console.cloud.google.com/auth/clients

Configuration keys:

- `OAuth:Google:ClientId`
- `OAuth:Google:Secret`

Verify:

- The Web application Client ID matches `OAuth:Google:ClientId`.
- The client secret is active/current.
- The expected NuvIoT callback is present under **Authorized redirect URIs**.
- Authorized JavaScript origins are correct if the application uses them.

Google requires redirect URIs to be explicitly registered for the OAuth client.

## LinkedIn

**Provider console:** https://www.linkedin.com/developers/apps

Configuration keys:

- `OAuth:LinkedIn:ClientId`
- `OAuth:LinkedIn:Secret`

Verify:

- Client ID matches `OAuth:LinkedIn:ClientId`.
- Client secret is current.
- The expected NuvIoT callback is present under the app's **Auth** tab.
- **Sign In with LinkedIn using OpenID Connect** and the required scopes/products remain enabled.

LinkedIn requires configured redirect URLs to be absolute HTTPS URLs for normal web applications.

## GitHub

**Provider console:** https://github.com/settings/developers

Configuration keys:

- `OAuth:GitHub:ClientId`
- `OAuth:GitHub:Secret`

Verify:

- Client ID matches `OAuth:GitHub:ClientId`.
- Client secret is current.
- The expected NuvIoT authorization callback URL is configured.
- Callback wildcard matching, if enabled, is intentional and no broader than required.

GitHub OAuth Apps use the configured authorization callback URL when completing the web authorization flow.

## X / Twitter

**Provider console:** https://developer.x.com/en/portal/dashboard

Configuration keys:

- `OAuth:Twitter:ClientId`
- `OAuth:Twitter:Secret`

Verify:

- The configured application/client identifier matches `OAuth:Twitter:ClientId`.
- The configured secret/credential is current.
- The expected NuvIoT callback/redirect URL is registered.
- The app's OAuth version, permissions, and application type still match the NuvIoT integration.

## Suggested configuration hardening

To make callback expectations visible in the same effective configuration that supplies OAuth credentials, consider adding explicit return URL settings for each provider, for example:

- `OAuth:Microsoft:ReturnUrl`
- `OAuth:Google:ReturnUrl`
- `OAuth:LinkedIn:ReturnUrl`
- `OAuth:GitHub:ReturnUrl`
- `OAuth:Twitter:ReturnUrl`

The probe can then require these values and print them during reconciliation without needing provider-admin API access. The provider console remains the authoritative place to confirm that the same URL is registered.
