using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IPasswordResetCodeRepo
    {
        Task StoreAsync(PasswordResetCode resetCode);
        Task<PasswordResetCode> GetLatestAsync(string userId);
        Task UpdateAsync(PasswordResetCode resetCode);
        Task ClearAsync(string userId);
    }
}
