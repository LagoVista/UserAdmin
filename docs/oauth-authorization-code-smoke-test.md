# OAuth Authorization Code Smoke Test

This smoke test proves the first OAuth vertical slice without requiring an MCP client.

## Host registration

Register the UserAdmin module first, followed by the authorization server module:

```csharp
services.AddUserAdminModule(configurationRoot, logger);

services.AddLagoVistaAuthorizationServer(options =>
{
    options.Issuer = new Uri("https://localhost:5001/");
    options.UseDevelopmentCertificates = true;
    options.DisableAccessTokenEncryption = true;
    options.Scopes.Add("knowledge.read");
});
```

The ASP.NET Core host must call authentication and authorization middleware after routing and before endpoint mapping:

```csharp
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

OpenIddict rejects non-HTTPS authorization-server requests by default. Use an HTTPS development endpoint.

## Development OAuth client

Create an active public `OAuthClientApplication` with these values:

```text
ClientId: lagovista-oauth-test
ClientType: public
Status: active
RequirePkce: true
RequireConsent: false
RedirectUris:
  http://127.0.0.1:8765/callback/
AllowedGrantTypes:
  authorization_code
AllowedScopes:
  knowledge.read
AllowedResources:
  https://localhost:5001/test-resource
```

The redirect URI comparison is exact, including scheme, host, port, path, and trailing slash.

## Run the flow

From the repository root in PowerShell 7:

```powershell
./tools/Test-OAuthAuthorizationCode.ps1 `
  -Authority https://localhost:5001 `
  -ClientId lagovista-oauth-test `
  -Resource https://localhost:5001/test-resource `
  -Scope knowledge.read
```

The harness:

1. creates a cryptographically random PKCE verifier and S256 challenge;
2. starts a localhost callback listener;
3. opens the browser at `/connect/authorize`;
4. uses the host's existing login challenge when no login cookie exists;
5. validates the returned `state` value;
6. exchanges the authorization code at `/connect/token`;
7. calls `/api/oauth/whoami` using the access token.

The final PowerShell object contains both the token response and the decoded identity returned by `whoami`.

## Expected discovery endpoint

The authorization server publishes metadata at:

```text
/.well-known/openid-configuration
```

The metadata should advertise `/connect/authorize`, `/connect/token`, authorization code flow, and S256 PKCE support.

## Deferred behavior

This slice does not yet implement:

- persistent consent or authorization grants;
- refresh tokens;
- confidential-client secret validation;
- revocation;
- production signing certificate resolution;
- MCP protected-resource metadata.
