using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IAnonymousVisitorBootstrapManager
    {
        Task<InvokeResult<AnonymousVisitorBootstrapResponse>> BootstrapAsync(AnonymousVisitorBootstrapRequest request);
        Task<InvokeResult<AnonymousVisitorBootstrapResponse>> RestoreAsync(AnonymousVisitorRestoreRequest request);
    }
}
