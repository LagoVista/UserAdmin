using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IUserVerficationManager
    {
        Task<InvokeResult> CheckConfirmedAsync(EntityHeader orgHeader, EntityHeader userHeader);
        Task<InvokeResult> SendConfirmationEmailAsync(EntityHeader orgHeader, EntityHeader userHeader);
        Task<InvokeResult> SendSMSCodeAsync(VerfiyPhoneNumber sendSMSCode, EntityHeader orgHeader, EntityHeader userHeader);
        Task<InvokeResult> ValidateSMSAsync(VerfiyPhoneNumber verifyRequest, EntityHeader orgHeader, EntityHeader userHeader);
        Task<InvokeResult> ValidateEmailAsync(ConfirmEmail confirmemaildto, EntityHeader orgHeader, EntityHeader userHeader);
    }
}
