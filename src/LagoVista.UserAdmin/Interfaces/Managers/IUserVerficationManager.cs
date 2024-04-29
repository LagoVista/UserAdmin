using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IUserVerficationManager
    {
        Task<InvokeResult> CheckConfirmedAsync(EntityHeader orgHeader, EntityHeader userHeader);
        Task<InvokeResult<string>> SendConfirmationEmailAsync(EntityHeader orgHeader, EntityHeader userHeader);
        Task<InvokeResult<string>> SendSMSCodeAsync(VerfiyPhoneNumber sendSMSCode, EntityHeader orgHeader, EntityHeader userHeader);
        Task<InvokeResult> ValidateSMSAsync(VerfiyPhoneNumber verifyRequest, EntityHeader userHeader);
        Task<InvokeResult> ValidateEmailAsync(ConfirmEmail confirmemaildto, EntityHeader userHeader);
        Task<InvokeResult> SetUserSMSValidated(string userId, EntityHeader orgHeader, EntityHeader userHeader);
    }
}
