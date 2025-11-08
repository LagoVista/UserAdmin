// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: dcd7d6ef7df8078957af1f8cb60f80d144dc9a9327305435d2f08ce2c4ded9bc
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IOrgHelper
    {
        Task<InvokeResult> SetUserOrgAsync(AuthRequest authRequest, AppUser appUser);
    }
}
