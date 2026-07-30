using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.DTOs;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class EmailVerificationFlowRequest
    {
        public EmailVerificationFlowRequest(ConfirmEmail request, EntityHeader user)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public ConfirmEmail Request { get; }
        public EntityHeader User { get; }
    }
}
