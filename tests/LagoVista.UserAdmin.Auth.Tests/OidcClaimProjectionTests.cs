using LagoVista.AspNetCore.AuthorizationServer;
using LagoVista.AspNetCore.Identity.Managers;
using NUnit.Framework;
using System.Security.Claims;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class OidcClaimProjectionTests
    {
        [Test]
        public void FromPrincipal_ProjectsProfileEmailUsernameAndSystemAdmin()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "USER123"),
                new Claim(ClaimTypes.GivenName, "Kevin"),
                new Claim(ClaimTypes.Surname, "Wolf"),
                new Claim(ClaimTypes.Email, "KEVINW@SLSYS.NET"),
                new Claim(ClaimsFactory.CurrentUserName, "kevinw"),
                new Claim(ClaimsFactory.IsSystemAdmin, "True"),
                new Claim(ClaimsFactory.CurrentOrgId, "ORG123"),
                new Claim(ClaimsFactory.CurrentOrgName, "Software Logistics"),
                new Claim(ClaimsFactory.CurrentOrgNamespace, "softwarelogistics"),
                new Claim(ClaimsFactory.IsAppBuilder, "True"),
                new Claim(ClaimsFactory.EmailVerified, "True"),
            }, "test");

            var projection = OidcClaimProjection.FromPrincipal(new ClaimsPrincipal(identity));

            Assert.That(projection.Subject, Is.EqualTo("USER123"));
            Assert.That(projection.Name, Is.EqualTo("Kevin Wolf"));
            Assert.That(projection.GivenName, Is.EqualTo("Kevin"));
            Assert.That(projection.Surname, Is.EqualTo("Wolf"));
            Assert.That(projection.PreferredUsername, Is.EqualTo("kevinw"));
            Assert.That(projection.Email, Is.EqualTo("KEVINW@SLSYS.NET"));
            Assert.That(projection.IsSystemAdmin, Is.EqualTo("True"));
            Assert.That(projection.CurrentOrgId, Is.EqualTo("ORG123"));
            Assert.That(projection.CurrentOrgName, Is.EqualTo("Software Logistics"));
            Assert.That(projection.CurrentOrgNamespace, Is.EqualTo("softwarelogistics"));
            Assert.That(projection.IsAppBuilder, Is.EqualTo("True"));
            Assert.That(projection.EmailVerified, Is.EqualTo("True"));
        }

        [Test]
        public void FromPrincipal_IgnoresPlaceholderNamesAndFallsBackToUsername()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "USER123"),
                new Claim(ClaimTypes.GivenName, ClaimsFactory.None),
                new Claim(ClaimTypes.Surname, ClaimsFactory.None),
                new Claim(ClaimsFactory.CurrentUserName, "kevinw"),
            }, "test");

            var projection = OidcClaimProjection.FromPrincipal(new ClaimsPrincipal(identity));

            Assert.That(projection.Name, Is.EqualTo("kevinw"));
            Assert.That(projection.PreferredUsername, Is.EqualTo("kevinw"));
        }

        [Test]
        public void FromPrincipal_IgnoresMissingOrPlaceholderOrganization()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "USER123"),
                new Claim(ClaimsFactory.CurrentOrgId, ClaimsFactory.None),
                new Claim(ClaimsFactory.CurrentOrgName, ClaimsFactory.None),
            }, "test");

            var projection = OidcClaimProjection.FromPrincipal(new ClaimsPrincipal(identity));
            Assert.That(projection.CurrentOrgId, Is.Null);
            Assert.That(projection.CurrentOrgName, Is.Null);
        }

        [Test]
        public void GetTeamRole_SystemAdminWithTeamRoleScope_ReturnsOwner()
        {
            var role = OidcTeamRoleProjection.GetTeamRole(
                new[] { "openid", "profile", "email", AuthorizationServerConstants.ScopeTeamRole },
                "True");

            Assert.That(role, Is.EqualTo(AuthorizationServerConstants.TeamRoleOwner));
        }

        [Test]
        public void GetTeamRole_SystemAdminWithoutTeamRoleScope_ReturnsNull()
        {
            var role = OidcTeamRoleProjection.GetTeamRole(
                new[] { "openid", "profile", "email" },
                "True");

            Assert.That(role, Is.Null);
        }

        [Test]
        public void GetTeamRole_NonSystemAdminWithTeamRoleScope_ReturnsNull()
        {
            var role = OidcTeamRoleProjection.GetTeamRole(
                new[] { "openid", AuthorizationServerConstants.ScopeTeamRole },
                "False");

            Assert.That(role, Is.Null);
        }

        [Test]
        public void GetTeamRole_MalformedSystemAdminClaim_ReturnsNull()
        {
            var role = OidcTeamRoleProjection.GetTeamRole(
                new[] { "openid", AuthorizationServerConstants.ScopeTeamRole },
                "not-a-bool");

            Assert.That(role, Is.Null);
        }
    }
}
