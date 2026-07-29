using LagoVista.UserAdmin.Models.Auth;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class PasswordRecoveryRequestFlowRequest
    {
        public PasswordRecoveryRequestFlowRequest(SendResetPasswordLink request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public SendResetPasswordLink Request { get; }
    }
}
