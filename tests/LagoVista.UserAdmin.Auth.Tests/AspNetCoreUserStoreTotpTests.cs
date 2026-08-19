using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class AspNetCoreUserStoreTotpTests
    {
        [Test]
        public async Task AuthenticatorKeyStore_Should_Resolve_Key_From_SecureStorage()
        {
            var userRepo = new Mock<IAppUserRepo>(MockBehavior.Strict);
            var secureStorage = new Mock<ISecureStorage>(MockBehavior.Strict);
            var user = new AppUser("user@example.com", "test")
            {
                UserName = "user@example.com",
                AuthenticatorKeySecretId = "totp-secret-id",
                AuthenticatorKey = null,
                TwoFactorEnabled = true
            };

            secureStorage
                .Setup(storage => storage.GetUserSecretAsync(It.IsAny<EntityHeader>(), "totp-secret-id"))
                .ReturnsAsync(InvokeResult<string>.Create("JBSWY3DPEHPK3PXP"));

            var store = new AspNetCoreUserStore(userRepo.Object, secureStorage.Object);

            Assert.That(store, Is.InstanceOf<IUserAuthenticatorKeyStore<AppUser>>());

            var key = await store.GetAuthenticatorKeyAsync(user, CancellationToken.None);

            Assert.That(key, Is.EqualTo("JBSWY3DPEHPK3PXP"));
            Assert.That(user.AuthenticatorKey, Is.Null);
            secureStorage.Verify(storage => storage.GetUserSecretAsync(It.IsAny<EntityHeader>(), "totp-secret-id"), Times.Once);
            secureStorage.VerifyNoOtherCalls();
            userRepo.VerifyNoOtherCalls();
        }
    }
}
