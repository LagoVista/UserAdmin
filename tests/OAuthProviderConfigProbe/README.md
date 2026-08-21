# OAuth Provider Configuration Probe

`Test-OAuthProviderConfig.ps1` checks the effective OAuth configuration returned by the NuvIoT Config Server and verifies that the public Microsoft and Google OpenID metadata/JWKS endpoints are healthy.

## Authentication

Set the Config Server auth token for each deployment you want to probe:

```powershell
$env:CFG_SRVR_LOCALDEV = '<token>'
$env:CFG_SRVR_DEV = '<token>'
$env:CFG_SRVR_LIVE = '<token>'
```

The script sends the selected value in the `x-config-auth` request header.

## Usage

LocalDev, all providers, default app key `web`:

```powershell
./Test-OAuthProviderConfig.ps1
```

Development Microsoft only:

```powershell
./Test-OAuthProviderConfig.ps1 -Deployment dev -Provider microsoft
```

All deployments and providers:

```powershell
./Test-OAuthProviderConfig.ps1 -Deployment all -Provider all
```

Override the application key:

```powershell
./Test-OAuthProviderConfig.ps1 -Deployment live -AppKey another-app
```

## Config Server request

The effective configuration is retrieved from:

```text
https://config.nuviot.com/api/config/{appKey}/{deploymentKey}
```

where both route values are URI escaped before the request is made.

## Current checks

For each requested deployment/provider the probe verifies:

- Config Server is reachable and accepts the environment-specific auth token.
- A Microsoft/Google OAuth configuration section can be located in the returned JSON.
- Client ID is configured.
- Client secret is configured. The value is never printed.
- Provider discovery metadata is reachable.
- Authorization and token endpoints are advertised.
- JWKS is reachable and contains signing keys.

The provider configuration lookup is intentionally tolerant of the current JSON shape. Once the exact effective configuration shape is confirmed in each environment, provider-specific assertions such as redirect URI, tenant/account type, and provider-side app-registration comparison can be tightened without changing the command-line contract.

The process exits with code `0` when all requested checks pass and `1` when any check fails.
