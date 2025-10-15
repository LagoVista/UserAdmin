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
