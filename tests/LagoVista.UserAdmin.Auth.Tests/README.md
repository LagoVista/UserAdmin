# LagoVista.UserAdmin.Auth.Tests

Focused executable tests for authentication and authorization behavior.

Run normal, database-free tests:

```powershell
dotnet test tests/LagoVista.UserAdmin.Auth.Tests/LagoVista.UserAdmin.Auth.Tests.csproj --filter "TestCategory!=Integration"
```

## Cassandra authorization-code reproduction (isolated only)

`OpenIddictCassandraTokenStoreIntegrationTests` exercises the real
`OpenIddictCassandraTokenStore.CreateAsync` / `UpdateAsync` behavior.

It intentionally does **not** connect to a database unless explicitly enabled.
Do not use a DEV/LIVE application keyspace. Provision a separate, disposable
test keyspace named `oauth_repro_*` and credentials scoped to that keyspace.
Provisioning the keyspace and configuring appropriate replication are separate
operator tasks; the test only creates its own protocol-token table/indexes
inside the specified keyspace.

PowerShell example (use a test-only Cassandra endpoint and test-only credentials):

```powershell
$env:OAUTH_REPRO_CONFIRM = "RUN_ISOLATED_OAUTH_REPRO"
$env:OAUTH_REPRO_KEYSPACE = "oauth_repro_authcas"
$env:OAUTH_REPRO_HOSTS = "YOUR_TEST_CASSANDRA_HOST"
$env:OAUTH_REPRO_PORT = "9042"
$env:OAUTH_REPRO_LOCAL_DC = "YOUR_DATACENTER"
$env:OAUTH_REPRO_USER = "YOUR_TEST_ONLY_USER"
# Set OAUTH_REPRO_PASSWORD securely in the local process environment.
dotnet test tests/LagoVista.UserAdmin.Auth.Tests/LagoVista.UserAdmin.Auth.Tests.csproj --filter "FullyQualifiedName~OpenIddictCassandraTokenStoreIntegrationTests"
```

Tests cover:
- New authorization code: create version 0, attach payload, update to version 1.
- A stale writer must fail with `ContentModifiedException` and not overwrite payload.
- A missing token must not be resurrected by conditional update.
- Thirty fresh create/update operations to expose recurring transient errors
  without running a concurrency load test.

Do not loosen the `IF version = ?` condition or retry token redemption
to make a test pass. Record the failing test, Cassandra exception type,
and `[applied]` outcome if available. Never record real token payloads,
authorization codes, credentials, or secrets in diagnostic output.

The normal test suite requires no database, external identity provider,
email service, or hosted ASP.NET runtime.
