using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Models.Auth;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class InvitationAcceptanceFlowHandler : IAuthenticationFlowHandler<InvitationAcceptanceFlowRequest, AcceptInviteResponse>
    {
        public const string TransitionKey = "auth.transition.invitation.accept";

        private readonly IInvitationAcceptanceService _invitationAcceptanceService;

        public InvitationAcceptanceFlowHandler(IInvitationAcceptanceService invitationAcceptanceService)
        {
            _invitationAcceptanceService = invitationAcceptanceService ?? throw new ArgumentNullException(nameof(invitationAcceptanceService));
        }

        public async Task<AuthenticationFlowResult<AcceptInviteResponse>> HandleAsync(InvitationAcceptanceFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _invitationAcceptanceService.AcceptInvitationAsync(request.InviteId, request.UserId);
            return new AuthenticationFlowResult<AcceptInviteResponse>(TransitionKey, result);
        }
    }
}
