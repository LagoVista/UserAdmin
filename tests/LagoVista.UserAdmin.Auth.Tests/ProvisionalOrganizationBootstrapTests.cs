using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class ProvisionalOrganizationBootstrapTests
    {
        [Test]
        public async Task CreateProvisionalOrganizationAsync_Should_Bootstrap_First_User_Without_Rereading_Organization()
        {
            const string appUserId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            const string organizationId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";

            var organizationRepo = new Mock<IOrganizationRepo>();
            var orgUserRepo = new Mock<IOrgUserRepo>();
            var appUserRepo = new Mock<IAppUserRepo>();
            var userRoleManager = new Mock<IUserRoleManager>();
            var coreAppServices = new Mock<ICoreAppServices>();

            organizationRepo
                .Setup(repo => repo.QueryOrganizationExistAsync(organizationId))
                .ReturnsAsync(false);
            organizationRepo
                .Setup(repo => repo.AddOrganizationAsync(It.IsAny<Organization>()))
                .Returns(Task.CompletedTask);
            orgUserRepo
                .Setup(repo => repo.QueryOrgHasUserAsync(organizationId, appUserId))
                .ReturnsAsync(false);
            orgUserRepo
                .Setup(repo => repo.AddOrgUserAsync(It.IsAny<OrgUser>()))
                .Returns(Task.CompletedTask);
            appUserRepo
                .Setup(repo => repo.UpdateAsync(It.IsAny<AppUser>()))
                .Returns(Task.CompletedTask);
            userRoleManager
                .Setup(manager => manager.UserHasRoleAsync(It.IsAny<string>(), appUserId, organizationId))
                .ReturnsAsync(true);
            coreAppServices
                .SetupGet(services => services.Logger)
                .Returns(Mock.Of<ILogger>());

            var manager = new OrgManager(
                organizationRepo.Object,
                Mock.Of<IOrgLocationRepo>(),
                orgUserRepo.Object,
                appUserRepo.Object,
                Mock.Of<IInviteUserRepo>(),
                Mock.Of<ILocationUserRepo>(),
                Mock.Of<ILocationRoleRepo>(),
                Mock.Of<IEmailSender>(),
                Mock.Of<IOrgInitializer>(),
                new DefaultRoleList(),
                Mock.Of<IOwnedObjectRepo>(),
                Mock.Of<IUserRoleRepo>(),
                userRoleManager.Object,
                Mock.Of<IAuthenticationLogManager>(),
                Mock.Of<ISubscriptionManager>(),
                Mock.Of<ILocationDiagramRepo>(),
                Mock.Of<IRoleRepo>(),
                Mock.Of<ISecureStorage>(),
                coreAppServices.Object,
                Mock.Of<IOrgInformationSource>());

            var appUser = new AppUser(null, "provisional-user", "Provisional Environment")
            {
                Id = appUserId,
                IsAnonymous = true,
                Organizations = new List<LagoVista.Core.Models.EntityHeader>()
            };

            var result = await manager.CreateProvisionalOrganizationAsync(appUser, organizationId);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.Id.ToString(), Is.EqualTo(organizationId));
            Assert.That(result.Result.Namespace, Is.EqualTo($"provisional{organizationId.ToLowerInvariant()}"));
            Assert.That(result.Result.DefaultVectorCollectionName, Is.EqualTo($"{result.Result.Namespace}-indexes"));
            Assert.That(appUser.CurrentOrganization.Id.ToString(), Is.EqualTo(organizationId));
            Assert.That(appUser.Organizations, Has.Count.EqualTo(1));
            Assert.That(appUser.Organizations[0].Id.ToString(), Is.EqualTo(organizationId));

            organizationRepo.Verify(repo => repo.AddOrganizationAsync(It.Is<Organization>(org => org.Id.ToString() == organizationId)), Times.Once);
            organizationRepo.Verify(repo => repo.GetOrganizationAsync(It.IsAny<string>()), Times.Never);
            orgUserRepo.Verify(repo => repo.AddOrgUserAsync(It.Is<OrgUser>(orgUser =>
                orgUser.OrgId.ToString() == organizationId &&
                orgUser.UserId.ToString() == appUserId &&
                orgUser.IsOrgAdmin &&
                orgUser.IsAppBuilder)), Times.Once);
        }
    }
}
