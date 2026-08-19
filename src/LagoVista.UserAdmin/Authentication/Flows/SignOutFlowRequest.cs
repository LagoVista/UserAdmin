using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Auth;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class SignOutFlowRequest
    {
        public SignOutFlowRequest(SignOutRequest request, EntityHeader organization, EntityHeader user)
        {
            Request = request ?? new SignOutRequest();
            Organization = organization;
            User = user;
        }

        public SignOutRequest Request { get; }
        public EntityHeader Organization { get; }
        public EntityHeader User { get; }
    }
}
