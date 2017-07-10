using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IPasswordManager
    {
        Task<InvokeResult> ResetPasswordAsync(ResetPassword resetPassword);
        Task<InvokeResult> ChangePasswordAsync(ChangePassword changePasswordDTO, EntityHeader orgEntityHeader, EntityHeader userEntityHeader);
        Task<InvokeResult> SendResetPasswordLinkAsync(SendResetPasswordLink sendResetPasswordLink);
    }
}
