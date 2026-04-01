using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IExternalLoginManager
    {
        Task<ExternalLogin> GetExternalLoginAsync(ExternalLoginInfo loginInfo);
        Task<InvokeResult<string>> AssociateExistingUserAsync(ExternalLogin externalLoginInfo, Dictionary<string, string> cookies, AppUser appUser, string inviteId, string returnUrl = null);
        Task<InvokeResult<string>> HandleExternalLogin(ExternalLogin externalLoginInfo, Dictionary<string, string> cookies, string inviteId, string returnUrl = null);
    }
}
