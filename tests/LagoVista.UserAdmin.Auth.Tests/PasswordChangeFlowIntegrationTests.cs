using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityUserManager = Microsoft.AspNetCore.Identity.UserManager<LagoVista.UserAdmin.Models.Users.AppUser>;
using LagoVistaIdentityUserManager = LagoVista.AspNetCore.Identity.Managers.UserManager;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class PasswordChangeFlowIntegrationTests
    {
        private const string SuccessEvidence = "auth|auth.test-binding.password-management.change|auth.flow.password-management.change|auth.transition.password-management.change-success";
        private const string FailedEvidence = "auth|auth.test-binding.password-management.change|auth.flow.password-management.change|auth.transition.password-management.change-failed";

        [Test]
        [Property("AptixEvidence", SuccessEvidence)]
        [Property("AptixAuthEvents", "ChangePasswordSuccess")]
        public async Task SuccessfulChange_Should_RunRealFlowAndManagers_AndEmitSuccessTransitionEvidence()
        {
            var harness = CreateHarness();
            var request = new ChangePassword { UserId = harness.User.Id, OldPassword = "old-password", NewPassword = "new-password" };

            harness.Validators.Setup(validator => validator.ValidatePasswordChangeRequest(request, harness.User.Id)).Returns(InvokeResult.Success);
            harness.IdentityUserManager.Setup(manager => manager.FindByIdAsync(harness.User.Id)).ReturnsAsync(harness.User);
            harness.IdentityUserManager.Setup(manager => manager.ChangePasswordAsync(harness.User, request.OldPassword, request.NewPassword)).ReturnsAsync(IdentityResult.Success);

            var result = await harness.FlowService.ChangePasswordAsync(request, harness.Organization, harness.UserHeader);

            Assert.That(result.Successful, Is.True);
            harness.IdentityUserManager.Verify(manager => manager.FindByIdAsync(harness.User.Id), Times.Once);
            harness.IdentityUserManager.Verify(manager => manager.ChangePasswordAsync(harness.User, request.OldPassword, request.NewPassword), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[] { AuthLogTypes.ChangePasswordSuccess }));
        }

        [Test]
        [Property("AptixEvidence", FailedEvidence)]
        [Property("AptixAuthEvents", "ChangePasswordFailed")]
        public async Task RejectedChange_Should_RunRealFlowAndManagers_AndEmitFailedTransitionEvidence()
        {
            var harness = CreateHarness();
            var request = new ChangePassword { UserId = harness.User.Id, OldPassword = "wrong-password", NewPassword = "new-password" };
            var identityFailure = IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." });

            harness.Validators.Setup(validator => validator.ValidatePasswordChangeRequest(request, harness.User.Id)).Returns(InvokeResult.Success);
            harness.IdentityUserManager.Setup(manager => manager.FindByIdAsync(harness.User.Id)).ReturnsAsync(harness.User);
            harness.IdentityUserManager.Setup(manager => manager.ChangePasswordAsync(harness.User, request.OldPassword, request.NewPassword)).ReturnsAsync(identityFailure);

            var result = await harness.FlowService.ChangePasswordAsync(request, harness.Organization, harness.UserHeader);

            Assert.That(result.Successful, Is.False);
            harness.IdentityUserManager.Verify(manager => manager.FindByIdAsync(harness.User.Id), Times.Once);
            harness.IdentityUserManager.Verify(manager => manager.ChangePasswordAsync(harness.User, request.OldPassword, request.NewPassword), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[] { AuthLogTypes.ChangePasswordFailed }));
            Assert.That(harness.Log.Events.Single().Extras, Is.EqualTo("Incorrect password."));
        }

        private static PasswordChangeHarness CreateHarness()
        {
            var log = new RecordingAuthenticationLogManager();
            var validators = new Mock<IAuthRequestValidators>(MockBehavior.Strict);
            var dependencyManager = new Mock<IDependencyManager>(MockBehavior.Loose);
            var security = new Mock<ISecurity>(MockBehavior.Loose);
            var adminLogger = new Mock<IAdminLogger>(MockBehavior.Loose);
            var appConfig = new Mock<IAppConfig>(MockBehavior.Loose);
            var passwordResetCodeRepo = new Mock<IPasswordResetCodeRepo>(MockBehavior.Loose);
            var emailSender = new Mock<IEmailSender>(MockBehavior.Loose);
            var identityUserManager = CreateIdentityUserManagerMock();
            var user = new AppUser("user@example.com", "test") { Id = "user-id", UserName = "user@example.com", Email = "user@example.com" };
            var organization = EntityHeader.Create("org-id", "Organization");
            var userHeader = EntityHeader.Create(user.Id, "User");

            var lagoVistaIdentityUserManager = new LagoVistaIdentityUserManager(identityUserManager.Object, log, adminLogger.Object, appConfig.Object, dependencyManager.Object, security.Object);
            var passwordManager = new PasswordManager(validators.Object, lagoVistaIdentityUserManager, emailSender.Object, passwordResetCodeRepo.Object, dependencyManager.Object, security.Object, log, adminLogger.Object, appConfig.Object);
            var passwordChangeHandler = new PasswordChangeFlowHandler(passwordManager);
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var flowService = new AuthenticationFlowService(passwordLoginHandler.Object, recoveryHandler.Object, passwordChangeHandler: passwordChangeHandler);

            return new PasswordChangeHarness
            {
                FlowService = flowService,
                Validators = validators,
                IdentityUserManager = identityUserManager,
                Log = log,
                User = user,
                Organization = organization,
                UserHeader = userHeader
            };
        }

        private static Mock<IdentityUserManager> CreateIdentityUserManagerMock()
        {
            var store = new Mock<IUserStore<AppUser>>(MockBehavior.Loose);
            return new Mock<IdentityUserManager>(
                store.Object,
                Options.Create(new IdentityOptions()),
                new Mock<IPasswordHasher<AppUser>>(MockBehavior.Loose).Object,
                Array.Empty<IUserValidator<AppUser>>(),
                Array.Empty<IPasswordValidator<AppUser>>(),
                new Mock<ILookupNormalizer>(MockBehavior.Loose).Object,
                new IdentityErrorDescriber(),
                new Mock<IServiceProvider>(MockBehavior.Loose).Object,
                new Mock<ILogger<IdentityUserManager>>(MockBehavior.Loose).Object);
        }

        private sealed class PasswordChangeHarness
        {
            public AuthenticationFlowService FlowService { get; set; }
            public Mock<IAuthRequestValidators> Validators { get; set; }
            public Mock<IdentityUserManager> IdentityUserManager { get; set; }
            public RecordingAuthenticationLogManager Log { get; set; }
            public AppUser User { get; set; }
            public EntityHeader Organization { get; set; }
            public EntityHeader UserHeader { get; set; }
        }
    }
}
