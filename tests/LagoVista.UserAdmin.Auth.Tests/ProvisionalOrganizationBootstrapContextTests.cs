using LagoVista.UserAdmin.Interfaces;
using NUnit.Framework;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class ProvisionalOrganizationBootstrapContextTests
    {
        [Test]
        public void FreshBootstrapProbes_Should_Be_OneShot_And_Exact()
        {
            const string organizationId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            const string userId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";

            ProvisionalOrganizationBootstrapContext.MarkFresh(organizationId, userId);

            Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeRoleProbe("OTHER", userId), Is.False);
            Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeRoleProbe(organizationId, userId), Is.True);
            Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeRoleProbe(organizationId, userId), Is.False);

            Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeMembershipProbe(organizationId, "OTHER"), Is.False);
            Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeMembershipProbe(organizationId, userId), Is.True);
            Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeMembershipProbe(organizationId, userId), Is.False);
        }
    }
}
