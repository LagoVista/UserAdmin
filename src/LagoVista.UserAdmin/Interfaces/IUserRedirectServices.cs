using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IUserRedirectServices
    {
        Task<InvokeResult<string>> IdentityDefaultRedirectAsync(AppUser user, string defaultRedirect = null);
    }
}
