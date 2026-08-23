using LagoVista.AspNetCore.AuthorizationServer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class OidcTeamRoleArrayTokenHandlerTests
    {
        [Test]
        public void RestoreTeamRoleArray_SingleRole_SerializesAsJsonArray()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("sub", "USER123"),
                new Claim(AuthorizationServerConstants.ClaimTeamRole, AuthorizationServerConstants.TeamRoleOwner),
            }, "test");
            var principal = new ClaimsPrincipal(identity);

            OidcTeamRoleArrayTokenHandler.RestoreTeamRoleArray(principal);

            var teamRoleClaim = principal.FindFirst(AuthorizationServerConstants.ClaimTeamRole);
            Assert.That(teamRoleClaim, Is.Not.Null);
            Assert.That(teamRoleClaim.ValueType, Is.EqualTo(JsonClaimValueTypes.JsonArray));

            var handler = new JsonWebTokenHandler();
            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = identity,
            });

            var parts = token.Split('.');
            Assert.That(parts.Length, Is.EqualTo(3));

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload += payload.Length % 4 switch
            {
                0 => String.Empty,
                2 => "==",
                3 => "=",
                _ => throw new InvalidOperationException("Invalid base64url payload."),
            };

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var document = JsonDocument.Parse(json);

            Assert.That(document.RootElement.TryGetProperty(
                AuthorizationServerConstants.ClaimTeamRole,
                out var teamRole), Is.True);
            Assert.That(teamRole.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(teamRole.GetArrayLength(), Is.EqualTo(1));
            Assert.That(teamRole[0].GetString(), Is.EqualTo(AuthorizationServerConstants.TeamRoleOwner));
        }
    }
}
