using LagoVista.UserAdmin.Models.DTOs;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class PasswordRecoveryVerificationFlowRequest
    {
        public PasswordRecoveryVerificationFlowRequest(VerifyPasswordResetCode request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public VerifyPasswordResetCode Request { get; }
    }
}
