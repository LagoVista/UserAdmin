using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class InvitationAcceptanceFlowRequest
    {
        public InvitationAcceptanceFlowRequest(string inviteId, string userId)
        {
            if (String.IsNullOrWhiteSpace(inviteId)) throw new ArgumentNullException(nameof(inviteId));
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            InviteId = inviteId;
            UserId = userId;
        }

        public string InviteId { get; }
        public string UserId { get; }
    }
}
