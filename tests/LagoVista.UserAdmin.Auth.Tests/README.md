# LagoVista.UserAdmin.Auth.Tests

Focused executable tests for the authentication response-state contract defined by SEC-000004.

Run independently:

``powershell
dotnet test tests/LagoVista.UserAdmin.Auth.Tests/LagoVista.UserAdmin.Auth.Tests.csproj
``

This first slice tests deterministic response-state resolution only. It does not require a database, external identity provider, email service, or hosted ASP.NET runtime.
