using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.DTOs;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class PasswordChangeFlowRequest
    {
        public PasswordChangeFlowRequest(ChangePassword request, EntityHeader organization, EntityHeader user)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Organization = organization ?? throw new ArgumentNullException(nameof(organization));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public ChangePassword Request { get; }
        public EntityHeader Organization { get; }
        public EntityHeader User { get; }
    }
}
