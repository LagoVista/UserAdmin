using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class EmailVerificationSendFlowRequest
    {
        public EmailVerificationSendFlowRequest(EntityHeader user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public EntityHeader User { get; }
    }
}
