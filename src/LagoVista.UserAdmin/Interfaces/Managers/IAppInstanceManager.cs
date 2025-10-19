// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 57a62038498d799fafd6a258969adedd1a09bb16bc8a3d9f415250d4507353cd
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Apps;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAppInstanceManager
    {
        Task<InvokeResult<AppInstance>> CreateForUserAsync(string appUserId, AuthRequest authRequest);
        Task<InvokeResult<AppInstance>> UpdateLastLoginAsync(string appUserId, AuthRequest authRequeset);
        Task<InvokeResult<AppInstance>> UpdateLastAccessTokenRefreshAsync(string appUserId, AuthRequest authRequest);
    }
}
