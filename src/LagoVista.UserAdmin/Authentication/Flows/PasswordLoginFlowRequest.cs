using LagoVista.UserAdmin.Models.Auth;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class PasswordLoginFlowRequest
    {
        public PasswordLoginFlowRequest(AuthLoginRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public AuthLoginRequest Request { get; }
    }
}
