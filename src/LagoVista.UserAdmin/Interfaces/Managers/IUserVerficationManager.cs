// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f98615408e602e9730b499eae7a77ea028d25c5643de2ca6ae7f9c2da27458e7
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IUserVerficationManager
    {
        Task<InvokeResult> CheckConfirmedAsync(EntityHeader userHeader);
        Task<InvokeResult<string>> SendConfirmationEmailAsync(string userId, string confirmSubject = "", string confirmBody = "", string appName = "", string logoFile = "");
        Task<InvokeResult<string>> SendConfirmationEmailAsync(AppUser appUser, string confirmSubject = "", string confirmBody = "", string appName = "", string logoFile = "");
        Task<InvokeResult<string>> SendSMSCodeAsync(VerfiyPhoneNumber sendSMSCode, EntityHeader userHeader);
        Task<InvokeResult> ValidateSMSAsync(VerfiyPhoneNumber verifyRequest, EntityHeader userHeader);
        Task<InvokeResult> ValidateEmailAsync(ConfirmEmail confirmemaildto, EntityHeader userHeader);
        Task<InvokeResult> SetUserSMSValidated(string userId, EntityHeader userHeader);
    }
}
