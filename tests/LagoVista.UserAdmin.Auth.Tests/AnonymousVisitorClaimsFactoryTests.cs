using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Claims;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class AnonymousVisitorClaimsFactoryTests
    {
        [Test]
        public void GetClaimsForAnonymousVisitor_Should_Emit_Compatibility_And_Visitor_Claims()
        {
            var user = CreateSharedUser();
            var claims = new ClaimsFactory(new Mock<IAdminLogger>().Object).GetClaimsForAnonymousVisitor(user, "visitor-actor-id");

            Assert.That(claims.Single(claim => claim.Type == ClaimTypes.NameIdentifier).Value, Is.EqualTo(user.Id));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.CurrentUserId).Value, Is.EqualTo(user.Id));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.CurrentUserName).Value, Is.EqualTo(user.UserName));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.CurrentOrgId).Value, Is.EqualTo(user.CurrentOrganization.Id));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.CurrentOrgName).Value, Is.EqualTo(user.CurrentOrganization.Text));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.CurrentOrgNamespace).Value, Is.EqualTo(user.CurrentOrganization.Namespace.ToString()));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.Logintype).Value, Is.EqualTo(user.LoginType.ToString()));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.ActorId).Value, Is.EqualTo("visitor-actor-id"));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.IdentityStage).Value, Is.EqualTo(ClaimsFactory.VisitorIdentityStage));
            Assert.That(claims.Single(claim => claim.Type == ClaimsFactory.Anonymous).Value, Is.EqualTo(Boolean.TrueString));
        }

        [Test]
        public void GetClaimsForAnonymousVisitor_Should_Not_Inherit_Privileges_Or_Resource_Claims()
        {
            var user = CreateSharedUser();
            user.IsSystemAdmin = true;
            user.IsOrgAdmin = true;
            user.IsAppBuilder = true;
            user.IsFinanceAdmin = true;
            user.IsCustomerAdmin = true;
            user.IsPreviewUser = true;
            user.IsUserDevice = true;
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            user.TwoFactorEnabled = true;

            var claims = new ClaimsFactory(new Mock<IAdminLogger>().Object).GetClaimsForAnonymousVisitor(user, "visitor-actor-id");
            var restrictedFlags = new[]
            {
                ClaimsFactory.IsSystemAdmin,
                ClaimsFactory.IsOrgAdmin,
                ClaimsFactory.IsAppBuilder,
                ClaimsFactory.IsFinancceAdmin,
                ClaimsFactory.IsCustomerAdmin,
                ClaimsFactory.IsPreviewUser,
                ClaimsFactory.IsUserDevice,
                ClaimsFactory.ExternalAccountVerified,
                ClaimsFactory.EmailVerified,
                ClaimsFactory.PhoneVerfiied,
                ClaimsFactory.TwoFactorEnabled,
                ClaimsFactory.OrgRequireMfa,
            };

            foreach (var restrictedFlag in restrictedFlags)
            {
                Assert.That(claims.Single(claim => claim.Type == restrictedFlag).Value, Is.EqualTo(Boolean.FalseString));
            }

            Assert.That(claims.Any(claim => claim.Type == ClaimTypes.Role), Is.False);
            Assert.That(claims.Any(claim => claim.Type == ClaimsFactory.CustomerId), Is.False);
            Assert.That(claims.Any(claim => claim.Type == ClaimsFactory.CustomerContactId), Is.False);
            Assert.That(claims.Any(claim => claim.Type == ClaimsFactory.DeviceRepoId), Is.False);
            Assert.That(claims.Any(claim => claim.Type == ClaimsFactory.InstanceId), Is.False);
        }

        private static AppUser CreateSharedUser()
        {
            return new AppUser("anonymous@system.local", "anonymous-system-user", "system")
            {
                Id = "shared-anonymous-user-id",
                LoginType = LoginTypes.AppUser,
                CurrentOrganization = new OrganizationSummary
                {
                    Id = "shared-anonymous-org-id",
                    Text = "Anonymous Visitors",
                    Namespace = "anonymousvisitors",
                },
            };
        }
    }
}
