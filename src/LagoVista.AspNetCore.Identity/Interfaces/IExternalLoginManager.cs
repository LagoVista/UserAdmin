// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b8bfa8bfba8441515dfb71da88f8722d156b42c9faa50470574b261b68d0eb00
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IExternalLoginManager
    {
        Task<ExternalLogin> GetExternalLoginAsync(ExternalLoginInfo loginInfo);
        Task<InvokeResult<string>> AssociateExistingUserAsync(ExternalLogin externalLoginInfo, Dictionary<string, string> cookies, AppUser appUser, string inviteId, string returnUrl = null);
        Task<InvokeResult<string>> HandleExternalLogin(ExternalLogin externalLoginInfo, Dictionary<string, string> cookies, string inviteId, string returnUrl = null);
    }
}
