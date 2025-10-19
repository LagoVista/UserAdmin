// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 268a2a60168ea90a3c36002ca1232f94ef5255292c3f224b4e411678aa82bfc7
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IUserVerficationManager
    {
        Task<InvokeResult> CheckConfirmedAsync(EntityHeader userHeader);
        Task<InvokeResult<string>> SendConfirmationEmailAsync(EntityHeader userHeader, string confirmSubject = "", string confirmBody = "", string appName = "", string logoFile = "");
        Task<InvokeResult<string>> SendSMSCodeAsync(VerfiyPhoneNumber sendSMSCode, EntityHeader userHeader);
        Task<InvokeResult> ValidateSMSAsync(VerfiyPhoneNumber verifyRequest, EntityHeader userHeader);
        Task<InvokeResult> ValidateEmailAsync(ConfirmEmail confirmemaildto, EntityHeader userHeader);
        Task<InvokeResult> SetUserSMSValidated(string userId, EntityHeader userHeader);
    }
}
