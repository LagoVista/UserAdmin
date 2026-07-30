using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface IInvitationAcceptanceService
    {
        Task<InvokeResult<AcceptInviteResponse>> AcceptInvitationAsync(string inviteId, string userId);
    }
}
