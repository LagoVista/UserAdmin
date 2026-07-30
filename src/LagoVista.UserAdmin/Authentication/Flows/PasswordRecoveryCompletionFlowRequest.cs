using LagoVista.UserAdmin.Models.DTOs;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class PasswordRecoveryCompletionFlowRequest
    {
        public PasswordRecoveryCompletionFlowRequest(ResetPassword request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public ResetPassword Request { get; }
    }
}
