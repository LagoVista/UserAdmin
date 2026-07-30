using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class InvitationAcceptanceFlowHandler : IAuthenticationFlowHandler<InvitationAcceptanceFlowRequest>
    {
        public const string TransitionKey = "auth.transition.invitation.accept";

        private readonly IOrganizationManager _organizationManager;

        public InvitationAcceptanceFlowHandler(IOrganizationManager organizationManager)
        {
            _organizationManager = organizationManager ?? throw new ArgumentNullException(nameof(organizationManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(InvitationAcceptanceFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _organizationManager.AcceptInvitationAsync(request.InviteId, request.UserId);
            return new AuthenticationFlowResult(TransitionKey, result);
        }
    }
}
