using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class InvitationAcceptanceFlowIntegrationTests
    {
        private const string InvitationAcceptanceEvidence = "auth|auth.test-binding.invitation.accept|auth.flow.invitation.accept|auth.transition.invitation.accept";
        private const string InviteId = "invite-123";
        private const string UserId = "user-123";

        [Test]
        [Property("AptixEvidence", InvitationAcceptanceEvidence)]
        public async Task SuccessfulAcceptance_Should_AddMembership_ConsumeInvitation_And_RecordMilestones()
        {
            var invitationAcceptanceService = new Mock<IInvitationAcceptanceService>(MockBehavior.Strict);
            var response = new AcceptInviteResponse
            {
                RedirectPage = "/invite/accepted",
                ResponseMessage = "Invitation accepted."
            };

            invitationAcceptanceService
                .Setup(service => service.AcceptInvitationAsync(InviteId, UserId))
                .ReturnsAsync(InvokeResult<AcceptInviteResponse>.Create(response));

            var flowService = CreateFlowService(invitationAcceptanceService.Object);

            var result = await flowService.AcceptInvitationAsync(InviteId, UserId);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(response));
            Assert.That(result.Result.RedirectPage, Is.EqualTo("/invite/accepted"));
            invitationAcceptanceService.Verify(service => service.AcceptInvitationAsync(InviteId, UserId), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", InvitationAcceptanceEvidence)]
        public async Task InactiveInvitation_Should_ReturnFailure_WithoutCreatingMembership()
        {
            var invitationAcceptanceService = new Mock<IInvitationAcceptanceService>(MockBehavior.Strict);
            var failedResult = InvokeResult<AcceptInviteResponse>.FromErrors(new ErrorMessage("Invitation is not active."));

            invitationAcceptanceService
                .Setup(service => service.AcceptInvitationAsync(InviteId, UserId))
                .ReturnsAsync(failedResult);

            var flowService = CreateFlowService(invitationAcceptanceService.Object);

            var result = await flowService.AcceptInvitationAsync(InviteId, UserId);

            Assert.That(result.Successful, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Invitation is not active"));
            invitationAcceptanceService.Verify(service => service.AcceptInvitationAsync(InviteId, UserId), Times.Once);
        }

        private static AuthenticationFlowService CreateFlowService(IInvitationAcceptanceService invitationAcceptanceService)
        {
            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryRequestHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var recoveryCompletionHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryCompletionFlowRequest>>(MockBehavior.Strict);
            var invitationAcceptanceHandler = new InvitationAcceptanceFlowHandler(invitationAcceptanceService);

            return new AuthenticationFlowService(passwordLoginHandler.Object, recoveryRequestHandler.Object, recoveryCompletionHandler.Object, invitationAcceptanceHandler);
        }
    }
}
