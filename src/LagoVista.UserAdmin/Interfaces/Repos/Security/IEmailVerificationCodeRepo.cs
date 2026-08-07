using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IEmailVerificationCodeRepo
    {
        Task StoreAsync(EmailVerificationCode verificationCode);
        Task<EmailVerificationCode> GetLatestAsync(string userId);
        Task UpdateAsync(EmailVerificationCode verificationCode);
    }
}
