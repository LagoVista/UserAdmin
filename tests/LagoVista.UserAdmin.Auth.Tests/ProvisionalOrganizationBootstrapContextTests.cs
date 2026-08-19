using LagoVista.UserAdmin.Interfaces;
using NUnit.Framework;
using System.Diagnostics;

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

            using (var activity = new Activity("provisional-bootstrap-test").Start())
            {
                ProvisionalOrganizationBootstrapContext.MarkFresh(organizationId, userId);

                Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeRoleProbe("OTHER", userId), Is.False);
                Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeRoleProbe(organizationId, userId), Is.True);
                Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeRoleProbe(organizationId, userId), Is.False);

                Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeMembershipProbe(organizationId, "OTHER"), Is.False);
                Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeMembershipProbe(organizationId, userId), Is.True);
                Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeMembershipProbe(organizationId, userId), Is.False);
            }
        }

        [Test]
        public void FreshBootstrapProbes_Should_Not_Cross_Request_Trace()
        {
            const string organizationId = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
            const string userId = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";

            using (var firstActivity = new Activity("provisional-bootstrap-first").Start())
            {
                ProvisionalOrganizationBootstrapContext.MarkFresh(organizationId, userId);
            }

            using (var secondActivity = new Activity("provisional-bootstrap-second").Start())
            {
                Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeRoleProbe(organizationId, userId), Is.False);
                Assert.That(ProvisionalOrganizationBootstrapContext.TryConsumeMembershipProbe(organizationId, userId), Is.False);
            }
        }
    }
}
